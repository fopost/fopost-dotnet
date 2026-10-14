using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class PostFilesTests
{
    private const string Limits = """
    {"data":{"maxFileBytes":419430400,"multipartMaxFileBytes":104857600,"maxFilesPerUpload":5,"directUploadAvailable":true}}
    """;

    private const string Presigned = """
    {"data":{"uploadId":"upl_1","uploadUrl":"https://uploads.test.fopost.com/upl_1","method":"PUT","headers":{"Content-Type":"video/mp4"}}}
    """;

    private const string Completed = """
    {"data":{"id":"med_1","type":"video","name":"launch.mp4","url":"https://api.test.fopost.com/v1/media/med_1/file","size":8}}
    """;

    private static PostFile File(string name, string mime, string content) =>
        new(name, mime, new MemoryStream(Encoding.ASCII.GetBytes(content)), content.Length);

    private static PostFile LargeFile(string name, string mime, long length) =>
        new(name, mime, new MemoryStream(Encoding.ASCII.GetBytes("12345678")), length);

    [Fact]
    public async Task Create_sends_multipart_when_every_file_fits_the_ceiling()
    {
        var handler = new StubHandler()
            .Json(Limits)
            .Json($$"""{"data":{{Fixtures.Post}}}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var post = await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Content = new List<PostContent> { new("Hello from .NET") },
            Accounts = new List<string> { "acc_1" },
            Files = new List<PostFile> { File("launch.png", "image/png", "PNG!"), File("b.jpg", "image/jpeg", "JPEG") },
        });

        Assert.Equal("post_1", post.Id);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/limits?workspaceId=ws_1", handler.Requests[0].RequestUri!.ToString());

        var request = handler.Requests[1];
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/posts", request.RequestUri!.ToString());
        Assert.Equal(TestClient.ApiKey, Assert.Single(request.Headers.GetValues("X-API-Key")));
        Assert.Equal("multipart/form-data", request.Content!.Headers.ContentType!.MediaType);

        var parts = MultipartParts(handler.Bodies[1]!);
        Assert.Equal(3, parts.Count);
        Assert.Contains("name=\"post\"", parts[0]);
        var json = JsonDocument.Parse(parts[0][(parts[0].IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4)..]).RootElement;
        Assert.Equal("ws_1", json.GetProperty("workspace_id").GetString());
        Assert.Equal("Hello from .NET", json.GetProperty("content")[0].GetProperty("text").GetString());
        Assert.Equal("acc_1", json.GetProperty("accounts")[0].GetString());

        Assert.Contains("name=\"files\"; filename=\"launch.png\"", parts[1]);
        Assert.Contains("Content-Type: image/png", parts[1]);
        Assert.EndsWith("PNG!", parts[1]);
        Assert.Contains("name=\"files\"; filename=\"b.jpg\"", parts[2]);
        Assert.EndsWith("JPEG", parts[2]);
    }

    private static List<string> MultipartParts(string body)
    {
        var boundary = body[..body.IndexOf("\r\n", StringComparison.Ordinal)];
        return body.Split(boundary, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim('\r', '\n', '-'))
            .Where(part => part.Length > 0)
            .ToList();
    }

    [Fact]
    public async Task Create_falls_back_to_the_multipart_ceiling_when_limits_fail()
    {
        var handler = new StubHandler()
            .Json("""{"error":"internal","message":"boom"}""", HttpStatusCode.InternalServerError)
            .Json($$"""{"data":{{Fixtures.Post}}}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Content = new List<PostContent> { new("Hi") },
            Accounts = new List<string> { "acc_1" },
            Files = new List<PostFile> { File("launch.png", "image/png", "PNG!") },
        });

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("multipart/form-data", handler.Requests[1].Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Create_uploads_directly_when_a_file_is_over_the_multipart_ceiling()
    {
        var handler = new StubHandler()
            .Json(Limits)
            .Json(Presigned, HttpStatusCode.Created)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.OK))
            .Json(Completed, HttpStatusCode.Created)
            .Json($$"""{"data":{{Fixtures.Post}}}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Content = new List<PostContent> { new("Big launch") },
            Accounts = new List<string> { "acc_1" },
            Files = new List<PostFile> { LargeFile("launch.mp4", "video/mp4", 300L * 1024 * 1024) },
        });

        Assert.Equal(5, handler.Requests.Count);
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/presign", handler.Requests[1].RequestUri!.ToString());
        var presign = JsonDocument.Parse(handler.Bodies[1]!).RootElement;
        Assert.Equal("ws_1", presign.GetProperty("workspaceId").GetString());
        Assert.Equal(300L * 1024 * 1024, presign.GetProperty("size").GetInt64());
        Assert.Equal("https://uploads.test.fopost.com/upl_1", handler.Requests[2].RequestUri!.ToString());
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/presign/upl_1/complete", handler.Requests[3].RequestUri!.ToString());

        var create = handler.Requests[4];
        Assert.Equal(HttpMethod.Post, create.Method);
        Assert.Equal("application/json", create.Content!.Headers.ContentType!.MediaType);
        var body = JsonDocument.Parse(handler.Bodies[4]!).RootElement;
        var media = body.GetProperty("content")[0].GetProperty("media")[0];
        Assert.Equal("video", media.GetProperty("type").GetString());
        Assert.Equal("launch.mp4", media.GetProperty("name").GetString());
        Assert.Equal("https://api.test.fopost.com/v1/media/med_1/file", media.GetProperty("url").GetString());
        Assert.Equal(8, media.GetProperty("size").GetDouble());
    }

    [Fact]
    public async Task Update_with_a_large_file_fetches_the_post_and_appends_to_its_first_block()
    {
        var handler = new StubHandler()
            .Json(Limits)
            .Json($$"""{"data":{{Fixtures.Post}}}""")
            .Json(Presigned, HttpStatusCode.Created)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.OK))
            .Json(Completed, HttpStatusCode.Created)
            .Json($$"""{"data":{{Fixtures.Post}}}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.UpdateAsync("post_1", new UpdatePostOptions
        {
            Title = "Retitled",
            Files = new List<PostFile> { LargeFile("launch.mp4", "video/mp4", 300L * 1024 * 1024) },
        });

        Assert.Equal(6, handler.Requests.Count);
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/limits", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, handler.Requests[1].Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/posts/post_1", handler.Requests[1].RequestUri!.ToString());
        Assert.Equal("ws_1", JsonDocument.Parse(handler.Bodies[2]!).RootElement.GetProperty("workspaceId").GetString());

        var update = handler.Requests[5];
        Assert.Equal(HttpMethod.Put, update.Method);
        var body = JsonDocument.Parse(handler.Bodies[5]!).RootElement;
        Assert.Equal("Retitled", body.GetProperty("title").GetString());
        var block = body.GetProperty("content")[0];
        Assert.Equal("Hello from .NET", block.GetProperty("text").GetString());
        Assert.Equal("video", block.GetProperty("media")[0].GetProperty("type").GetString());
    }

    [Fact]
    public async Task Update_with_small_files_sends_multipart_to_the_post()
    {
        var handler = new StubHandler()
            .Json(Limits)
            .Json($$"""{"data":{{Fixtures.Post}}}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.UpdateAsync("post_1", new UpdatePostOptions
        {
            Files = new List<PostFile> { File("launch.png", "image/png", "PNG!") },
        });

        Assert.Equal(2, handler.Requests.Count);
        var request = handler.Requests[1];
        Assert.Equal(HttpMethod.Put, request.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/posts/post_1", request.RequestUri!.ToString());
        var parts = MultipartParts(handler.Bodies[1]!);
        Assert.Contains("name=\"post\"", parts[0]);
        Assert.EndsWith("{}", parts[0]);
        Assert.Contains("name=\"files\"; filename=\"launch.png\"", parts[1]);
    }

    [Fact]
    public async Task Oversize_on_the_post_endpoint_is_a_413_with_the_ceiling_readable()
    {
        var handler = new StubHandler()
            .Json(Limits)
            .Json("""
            {"error":"file_too_large","message":"Too big","max_file_bytes":104857600,"file_bytes":104857601,"upgrade_url":"https://app.test.fopost.com/settings/billing"}
            """, HttpStatusCode.RequestEntityTooLarge);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(() => test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Content = new List<PostContent> { new("Hi") },
            Accounts = new List<string> { "acc_1" },
            Files = new List<PostFile> { File("launch.png", "image/png", "PNG!") },
        }));

        Assert.Equal(413, error.Status);
        Assert.Equal("file_too_large", error.Code);
        Assert.Equal(104_857_600L, (long)error.Body!["max_file_bytes"]!);
        Assert.Equal(104_857_601L, (long)error.Body["file_bytes"]!);
    }

    [Fact]
    public async Task Multipart_is_rebuilt_for_a_retry_after_429()
    {
        var handler = new StubHandler()
            .Json(Limits)
            .TooManyRequests("1")
            .Json($$"""{"data":{{Fixtures.Post}}}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Content = new List<PostContent> { new("Hi") },
            Accounts = new List<string> { "acc_1" },
            Files = new List<PostFile> { File("launch.png", "image/png", "PNG!") },
        });

        Assert.Equal(3, handler.Requests.Count);
        Assert.Contains("PNG!", handler.Bodies[1]);
        Assert.Contains("PNG!", handler.Bodies[2]);
        Assert.Equal(TimeSpan.FromSeconds(1), Assert.Single(test.Waits));
    }
}
