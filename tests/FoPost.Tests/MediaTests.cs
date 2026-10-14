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
    public async Task Limits_reads_the_workspace_ceilings()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "maxFileBytes": 419430400,
            "source": "accounts",
            "connectedAccounts": 5,
            "largeUploadsEnabled": false,
            "nextTier": { "upToAccounts": 15, "maxFileBytes": 2147483648 },
            "multipartMaxFileBytes": 104857600,
            "maxFilesPerUpload": 5,
            "directUploadAvailable": true,
            "allowedTypes": ["image/png", "video/mp4"]
          }
        }
        """);
        using var test = new TestClient(handler);

        var limits = await test.Client.Media.LimitsAsync("ws_1");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/media/limits?workspaceId=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(419_430_400L, limits.MaxFileBytes);
        Assert.Equal("accounts", limits.Source);
        Assert.Equal(5, limits.ConnectedAccounts);
        Assert.False(limits.LargeUploadsEnabled);
        Assert.Equal(15, limits.NextTier!.UpToAccounts);
        Assert.Equal(2_147_483_648L, limits.NextTier.MaxFileBytes);
        Assert.Equal(104_857_600L, limits.MultipartMaxFileBytes);
        Assert.Equal(5, limits.MaxFilesPerUpload);
        Assert.True(limits.DirectUploadAvailable);
        Assert.Equal(new[] { "image/png", "video/mp4" }, limits.AllowedTypes);
    }

    [Fact]
    public async Task Limits_omits_the_workspace_when_none_is_given()
    {
        var handler = new StubHandler().Json("""{"data":{"maxFileBytes":104857600,"multipartMaxFileBytes":104857600}}""");
        using var test = new TestClient(handler);

        await test.Client.Media.LimitsAsync();

        Assert.Equal($"{TestClient.BaseUrl}/v1/media/limits", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Presign_sends_the_optional_fields_and_reads_the_ceiling()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "uploadId": "upl_1",
            "uploadUrl": "https://uploads.test.fopost.com/staging/upl_1",
            "method": "PUT",
            "headers": {},
            "maxFileBytes": 5368709120
          }
        }
        """, HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var upload = await test.Client.Media.PresignAsync(new PresignOptions
        {
            WorkspaceId = "ws_1",
            Filename = "founder.jpg",
            MimeType = "image/jpeg",
            Size = 5_368_709_120L,
            FolderId = "fld_1",
            Origin = "chat",
            BrandRole = "person",
            BrandVoiceId = "bv_1",
            PersonConsent = new PersonConsent("Jane Example", "Agreed to use in marketing"),
        });

        Assert.Equal(5_368_709_120L, upload.MaxFileBytes);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal(5_368_709_120L, body.GetProperty("size").GetInt64());
        Assert.Equal("fld_1", body.GetProperty("folderId").GetString());
        Assert.Equal("chat", body.GetProperty("origin").GetString());
        Assert.Equal("person", body.GetProperty("brandRole").GetString());
        Assert.Equal("bv_1", body.GetProperty("brandVoiceId").GetString());
        Assert.Equal("Jane Example", body.GetProperty("personConsent").GetProperty("subject").GetString());
        Assert.Equal("Agreed to use in marketing", body.GetProperty("personConsent").GetProperty("statement").GetString());
    }

    [Fact]
    public async Task Upload_direct_streams_a_file_of_known_length()
    {
        var handler = new StubHandler()
            .Json(Presigned, HttpStatusCode.Created)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.OK))
            .Json(Completed, HttpStatusCode.Created);
        using var test = new TestClient(handler);
        using var stream = new MemoryStream(Encoding.ASCII.GetBytes("PNG!"));

        var media = await test.Client.Media.UploadDirectAsync("ws_1", "launch.png", "image/png", stream);

        Assert.Equal("med_1", media.Id);
        var body = JsonDocument.Parse(handler.Bodies[0]!).RootElement;
        Assert.Equal(4, body.GetProperty("size").GetInt64());
        var put = handler.Requests[1];
        Assert.IsType<StreamContent>(put.Content);
        Assert.Equal(4, put.Content!.Headers.ContentLength);
        Assert.Equal("PNG!", handler.Bodies[1]);
    }

    [Fact]
    public async Task Presign_oversize_is_a_413_whose_body_names_the_ceiling()
    {
        var handler = new StubHandler().Json("""
        {
          "error": "file_too_large",
          "message": "This file is larger than the account allows",
          "max_file_bytes": 104857600,
          "file_bytes": 209715200,
          "upgrade_url": "https://app.test.fopost.com/settings/billing"
        }
        """, HttpStatusCode.RequestEntityTooLarge);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(() =>
            test.Client.Media.PresignAsync("ws_1", "big.mp4", "video/mp4", 209_715_200L));

        Assert.Equal(413, error.Status);
        Assert.Equal("file_too_large", error.Code);
        Assert.Equal(104_857_600L, (long)error.Body!["max_file_bytes"]!);
        Assert.Equal(209_715_200L, (long)error.Body["file_bytes"]!);
        Assert.Equal("https://app.test.fopost.com/settings/billing", (string)error.Body["upgrade_url"]!);
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
