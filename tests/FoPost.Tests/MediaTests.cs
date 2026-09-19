using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class MediaTests
{
    private const string Presigned = """
    {
      "data": {
        "uploadId": "upl_1",
        "uploadUrl": "https://uploads.test.fopost.com/staging/upl_1?sig=abc",
        "method": "PUT",
        "headers": { "Content-Type": "image/png" },
        "expiresAt": "2026-09-19T12:00:00.000Z"
      }
    }
    """;

    private const string Completed = """
    {
      "data": {
        "id": "med_1",
        "type": "image",
        "name": "launch.png",
        "url": "https://api.test.fopost.com/v1/media/med_1/file",
        "previewUrl": "https://api.test.fopost.com/v1/media/med_1/file",
        "size": 4
      }
    }
    """;

    [Fact]
    public async Task Upload_direct_presigns_puts_the_bytes_and_completes()
    {
        var handler = new StubHandler()
            .Json(Presigned, HttpStatusCode.Created)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.OK))
            .Json(Completed, HttpStatusCode.Created);
        using var test = new TestClient(handler);
        var bytes = Encoding.ASCII.GetBytes("PNG!");

        var media = await test.Client.Media.UploadDirectAsync("ws_1", "launch.png", "image/png", bytes);

        Assert.Equal("med_1", media.Id);
        Assert.Equal("image", media.Type);
        Assert.Equal(4, media.Size);
        Assert.Equal(3, handler.Requests.Count);

        var presign = handler.Requests[0];
        Assert.Equal(HttpMethod.Post, presign.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/presign", presign.RequestUri!.ToString());
        Assert.Equal(TestClient.ApiKey, Assert.Single(presign.Headers.GetValues("X-API-Key")));
        var body = JsonDocument.Parse(handler.Bodies[0]!).RootElement;
        Assert.Equal("ws_1", body.GetProperty("workspaceId").GetString());
        Assert.Equal("launch.png", body.GetProperty("filename").GetString());
        Assert.Equal("image/png", body.GetProperty("mimeType").GetString());
        Assert.Equal(4, body.GetProperty("size").GetInt64());

        var put = handler.Requests[1];
        Assert.Equal(HttpMethod.Put, put.Method);
        Assert.Equal("https://uploads.test.fopost.com/staging/upl_1?sig=abc", put.RequestUri!.ToString());
        Assert.False(put.Headers.Contains("X-API-Key"));
        Assert.Null(put.Headers.Authorization);
        Assert.Equal("image/png", put.Content!.Headers.ContentType!.ToString());
        Assert.Equal(4, put.Content.Headers.ContentLength);
        Assert.Equal("PNG!", handler.Bodies[1]);

        var complete = handler.Requests[2];
        Assert.Equal(HttpMethod.Post, complete.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/presign/upl_1/complete", complete.RequestUri!.ToString());
        Assert.Null(handler.Bodies[2]);
    }

    [Fact]
    public async Task Upload_direct_throws_on_a_rejected_put_and_never_completes()
    {
        var handler = new StubHandler()
            .Json(Presigned, HttpStatusCode.Created)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent("<Error><Code>AccessDenied</Code></Error>", Encoding.UTF8, "application/xml"),
            });
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(() =>
            test.Client.Media.UploadDirectAsync("ws_1", "launch.png", "image/png", new byte[] { 1, 2, 3 }));

        Assert.Equal(403, error.Status);
        Assert.Contains("AccessDenied", error.Message);
        Assert.Equal(2, handler.Requests.Count);
    }
}
