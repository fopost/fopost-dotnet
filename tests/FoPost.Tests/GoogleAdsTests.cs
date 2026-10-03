using System.Net.Http;
using System.Text.Json;
using FoPost.Resources;
using Xunit;

namespace FoPost.Tests;

public class GoogleAdsTests
{
    private static GoogleAdsScope Scope => new()
    {
        WorkspaceId = "ws_1",
        ConnectionId = "conn_1",
        CustomerId = "1234567890",
    };

    private const string Keyword = """
    {
      "id": "1234567890~keyword~77~99",
      "adGroupId": "1234567890~adGroup~77",
      "text": "running shoes",
      "matchType": "EXACT",
      "status": "ENABLED",
      "cpcBidMinor": 180,
      "negative": false
    }
    """;

    [Fact]
    public async Task Keywords_name_the_connection_and_the_customer()
    {
        var handler = new StubHandler().Json($$"""{"data":[{{Keyword}}]}""");
        using var test = new TestClient(handler);

        var keywords = await test.Client.Ads.Google.KeywordsAsync(Scope, "1234567890~adGroup~77");

        Assert.Single(keywords);
        Assert.Equal("running shoes", keywords[0].Text);
        Assert.Equal(180, keywords[0].CpcBidMinor);

        var uri = handler.LastRequest.RequestUri!;
        Assert.Equal("/v1/ads/google/keywords", uri.AbsolutePath);
        Assert.Contains("connection_id=conn_1", uri.Query);
        Assert.Contains("customer_id=1234567890", uri.Query);
    }

    [Fact]
    public async Task Create_keyword_sends_the_scope_in_the_body()
    {
        var handler = new StubHandler().Json("""{"data":{"id":"1234567890~keyword~77~99"}}""");
        using var test = new TestClient(handler);

        var id = await test.Client.Ads.Google.CreateKeywordAsync(new CreateGoogleKeywordOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            CustomerId = "1234567890",
            AdGroupId = "1234567890~adGroup~77",
            Text = "running shoes",
            MatchType = GoogleMatchTypes.Exact,
        });

        Assert.Equal("1234567890~keyword~77~99", id);
        using var body = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("1234567890", body.RootElement.GetProperty("customerId").GetString());
        Assert.Equal("EXACT", body.RootElement.GetProperty("matchType").GetString());
    }

    [Fact]
    public async Task Delete_carries_the_scope_in_the_body()
    {
        var handler = new StubHandler().Json("""{"data":null}""");
        using var test = new TestClient(handler);

        await test.Client.Ads.Google.DeleteAssetAsync("1234567890~asset~4321", Scope);

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        using var body = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("conn_1", body.RootElement.GetProperty("connectionId").GetString());
    }

    [Fact]
    public async Task Ad_schedule_is_replaced_with_put()
    {
        var handler = new StubHandler().Json("""{"data":{"slots":2}}""");
        using var test = new TestClient(handler);

        var slots = await test.Client.Ads.Google.SetAdScheduleAsync(new SetGoogleAdScheduleOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            CustomerId = "1234567890",
            CampaignId = "1234567890~campaign~55",
            Slots = new List<GoogleAdScheduleInput>
            {
                new() { DayOfWeek = "MONDAY", StartHour = 9, EndHour = 18 },
            },
        });

        Assert.Equal(2, slots);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
    }

    [Fact]
    public async Task Query_returns_rows_as_google_sends_them()
    {
        var handler = new StubHandler().Json("""{"data":{"rows":[{"campaign":{"id":"55"}}]}}""");
        using var test = new TestClient(handler);

        var result = await test.Client.Ads.Google.QueryAsync(new GoogleQueryOptions
        {
            ConnectionId = "conn_1",
            CustomerId = "1234567890",
            Query = "SELECT campaign.id FROM campaign",
        });

        Assert.Single(result.Rows);
        Assert.Equal("/v1/ads/insights/query", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Authorize_google_has_its_own_route()
    {
        var handler = new StubHandler().Json("""{"data":{"url":"https://accounts.google.com/o/x"}}""");
        using var test = new TestClient(handler);

        var url = await test.Client.Ads.AuthorizeGoogleAsync(new AuthorizeGoogleAdsOptions
        {
            WorkspaceId = "ws_1",
        });

        Assert.Equal("https://accounts.google.com/o/x", url);
        Assert.Equal("/v1/ads/connections/google/authorize", handler.LastRequest.RequestUri!.AbsolutePath);
    }
}
