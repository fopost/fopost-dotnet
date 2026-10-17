using System.Net;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class PlatformMetricsTests
{
    [Fact]
    public async Task Platform_metrics_asks_for_raw_and_reads_the_set()
    {
        var handler = new StubHandler().Json("""
        {"data":{"platform":"facebook",
          "account":{"fetched_at":"2026-09-20T02:00:00.000Z","metrics":[
            {"key":"page_daily_video_ad_break_earnings","label":"Ad Break Earnings","kind":"currency_usd","value":42.15},
            {"key":"page_impressions_paid","label":"Paid Impressions","kind":"count","value":1500}]},
          "post":{"external_post_id":"123_456","fetched_at":"2026-09-20T02:00:00.000Z","metrics":[]}}}
        """);
        using var test = new TestClient(handler);

        var metrics = await test.Client.Accounts.PlatformMetricsAsync("a1");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/insights?raw=true",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("facebook", metrics.Platform);
        Assert.Equal("2026-09-20T02:00:00.000Z", metrics.Account.FetchedAt);
        Assert.Equal(
            new[] { "page_daily_video_ad_break_earnings", "page_impressions_paid" },
            metrics.Account.Metrics.Select(m => m.Key));
        Assert.Equal(42.15, metrics.Account.Metrics[0].Number());
        Assert.Equal("123_456", metrics.Post.ExternalPostId);
        Assert.Empty(metrics.Post.Metrics);
    }

    [Fact]
    public async Task A_series_value_stays_a_raw_element()
    {
        var handler = new StubHandler().Json("""
        {"data":{"platform":"youtube",
          "account":{"fetched_at":null,"metrics":[
            {"key":"daily_views","label":"Views by Day","kind":"series","value":[{"day":"2026-09-19","views":600}]}]},
          "post":{"external_post_id":null,"fetched_at":null,"metrics":[]}}}
        """);
        using var test = new TestClient(handler);

        var row = (await test.Client.Accounts.PlatformMetricsAsync("a1")).Account.Metrics[0];

        Assert.Null(row.Number());
        Assert.Equal(JsonValueKind.Array, row.Value!.Value.ValueKind);
        Assert.Equal(600, row.Value!.Value[0].GetProperty("views").GetInt32());
    }

    [Fact]
    public async Task A_pending_metric_grant_throws()
    {
        var handler = new StubHandler().Json(
            """{"error":"platform_metrics_unavailable","message":"google-business metrics are not available on this deployment yet."}""",
            HttpStatusCode.ServiceUnavailable);
        using var test = new TestClient(handler, new FoPostClientOptions { MaxRetries = 1 });

        var error = await Assert.ThrowsAsync<FoPostException>(
            () => test.Client.Accounts.PlatformMetricsAsync("a1"));

        Assert.Equal(503, error.Status);
        Assert.Equal("platform_metrics_unavailable", error.Code);
    }
}
