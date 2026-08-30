using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class ResourceTests
{
    [Fact]
    public async Task Accounts_list_uses_the_camelCase_query_param_this_endpoint_reads()
    {
        var handler = new StubHandler().Json($$"""{"data":[{{Fixtures.Account}}]}""");
        using var test = new TestClient(handler);

        var accounts = await test.Client.Accounts.ListAsync("ws_1");

        Assert.Equal("acc_1", Assert.Single(accounts).Id);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts?workspaceId=ws_1",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Accounts_health_reads_one_account()
    {
        var handler = new StubHandler().Json("""
        {"data":{"id":"acc_1","platform":"twitter","active":true,"healthStatus":"expired"}}
        """);
        using var test = new TestClient(handler);

        var health = await test.Client.Accounts.HealthAsync("acc_1");

        Assert.Equal("expired", health.HealthStatus);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/health",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Workspaces_list_and_get()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"ws_1","name":"Acme","slug":"acme","timezone":"UTC"}]}""")
            .Json("""{"data":{"id":"ws_1","name":"Acme","accounts":[]}}""");
        using var test = new TestClient(handler);

        var workspaces = await test.Client.Workspaces.ListAsync();
        var workspace = await test.Client.Workspaces.GetAsync("ws_1");

        Assert.Equal("Acme", Assert.Single(workspaces).Name);
        Assert.Equal("ws_1", workspace.Id);
        Assert.Empty(workspace.Accounts);
    }

    [Fact]
    public async Task Labels_list_uses_the_snake_case_query_param()
    {
        var handler = new StubHandler().Json("""{"data":[{"id":"lbl_1","name":"Launch","color":"#ff0000"}]}""");
        using var test = new TestClient(handler);

        var labels = await test.Client.Labels.ListAsync("ws_1");

        Assert.Equal("Launch", Assert.Single(labels).Name);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/labels?workspace_id=ws_1",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Ai_credits_reads_the_balance()
    {
        var handler = new StubHandler().Json("""
        {"data":{"credits_remaining":940,"credits_used":60,"credits_total":1000}}
        """);
        using var test = new TestClient(handler);

        var balance = await test.Client.Ai.CreditsAsync();

        Assert.Equal(940, balance.CreditsRemaining);
        Assert.Equal(1000, balance.CreditsTotal);
    }

    [Fact]
    public async Task Ai_generate_caption_compacts_the_body_it_sends()
    {
        var handler = new StubHandler().Json("""
        {"caption":"Shipping a new feature","credits":{"charged":2,"remaining":938}}
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Ai.GenerateCaptionAsync(new GenerateCaptionOptions
        {
            CurrentCaption = "shipping a new feature",
            Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
        });

        Assert.Equal("Shipping a new feature", result.Caption);
        Assert.Equal(2, result.Credits!.Charged);

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("shipping a new feature", body.GetProperty("current_caption").GetString());
        Assert.False(body.TryGetProperty("char_limit", out _));
        Assert.False(body.TryGetProperty("workspace_id", out _));
    }

    [Fact]
    public async Task Ai_rewrite_returns_one_variant_per_platform()
    {
        var handler = new StubHandler().Json("""
        {
          "results": [
            {"platform":"twitter","content":"Short","credits":1},
            {"platform":"linkedin","content":"Longer","credits":1}
          ],
          "credits": {"charged":2,"remaining":936}
        }
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Ai.RewriteAsync(new RewriteOptions
        {
            Content = "A draft",
            Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
        });

        Assert.Equal(2, result.Results.Count);
        Assert.Equal("Short", result.Results[0].Content);
        Assert.Equal(936, result.Credits!.Remaining);
    }

    [Fact]
    public async Task Ai_repurpose_url_keys_its_posts_by_platform()
    {
        var handler = new StubHandler().Json("""
        {
          "url": "https://example.com/blog/post",
          "title": "A post",
          "posts": {"twitter":"tweet","linkedin":"update"},
          "credits": {"charged":4,"remaining":932}
        }
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Ai.RepurposeUrlAsync(new RepurposeUrlOptions
        {
            Url = "https://example.com/blog/post",
            Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
        });

        Assert.Equal("A post", result.Title);
        Assert.Equal("tweet", result.Posts[Platforms.Twitter]);
    }
}
