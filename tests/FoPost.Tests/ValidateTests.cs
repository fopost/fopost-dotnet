using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class ValidateTests
{
    [Fact]
    public async Task Post_sends_content_media_and_platforms_and_reads_per_platform_verdicts()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "ready": false,
            "platforms": [
              {"platform":"twitter","ready":false,"issues":["over_length"],"score":42,"signals":[{"level":"warn","code":"no_hashtags","message":"Add a hashtag"}]},
              {"platform":"linkedin","ready":true,"issues":[],"signals":[]}
            ]
          }
        }
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Validate.PostAsync(new ValidatePostOptions
        {
            Content = "Hello",
            Media = new List<ValidateMediaItem> { new() { Url = "https://yourbrand.com/a.png", MimeType = "image/png", Size = 1024 } },
            Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/validate/post", handler.LastRequest.RequestUri!.ToString());

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("Hello", body.GetProperty("content").GetString());
        var media = Assert.Single(body.GetProperty("media").EnumerateArray());
        Assert.Equal("https://yourbrand.com/a.png", media.GetProperty("url").GetString());
        Assert.Equal("image/png", media.GetProperty("mime_type").GetString());
        Assert.Equal(1024, media.GetProperty("size").GetInt64());
        Assert.Equal(2, body.GetProperty("platforms").GetArrayLength());

        Assert.False(result.Ready);
        Assert.Equal(2, result.Platforms.Count);
        Assert.Equal("over_length", Assert.Single(result.Platforms[0].Issues));
        Assert.Equal(42, result.Platforms[0].Score);
        Assert.Equal("warn", Assert.Single(result.Platforms[0].Signals).Level);
        Assert.True(result.Platforms[1].Ready);
        Assert.Null(result.Platforms[1].Score);
    }

    [Fact]
    public async Task Length_sends_text_and_platforms_and_reads_a_null_limit()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "ok": false,
            "platforms": [
              {"platform":"twitter","length":300,"limit":280,"unit":"chars","ok":false,"signals":[{"level":"warn","code":"over_length","message":"20 over"}]},
              {"platform":"linkedin","length":300,"limit":null,"unit":"chars","ok":true,"signals":[]}
            ]
          }
        }
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Validate.LengthAsync(new ValidateLengthOptions
        {
            Text = "A long draft",
            Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/validate/length", handler.LastRequest.RequestUri!.ToString());

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("A long draft", body.GetProperty("text").GetString());
        Assert.Equal("twitter", body.GetProperty("platforms")[0].GetString());

        Assert.False(result.Ok);
        Assert.Equal(280, result.Platforms[0].Limit);
        Assert.Equal("over_length", Assert.Single(result.Platforms[0].Signals).Code);
        Assert.Null(result.Platforms[1].Limit);
        Assert.True(result.Platforms[1].Ok);
    }

    [Fact]
    public async Task Media_sends_the_url_and_reads_the_verdict()
    {
        var handler = new StubHandler().Json("""
        {"data":{"ok":true,"issues":[],"name":"a.png","size":1024,"mime_type":"image/png","type":"image"}}
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Validate.MediaAsync("https://yourbrand.com/a.png");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/validate/media", handler.LastRequest.RequestUri!.ToString());

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("https://yourbrand.com/a.png", body.GetProperty("url").GetString());

        Assert.True(result.Ok);
        Assert.Empty(result.Issues);
        Assert.Equal("a.png", result.Name);
        Assert.Equal(1024, result.Size);
        Assert.Equal("image/png", result.MimeType);
        Assert.Equal("image", result.Type);
    }
}
