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

    [Fact]
    public async Task Bid_strategies_name_the_connection_and_the_customer()
    {
        var handler = new StubHandler().Json("""
            {"data":[{"id":"1234567890~biddingStrategy~42","name":"Target CPA","type":"TARGET_CPA",
            "status":"ENABLED","campaignCount":3}]}
            """);
        using var test = new TestClient(handler);

        var strategies = await test.Client.Ads.Google.BidStrategiesAsync(Scope);

        Assert.Single(strategies);
        Assert.Equal("Target CPA", strategies[0].Name);
        Assert.Equal(3, strategies[0].CampaignCount);

        var uri = handler.LastRequest.RequestUri!;
        Assert.Equal("/v1/ads/google/bid-strategies", uri.AbsolutePath);
        Assert.Contains("connection_id=conn_1", uri.Query);
        Assert.Contains("customer_id=1234567890", uri.Query);
    }

    [Fact]
    public async Task Create_bid_strategy_sends_the_scope_in_the_body()
    {
        var handler = new StubHandler().Json("""{"data":{"id":"1234567890~biddingStrategy~42"}}""");
        using var test = new TestClient(handler);

        var id = await test.Client.Ads.Google.CreateBidStrategyAsync(new CreateGoogleBidStrategyOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            CustomerId = "1234567890",
            Name = "Target CPA",
            Type = GoogleBidStrategyTypes.TargetCpa,
            TargetMinor = 1500,
        });

        Assert.Equal("1234567890~biddingStrategy~42", id);
        using var body = JsonDocument.Parse(handler.LastBody!);
        Assert.Equal("1234567890", body.RootElement.GetProperty("customerId").GetString());
        Assert.Equal("TARGET_CPA", body.RootElement.GetProperty("type").GetString());
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
