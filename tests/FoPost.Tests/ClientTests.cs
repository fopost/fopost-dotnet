using Xunit;

namespace FoPost.Tests;

public class ClientTests
{
    [Fact]
    public void Constructor_rejects_a_missing_credential()
    {
        var options = new FoPostClientOptions { ApiKey = null };

        Assert.Throws<ArgumentException>(() => new FoPostClient(options));
    }

    [Fact]
    public void Constructor_accepts_a_bearer_token_without_an_api_key()
    {
        using var client = new FoPostClient(new FoPostClientOptions
        {
            ApiKey = null,
            BearerToken = "jwt",
        });

        Assert.Equal(FoPostClientOptions.DefaultBaseUrl, client.BaseUrl);
    }

    [Fact]
    public void Constructor_rejects_a_retry_count_below_one()
    {
        var options = new FoPostClientOptions { ApiKey = "k", MaxRetries = 0 };

        Assert.Throws<ArgumentException>(() => new FoPostClient(options));
    }

    [Theory]
    [InlineData("https://api.fopost.com/", "https://api.fopost.com")]
    [InlineData("  https://self.hosted/api  ", "https://self.hosted/api")]
    [InlineData("", FoPostClientOptions.DefaultBaseUrl)]
    public void Base_url_is_normalized(string given, string expected)
    {
        using var client = new FoPostClient(new FoPostClientOptions { ApiKey = "k", BaseUrl = given });

        Assert.Equal(expected, client.BaseUrl);
    }

    [Fact]
    public void The_default_base_url_targets_the_v1_prefix_the_api_actually_serves()
    {
        // Regression guard: /api/v1 is a 404 on the deployed API.
        Assert.Equal("https://api.fopost.com", FoPostClientOptions.DefaultBaseUrl);
    }

    [Fact]
    public async Task Every_request_carries_the_api_key_and_a_user_agent()
    {
        var handler = new StubHandler().Json("""{"data":[]}""");
        using var test = new TestClient(handler);

        await test.Client.Workspaces.ListAsync();

        var request = handler.LastRequest;
        Assert.Equal($"{TestClient.BaseUrl}/v1/workspaces", request.RequestUri!.ToString());
        Assert.Equal(TestClient.ApiKey, request.Headers.GetValues("X-API-Key").Single());
        Assert.Contains("fopost-dotnet", request.Headers.UserAgent.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_bearer_token_is_sent_as_an_authorization_header()
    {
        var handler = new StubHandler().Json("""{"data":[]}""");
        using var test = new TestClient(handler, new FoPostClientOptions
        {
            ApiKey = null,
            BearerToken = "jwt-token",
        });

        await test.Client.Workspaces.ListAsync();

        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("jwt-token", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.False(handler.LastRequest.Headers.Contains("X-API-Key"));
    }

    [Fact]
    public async Task A_bearer_token_wins_over_an_api_key_that_came_from_the_environment()
    {
        var handler = new StubHandler().Json("""{"data":[]}""");
        using var test = new TestClient(handler, new FoPostClientOptions
        {
            ApiKey = "fp_from_env",
            BearerToken = "jwt-token",
        });

        await test.Client.Workspaces.ListAsync();

        Assert.Equal("jwt-token", handler.LastRequest.Headers.Authorization!.Parameter);
        Assert.False(handler.LastRequest.Headers.Contains("X-API-Key"));
    }

    [Fact]
    public async Task The_escape_hatch_reaches_an_unwrapped_endpoint()
    {
        var handler = new StubHandler().Json("""{"data":{"ok":true}}""");
        using var test = new TestClient(handler);

        var body = await test.Client.RequestAsync(
            HttpMethod.Get,
            "/v1/analytics/overview",
            query: new Dictionary<string, object?> { ["workspace_id"] = "ws_1" });

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/analytics/overview?workspace_id=ws_1",
            handler.LastRequest.RequestUri!.ToString());
        Assert.NotNull(body);
    }
}
