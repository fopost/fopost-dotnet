using System.Net;
using Xunit;

namespace FoPost.Tests;

/// <summary>Fonts, video edits, and media alt text and file URLs.</summary>
public class CreateSuiteTests
{
    private const string Font = """
    {"id":"font_1","family":"Brand","style":"normal","weight":700,"format":"woff2","file_url":"/v1/fonts/font_1/file","created_at":"2026-11-01T00:00:00Z"}
    """;

    private const string Job = """
    {"id":"job_1","status":"succeeded","progress":1,"source_media_id":"media_1","operations":[{"op":"trim","params":{"startSec":2,"endSec":12}}],"result_media_id":"media_2","result_media":{"id":"media_2","preview_url":"/v1/media/media_2/file","mime_type":"video/mp4"},"extra_outputs":[{"media_id":"media_3","label":"Subtitles","mime":"text/vtt","preview_url":null}],"result":null,"error":null,"created_at":"2026-11-01T00:00:00Z","updated_at":"2026-11-01T00:01:00Z","completed_at":"2026-11-01T00:01:00Z"}
    """;

    [Fact]
    public async Task Fonts_list_reads_the_library_and_the_uploads()
    {
        var handler = new StubHandler().Json($$"""
        {"library":[{"slug":"geist","family":"Geist","category":"sans-serif","scripts":["latin"],"weights":[400,700],"files":[{"path":"/fonts/geist.woff2","subset":"latin","weight":400,"style":"normal","unicode_range":"U+0000-00FF"}]}],"uploaded":[{{Font}}]}
        """);
        using var test = new TestClient(handler);

        var fonts = await test.Client.Fonts.ListAsync("ws_1");

        Assert.Equal($"{TestClient.BaseUrl}/v1/fonts?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        var library = Assert.Single(fonts.Library);
        Assert.Equal(new[] { 400, 700 }, library.Weights);
        Assert.Equal("U+0000-00FF", Assert.Single(library.Files).UnicodeRange);
        Assert.Equal(700, Assert.Single(fonts.Uploaded).Weight);
    }

    [Fact]
    public async Task Fonts_upload_sends_a_multipart_form_and_rebuilds_it_on_retry()
    {
        var handler = new StubHandler().TooManyRequests("0").Json(Font, HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var font = await test.Client.Fonts.UploadAsync(
            "ws_1",
            "Brand-Bold.woff2",
            new byte[] { 0x77, 0x4F, 0x46, 0x32 },
            new UploadFontOptions { Family = "Brand", Weight = 700, Style = "normal" });

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/fonts", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("multipart/form-data", handler.LastRequest.Content!.Headers.ContentType!.MediaType);
        var body = handler.LastBody!;
        Assert.Contains("name=\"workspace_id\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"family\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"weight\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"file\"; filename=\"Brand-Bold.woff2\"", body, StringComparison.Ordinal);
        Assert.Contains("font/woff2", body, StringComparison.Ordinal);
        Assert.Contains("wOF2", body, StringComparison.Ordinal);
        Assert.Contains("wOF2", handler.Bodies[0]!, StringComparison.Ordinal);
        Assert.Equal("font_1", font.Id);
        Assert.Equal("woff2", font.Format);
    }

    [Fact]
    public async Task Fonts_upload_from_a_path_and_delete()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fopost-{Guid.NewGuid():N}.ttf");
        await File.WriteAllBytesAsync(path, new byte[] { 0, 1, 0, 0 });
        try
        {
            var handler = new StubHandler()
                .Json(Font, HttpStatusCode.Created)
                .Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
            using var test = new TestClient(handler);

            await test.Client.Fonts.UploadAsync("ws_1", path);
            Assert.Contains($"filename=\"{Path.GetFileName(path)}\"", handler.LastBody!, StringComparison.Ordinal);
            Assert.Contains("font/ttf", handler.LastBody!, StringComparison.Ordinal);
            Assert.DoesNotContain("name=\"family\"", handler.LastBody!, StringComparison.Ordinal);

            await test.Client.Fonts.DeleteAsync("font_1");
            Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
            Assert.Equal("/v1/fonts/font_1", handler.LastRequest.RequestUri!.AbsolutePath);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task Video_edit_ops_and_list()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"name":"trim","available":true,"uses_ai":false},{"name":"subtitles","available":true,"uses_ai":true}]}""")
            .Json($$"""{"data":[{{Job}}]}""");
        using var test = new TestClient(handler);

        var ops = await test.Client.VideoEdits.OpsAsync();
        Assert.Equal($"{TestClient.BaseUrl}/v1/video-edits/ops", handler.LastRequest.RequestUri!.ToString());
        Assert.True(ops[1].UsesAi);
        Assert.Equal(VideoEditOps.Subtitles, ops[1].Name);

        var jobs = await test.Client.VideoEdits.ListAsync(mediaId: "media_1", limit: 5);
        Assert.Equal($"{TestClient.BaseUrl}/v1/video-edits?media_id=media_1&limit=5", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("media_2", Assert.Single(jobs).ResultMediaId);
    }

    [Fact]
    public async Task Video_edit_create_and_get()
    {
        var handler = new StubHandler().Json(Job, HttpStatusCode.Created).Json(Job);
        using var test = new TestClient(handler);

        var job = await test.Client.VideoEdits.CreateAsync(
            "media_1",
            new[]
            {
                new VideoEditOperation(VideoEditOps.Trim, new Dictionary<string, object?> { ["startSec"] = 2, ["endSec"] = 12 }),
                new VideoEditOperation(VideoEditOps.Denoise),
            },
            workspaceId: "ws_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/video-edits", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(
            """{"media_id":"media_1","operations":[{"op":"trim","params":{"startSec":2,"endSec":12}},{"op":"denoise"}],"workspace_id":"ws_1"}""",
            handler.LastBody);
        Assert.Equal(VideoEditStatuses.Succeeded, job.Status);
        Assert.Equal("video/mp4", job.ResultMedia!.MimeType);
        Assert.Equal("text/vtt", Assert.Single(job.ExtraOutputs).Mime);
        Assert.Equal("12", Assert.Single(job.Operations).Params!["endSec"]!.ToString());

        await test.Client.VideoEdits.GetAsync("job_1", "ws_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/video-edits/job_1?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Media_alt_text_and_file_url()
    {
        var handler = new StubHandler()
            .Json("""{"alt_text":"A red bicycle against a wall","alt_text_source":"ai","tags":["bicycle","red"]}""")
            .Json("""{"url":"https://files.example.com/signed"}""");
        using var test = new TestClient(handler);

        var alt = await test.Client.Media.GenerateAltTextAsync("media_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/media/media_1/alt-text", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Null(handler.LastBody);
        Assert.Equal("A red bicycle against a wall", alt.AltText);
        Assert.Equal(new[] { "bicycle", "red" }, alt.Tags);

        var url = await test.Client.Media.FileUrlAsync("media_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/media_1/file?format=json", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("https://files.example.com/signed", url);
    }
}
