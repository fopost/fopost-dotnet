using System.Net.Http;
using FoPost.Resources;
using Xunit;

namespace FoPost.Tests;

/// <summary>The deeper analytics endpoints: decay, cadence, timelines, changes and native posts.</summary>
public class AnalyticsTests
{
    [Fact]
    public async Task Decay_reads_the_bands_and_the_half_life()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "days": 30,
            "postsMeasured": 2,
            "halfLifeBucket": "1h_3h",
            "bands": [
              {"bucket":"under_1h","label":"First hour","posts":2,"avgEngagements":25,"avgImpressions":300,"shareOfFinal":0.3},
              {"bucket":"6h_12h","label":"6-12 hours","posts":0,"avgEngagements":0,"avgImpressions":0,"shareOfFinal":null}
            ]
          }
        }
        """);
        using var test = new TestClient(handler);

        var decay = await test.Client.Analytics.DecayAsync(new AnalyticsScopeOptions { Days = 30, AccountId = "a1" });

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/analytics/decay?days=30&accountId=a1",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("1h_3h", decay.HalfLifeBucket);
        Assert.Equal(2, decay.PostsMeasured);
        Assert.Equal(0.3, decay.Bands[0].ShareOfFinal);
        // A band nothing was measured in reports no share rather than zero
        Assert.Null(decay.Bands[1].ShareOfFinal);
    }

    [Fact]
    public async Task Frequency_reads_the_weeks_and_the_best_cadence()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "days": 90,
            "weeks": [{"weekStart":"2026-03-02","posts":2,"engagements":240,"avgEngagementsPerPost":120}],
            "bands": [{"band":"under_3","label":"1-2 a week","weeks":1,"posts":2,"avgPostsPerWeek":2,"avgEngagementsPerPost":120,"engagementRate":0.12}],
            "best": {"band":"under_3","label":"1-2 a week","avgEngagementsPerPost":120}
          }
        }
        """);
        using var test = new TestClient(handler);

        var cadence = await test.Client.Analytics.FrequencyAsync(new AnalyticsScopeOptions { Days = 90 });

        Assert.Equal("2026-03-02", cadence.Weeks[0].WeekStart);
        Assert.Equal(0.12, cadence.Bands[0].EngagementRate);
        Assert.NotNull(cadence.Best);
        Assert.Equal("1-2 a week", cadence.Best!.Label);
    }

    [Fact]
    public async Task A_timeline_can_be_addressed_by_permalink()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "postId": null,
            "deliveries": [{
              "accountId":"a1","platform":"twitter","username":"acme","externalPostId":"1",
              "postedAt":"2026-03-02T00:00:00.000Z",
              "points":[{"at":"2026-03-02T00:30:00.000Z","ageMinutes":30,"engagements":40,"impressions":400,
                         "reach":null,"likes":30,"comments":null,"shares":null,"videoViews":null,
                         "delta":{"impressions":400,"reach":0,"engagements":40,"likes":30,"comments":0,"shares":0}}]
            }]
          }
        }
        """);
        using var test = new TestClient(handler);

        var timeline = await test.Client.Analytics.TimelineAsync("https://x.com/acme/status/1");

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/analytics/posts/https%3A%2F%2Fx.com%2Facme%2Fstatus%2F1/timeline",
            handler.LastRequest.RequestUri!.ToString());
        // A post made on the network has no FoPost id
        Assert.Null(timeline.PostId);
        var point = timeline.Deliveries[0].Points[0];
        Assert.Equal(30, point.AgeMinutes);
        Assert.Equal(40, point.Delta.Engagements);
    }

    [Fact]
    public async Task Changes_carries_the_cursor()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "since": "2026-03-02T00:00:00.000Z",
            "cursor": "2026-03-02T06:00:00.000Z",
            "hasMore": true,
            "changes": [{"accountId":"a1","platform":"twitter","externalPostId":"1","postId":"p1",
                         "postedAt":"2026-03-02T00:00:00.000Z","fetchedAt":"2026-03-02T06:00:00.000Z",
                         "impressions":900,"reach":null,"engagements":90,"likes":70,"comments":10,"shares":10}]
          }
        }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Analytics.ChangesAsync(new MetricChangesOptions
        {
            Since = DateTimeOffset.Parse("2026-03-02T00:00:00Z"),
            Limit = 100,
        });

        Assert.Contains("limit=100", handler.LastRequest.RequestUri!.Query);
        Assert.True(page.HasMore);
        Assert.Equal("p1", page.Changes[0].PostId);
    }

    [Fact]
    public async Task CollectPost_reports_each_delivery()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "collected": 1,
            "deliveries": [{"accountId":"a1","platform":"twitter","externalPostId":"1","collected":true,
                            "fetchedAt":"2026-03-02T00:30:00.000Z","message":null}]
          }
        }
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Analytics.CollectPostAsync("p1");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/posts/p1/analytics/collect",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(1, result.Collected);
        Assert.True(result.Deliveries[0].Collected);
    }

    [Fact]
    public async Task NativePosts_keeps_the_meta_envelope()
    {
        var handler = new StubHandler().Json("""
        {
          "data": [{"externalPostId":"1","text":"Posted by hand","permalink":"https://x.com/acme/status/1",
                    "thumbnailUrl":null,"mediaType":null,"postedAt":"2026-03-02T00:00:00.000Z",
                    "fetchedAt":"2026-03-02T06:00:00.000Z",
                    "metrics":{"impressions":900,"reach":null,"engagements":90,"likes":70,
                               "comments":10,"shares":10,"videoViews":null}}],
          "meta": {"page":1,"perPage":20,"total":1}
        }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Analytics.NativePostsAsync("a1", new NativePostsOptions { Page = 1, PerPage = 20 });

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/native-posts?page=1&per_page=20",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Single(page);
        Assert.Equal("https://x.com/acme/status/1", page[0].Permalink);
        Assert.Equal(90, page[0].Metrics.Engagements);
        Assert.Equal(1, page.Meta.Total);
    }
}
