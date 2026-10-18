using Xunit;

namespace FoPost.Tests;

/// <summary>The dashboard reports under /v1/analytics: overview, series, top posts, labels, table, streak, demographics, inbox, collect.</summary>
public class AnalyticsReportTests
{
    [Fact]
    public async Task Overview_sends_the_scope_and_reads_totals_deltas_and_accounts()
    {
        var handler = new StubHandler().Json("""
        {"total_accounts":2,"total_followers":1200,"total_posts":14,"total_engagement":340,"engagement_rate":0.028,
         "deltas":{"followers":3.5,"posts":null},"today_stats":{"posts":1,"follower_change":4,"engagement":12},
         "platforms":[{"platform":"twitter","accounts":1,"followers":800}],
         "accounts":[{"account_id":"acc_1","platform":"twitter","username":"yourbrand","followers":800,"history":[{"date":"2026-09-01","followers":790}]}]}
        """);
        using var test = new TestClient(handler);

        var overview = await test.Client.Analytics.OverviewAsync(new Resources.AnalyticsScopeOptions { Days = 30, WorkspaceId = "ws_1" });

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/overview?days=30&workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(1200, overview.TotalFollowers);
        Assert.Equal(3.5, overview.Deltas!.Followers);
        Assert.Null(overview.Deltas.Posts);
        Assert.Equal(4, overview.TodayStats!.FollowerChange);
        Assert.Equal(790, Assert.Single(Assert.Single(overview.Accounts).History).Followers);
    }

    [Fact]
    public async Task Time_series_top_posts_and_labels_take_a_range()
    {
        var handler = new StubHandler()
            .Json("""{"days":7,"series":[{"date":"2026-09-01","engagements":12,"impressions":300,"likes":9,"comments":2,"shares":1,"followers":800,"posts":1}]}""")
            .Json("""{"data":[{"rank":1,"post_id":"post_1","source":"fopost","preview":"Hello","platforms":[{"platform":"twitter","username":"yourbrand","url":null,"removed":false}],"labels":[{"id":"lbl_1","name":"Launch","color":"#2563eb"}],"metrics":{"engagements":40,"impressions":900}}]}""")
            .Json("""{"data":[{"label_id":"lbl_1","name":"Launch","color":"#2563eb","post_count":3,"engagements":90,"engagement_rate":0.05,"follower_delta":null}]}""");
        using var test = new TestClient(handler);
        var from = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero);

        var series = await test.Client.Analytics.TimeSeriesAsync(new Resources.AnalyticsRangeOptions { From = from, To = to, AccountId = "acc_1" });
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/analytics/time-series?from=2026-09-01T00%3A00%3A00.000Z&to=2026-09-08T00%3A00%3A00.000Z&account_id=acc_1",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(7, series.Days);
        Assert.Equal(300, Assert.Single(series.Series).Impressions);

        var top = await test.Client.Analytics.TopPostsAsync(new Resources.TopPostsOptions { Limit = 5, Days = 30, Sort = "recent", Label = "lbl_1" });
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/top-posts?limit=5&days=30&sort=recent&label=lbl_1", handler.LastRequest.RequestUri!.ToString());
        var post = Assert.Single(top);
        Assert.Equal(1, post.Rank);
        Assert.Equal(40, post.Metrics!.Engagements);
        Assert.Equal("Launch", Assert.Single(post.Labels).Name);

        var labels = await test.Client.Analytics.LabelsAsync(new Resources.AnalyticsRangeOptions { Days = 30 });
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/labels?days=30", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(0.05, Assert.Single(labels).EngagementRate);
    }

    [Fact]
    public async Task Posts_table_streak_demographics_inbox_and_collect()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"post_id":"post_1","status":"published","platforms":[{"platform":"twitter","username":"yourbrand","delivery_status":"published"}],"delivery_summary":{"total":1,"published":1,"failed":0,"pending":0}}],"meta":{"page":1,"per_page":20,"total":1,"total_pages":1},"summary":{"draft":2,"scheduled":1,"published":1,"failed":0,"pending":0,"total":4}}""")
            .Json("""{"streak":[{"date":"2026-09-01","count":2,"published_count":2,"failed_count":0,"scheduled_count":0}]}""")
            .Json("""{"audience":"engaged","dimensions":{"age":[{"key":"25-34","value":40,"share":0.4}],"gender":[],"country":[],"city":[]},"contributing_accounts":[{"account_id":"acc_1","platform":"instagram","username":"yourbrand"}],"unsupported_accounts":[]}""")
            .Json("""{"totals":{"received":30,"sent":20,"answered":18,"open":5,"conversations":12,"median_response_minutes":42.5},"deltas":{"received":10},"series":[],"by_platform":[{"platform":"instagram","received":30,"sent":20}],"by_type":[{"type":"dm","count":30}],"by_account":[],"response_time":[{"bucket":"under_1h","count":15}],"heatmap":[{"weekday":1,"hour":9,"count":4}],"time_zone":"Europe/Zagreb"}""")
            .Json("""{"accounts":2,"posts":14,"demographics":1,"errors":1,"error_details":[{"account_id":"acc_2","platform":"twitter","username":"other","stage":"account","message":"Balance spent","action":"fund"}]}""");
        using var test = new TestClient(handler);

        var table = await test.Client.Analytics.PostsTableAsync(new Resources.PostsTableOptions { Page = 1, PerPage = 20, WorkspaceId = "ws_1" });
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/posts-table?page=1&per_page=20&workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(1, table.Meta.TotalPages);
        Assert.Equal(4, table.Summary!.Total);
        Assert.Equal("published", Assert.Single(Assert.Single(table.Rows).Platforms).DeliveryStatus);

        var streak = await test.Client.Analytics.PostingStreakAsync("ws_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/posting-streak?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(2, Assert.Single(streak).PublishedCount);

        var demographics = await test.Client.Analytics.DemographicsAsync(new Resources.DemographicsOptions { Audience = "engaged", AccountId = "acc_1" });
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/demographics?audience=engaged&account_id=acc_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(0.4, Assert.Single(demographics.Dimensions.Age).Share);
        Assert.Equal("instagram", Assert.Single(demographics.ContributingAccounts).Platform);

        var inbox = await test.Client.Analytics.InboxAsync(new Resources.InboxAnalyticsOptions { Days = 30, TimeZone = "Europe/Zagreb" });
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/inbox?days=30&tz=Europe%2FZagreb", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(42.5, inbox.Totals!.MedianResponseMinutes);
        Assert.Equal(9, Assert.Single(inbox.Heatmap).Hour);

        var collected = await test.Client.Analytics.CollectAsync("ws_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/analytics/collect?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Null(handler.LastBody);
        Assert.Equal("fund", Assert.Single(collected.ErrorDetails).Action);
    }
}
