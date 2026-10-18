using System.Text.Json.Serialization;

namespace FoPost;

// The dashboard reports under /v1/analytics/*, all on the analytics scope.

/// <summary>Workspace-wide totals over a window, with the change against the window before.</summary>
public sealed class AnalyticsOverview : FoPostModel
{
    [JsonPropertyName("total_accounts")]
    public int? TotalAccounts { get; set; }

    [JsonPropertyName("total_followers")]
    public int? TotalFollowers { get; set; }

    [JsonPropertyName("total_posts")]
    public int? TotalPosts { get; set; }

    [JsonPropertyName("total_engagement")]
    public int? TotalEngagement { get; set; }

    [JsonPropertyName("total_impressions")]
    public int? TotalImpressions { get; set; }

    [JsonPropertyName("total_reach")]
    public int? TotalReach { get; set; }

    [JsonPropertyName("total_likes")]
    public int? TotalLikes { get; set; }

    [JsonPropertyName("total_comments")]
    public int? TotalComments { get; set; }

    [JsonPropertyName("total_shares")]
    public int? TotalShares { get; set; }

    [JsonPropertyName("total_reposts")]
    public int? TotalReposts { get; set; }

    [JsonPropertyName("total_saves")]
    public int? TotalSaves { get; set; }

    [JsonPropertyName("total_clicks")]
    public int? TotalClicks { get; set; }

    [JsonPropertyName("total_video_views")]
    public int? TotalVideoViews { get; set; }

    [JsonPropertyName("total_profile_views")]
    public int? TotalProfileViews { get; set; }

    [JsonPropertyName("engagement_rate")]
    public double? EngagementRate { get; set; }

    [JsonPropertyName("deltas")]
    public AnalyticsDeltas? Deltas { get; set; }

    [JsonPropertyName("today_stats")]
    public AnalyticsTodayStats? TodayStats { get; set; }

    [JsonPropertyName("platforms")]
    public IList<AnalyticsPlatformTotal> Platforms { get; set; } = new List<AnalyticsPlatformTotal>();

    [JsonPropertyName("accounts")]
    public IList<AnalyticsAccountSummary> Accounts { get; set; } = new List<AnalyticsAccountSummary>();
}

/// <summary>Percentage change against the previous window; null when there was nothing to compare.</summary>
public sealed class AnalyticsDeltas : FoPostModel
{
    [JsonPropertyName("followers")]
    public double? Followers { get; set; }

    [JsonPropertyName("posts")]
    public double? Posts { get; set; }

    [JsonPropertyName("engagement")]
    public double? Engagement { get; set; }

    [JsonPropertyName("impressions")]
    public double? Impressions { get; set; }

    [JsonPropertyName("likes")]
    public double? Likes { get; set; }

    [JsonPropertyName("comments")]
    public double? Comments { get; set; }

    [JsonPropertyName("shares")]
    public double? Shares { get; set; }

    [JsonPropertyName("profile_views")]
    public double? ProfileViews { get; set; }
}

public sealed class AnalyticsTodayStats : FoPostModel
{
    [JsonPropertyName("posts")]
    public int? Posts { get; set; }

    [JsonPropertyName("follower_change")]
    public int? FollowerChange { get; set; }

    [JsonPropertyName("engagement")]
    public int? Engagement { get; set; }
}

public sealed class AnalyticsPlatformTotal : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("accounts")]
    public int? Accounts { get; set; }

    [JsonPropertyName("followers")]
    public int? Followers { get; set; }
}

public sealed class AnalyticsAccountSummary : FoPostModel
{
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("followers")]
    public int? Followers { get; set; }

    [JsonPropertyName("total_posts")]
    public int? TotalPosts { get; set; }

    [JsonPropertyName("fetched_at")]
    public DateTimeOffset? FetchedAt { get; set; }

    [JsonPropertyName("history")]
    public IList<FollowerPoint> History { get; set; } = new List<FollowerPoint>();
}

public sealed class FollowerPoint : FoPostModel
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("followers")]
    public int? Followers { get; set; }
}

/// <summary>One row per day over the window.</summary>
public sealed class AnalyticsTimeSeries : FoPostModel
{
    [JsonPropertyName("days")]
    public int Days { get; set; }

    [JsonPropertyName("series")]
    public IList<TimeSeriesPoint> Series { get; set; } = new List<TimeSeriesPoint>();
}

public sealed class TimeSeriesPoint : FoPostModel
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("engagements")]
    public int? Engagements { get; set; }

    [JsonPropertyName("impressions")]
    public int? Impressions { get; set; }

    [JsonPropertyName("likes")]
    public int? Likes { get; set; }

    [JsonPropertyName("comments")]
    public int? Comments { get; set; }

    [JsonPropertyName("shares")]
    public int? Shares { get; set; }

    [JsonPropertyName("followers")]
    public int? Followers { get; set; }

    [JsonPropertyName("posts")]
    public int? Posts { get; set; }
}

/// <summary>A post ranked by engagement. <see cref="Source"/> is <c>fopost</c> or <c>platform</c> for one made natively.</summary>
public sealed class TopPost : FoPostModel
{
    [JsonPropertyName("rank")]
    public int? Rank { get; set; }

    [JsonPropertyName("post_id")]
    public string? PostId { get; set; }

    [JsonPropertyName("external_post_id")]
    public string? ExternalPostId { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("preview")]
    public string? Preview { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("platforms")]
    public IList<TopPostPlatform> Platforms { get; set; } = new List<TopPostPlatform>();

    [JsonPropertyName("labels")]
    public IList<Label> Labels { get; set; } = new List<Label>();

    [JsonPropertyName("metrics")]
    public TopPostMetrics? Metrics { get; set; }
}

public sealed class TopPostPlatform : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("removed")]
    public bool? Removed { get; set; }
}

public sealed class TopPostMetrics : FoPostModel
{
    [JsonPropertyName("engagements")]
    public int? Engagements { get; set; }

    [JsonPropertyName("impressions")]
    public int? Impressions { get; set; }

    [JsonPropertyName("reach")]
    public int? Reach { get; set; }

    [JsonPropertyName("likes")]
    public int? Likes { get; set; }

    [JsonPropertyName("comments")]
    public int? Comments { get; set; }

    [JsonPropertyName("shares")]
    public int? Shares { get; set; }

    [JsonPropertyName("reposts")]
    public int? Reposts { get; set; }

    [JsonPropertyName("clicks")]
    public int? Clicks { get; set; }

    [JsonPropertyName("saves")]
    public int? Saves { get; set; }

    [JsonPropertyName("video_views")]
    public int? VideoViews { get; set; }
}

/// <summary>What the posts under one label earned together.</summary>
public sealed class LabelAnalytics : FoPostModel
{
    [JsonPropertyName("label_id")]
    public string LabelId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("post_count")]
    public int? PostCount { get; set; }

    [JsonPropertyName("impressions")]
    public int? Impressions { get; set; }

    [JsonPropertyName("reach")]
    public int? Reach { get; set; }

    [JsonPropertyName("engagements")]
    public int? Engagements { get; set; }

    [JsonPropertyName("likes")]
    public int? Likes { get; set; }

    [JsonPropertyName("comments")]
    public int? Comments { get; set; }

    [JsonPropertyName("shares")]
    public int? Shares { get; set; }

    [JsonPropertyName("engagement_rate")]
    public double? EngagementRate { get; set; }

    [JsonPropertyName("follower_delta")]
    public int? FollowerDelta { get; set; }
}

/// <summary>A page of posts with their per-network delivery state, plus status counts for the whole window.</summary>
public sealed class PostsTable : FoPostModel
{
    [JsonPropertyName("data")]
    public IList<PostsTableRow> Rows { get; set; } = new List<PostsTableRow>();

    [JsonPropertyName("meta")]
    public OffsetPageMeta Meta { get; set; } = new();

    [JsonPropertyName("summary")]
    public PostsTableSummary? Summary { get; set; }
}

public sealed class PostsTableRow : FoPostModel
{
    [JsonPropertyName("post_id")]
    public string PostId { get; set; } = string.Empty;

    [JsonPropertyName("preview")]
    public string? Preview { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("scheduled_at")]
    public DateTimeOffset? ScheduledAt { get; set; }

    [JsonPropertyName("platforms")]
    public IList<PostsTablePlatform> Platforms { get; set; } = new List<PostsTablePlatform>();

    [JsonPropertyName("delivery_summary")]
    public DeliverySummary? DeliverySummary { get; set; }
}

public sealed class PostsTablePlatform : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("delivery_status")]
    public string? DeliveryStatus { get; set; }
}

public sealed class DeliverySummary : FoPostModel
{
    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("published")]
    public int? Published { get; set; }

    [JsonPropertyName("failed")]
    public int? Failed { get; set; }

    [JsonPropertyName("pending")]
    public int? Pending { get; set; }
}

public sealed class PostsTableSummary : FoPostModel
{
    [JsonPropertyName("draft")]
    public int? Draft { get; set; }

    [JsonPropertyName("scheduled")]
    public int? Scheduled { get; set; }

    [JsonPropertyName("published")]
    public int? Published { get; set; }

    [JsonPropertyName("failed")]
    public int? Failed { get; set; }

    [JsonPropertyName("pending")]
    public int? Pending { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>One day of posting activity in the 365-day streak.</summary>
public sealed class PostingStreakDay : FoPostModel
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("published_count")]
    public int? PublishedCount { get; set; }

    [JsonPropertyName("failed_count")]
    public int? FailedCount { get; set; }

    [JsonPropertyName("scheduled_count")]
    public int? ScheduledCount { get; set; }
}

/// <summary>Who the audience is, by age, gender, country and city.</summary>
public sealed class AudienceDemographics : FoPostModel
{
    /// <summary><c>followers</c>, <c>engaged</c> or <c>reached</c>.</summary>
    [JsonPropertyName("audience")]
    public string Audience { get; set; } = string.Empty;

    [JsonPropertyName("dimensions")]
    public DemographicDimensions Dimensions { get; set; } = new();

    [JsonPropertyName("contributing_accounts")]
    public IList<DemographicAccount> ContributingAccounts { get; set; } = new List<DemographicAccount>();

    /// <summary>Accounts on networks that publish no demographics.</summary>
    [JsonPropertyName("unsupported_accounts")]
    public IList<DemographicAccount> UnsupportedAccounts { get; set; } = new List<DemographicAccount>();
}

public sealed class DemographicDimensions : FoPostModel
{
    [JsonPropertyName("age")]
    public IList<DemographicBucket> Age { get; set; } = new List<DemographicBucket>();

    [JsonPropertyName("gender")]
    public IList<DemographicBucket> Gender { get; set; } = new List<DemographicBucket>();

    [JsonPropertyName("country")]
    public IList<DemographicBucket> Country { get; set; } = new List<DemographicBucket>();

    [JsonPropertyName("city")]
    public IList<DemographicBucket> City { get; set; } = new List<DemographicBucket>();
}

public sealed class DemographicBucket : FoPostModel
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public double Value { get; set; }

    /// <summary>Fraction of the audience, 0 to 1.</summary>
    [JsonPropertyName("share")]
    public double Share { get; set; }
}

public sealed class DemographicAccount : FoPostModel
{
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}

/// <summary>How the inbox is doing: volume, response times and who answers.</summary>
public sealed class InboxAnalytics : FoPostModel
{
    [JsonPropertyName("totals")]
    public InboxTotals? Totals { get; set; }

    [JsonPropertyName("deltas")]
    public InboxDeltas? Deltas { get; set; }

    [JsonPropertyName("series")]
    public IList<InboxSeriesPoint> Series { get; set; } = new List<InboxSeriesPoint>();

    [JsonPropertyName("by_platform")]
    public IList<InboxPlatformCount> ByPlatform { get; set; } = new List<InboxPlatformCount>();

    [JsonPropertyName("by_type")]
    public IList<InboxTypeCount> ByType { get; set; } = new List<InboxTypeCount>();

    [JsonPropertyName("by_account")]
    public IList<InboxAccountCount> ByAccount { get; set; } = new List<InboxAccountCount>();

    [JsonPropertyName("response_time")]
    public IList<ResponseTimeBucket> ResponseTime { get; set; } = new List<ResponseTimeBucket>();

    [JsonPropertyName("heatmap")]
    public IList<HeatmapCell> Heatmap { get; set; } = new List<HeatmapCell>();

    [JsonPropertyName("time_zone")]
    public string? TimeZone { get; set; }
}

public sealed class InboxTotals : FoPostModel
{
    [JsonPropertyName("received")]
    public int? Received { get; set; }

    [JsonPropertyName("sent")]
    public int? Sent { get; set; }

    [JsonPropertyName("answered")]
    public int? Answered { get; set; }

    [JsonPropertyName("open")]
    public int? Open { get; set; }

    [JsonPropertyName("conversations")]
    public int? Conversations { get; set; }

    [JsonPropertyName("median_response_minutes")]
    public double? MedianResponseMinutes { get; set; }
}

public sealed class InboxDeltas : FoPostModel
{
    [JsonPropertyName("received")]
    public double? Received { get; set; }

    [JsonPropertyName("sent")]
    public double? Sent { get; set; }

    [JsonPropertyName("answered")]
    public double? Answered { get; set; }
}

public sealed class InboxSeriesPoint : FoPostModel
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("received")]
    public int? Received { get; set; }

    [JsonPropertyName("sent")]
    public int? Sent { get; set; }

    [JsonPropertyName("answered")]
    public int? Answered { get; set; }
}

public sealed class InboxPlatformCount : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("received")]
    public int? Received { get; set; }

    [JsonPropertyName("sent")]
    public int? Sent { get; set; }
}

public sealed class InboxTypeCount : FoPostModel
{
    /// <summary>One of <see cref="InboxItemTypes"/>, or <c>reply</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

public sealed class InboxAccountCount : FoPostModel
{
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("received")]
    public int? Received { get; set; }

    [JsonPropertyName("sent")]
    public int? Sent { get; set; }
}

public sealed class ResponseTimeBucket : FoPostModel
{
    /// <summary><c>under_1h</c>, <c>1h_6h</c>, <c>6h_24h</c>, <c>1d_3d</c> or <c>over_3d</c>.</summary>
    [JsonPropertyName("bucket")]
    public string Bucket { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

public sealed class HeatmapCell : FoPostModel
{
    [JsonPropertyName("weekday")]
    public int Weekday { get; set; }

    [JsonPropertyName("hour")]
    public int Hour { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }
}

/// <summary>What a collection run read, and which accounts it could not.</summary>
public sealed class AnalyticsCollectResult : FoPostModel
{
    [JsonPropertyName("accounts")]
    public int Accounts { get; set; }

    [JsonPropertyName("posts")]
    public int Posts { get; set; }

    [JsonPropertyName("demographics")]
    public int Demographics { get; set; }

    [JsonPropertyName("errors")]
    public int Errors { get; set; }

    [JsonPropertyName("error_details")]
    public IList<AnalyticsCollectError> ErrorDetails { get; set; } = new List<AnalyticsCollectError>();
}

public sealed class AnalyticsCollectError : FoPostModel
{
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary><c>account</c>, <c>demographics</c>, <c>timeline</c> or <c>post</c>.</summary>
    [JsonPropertyName("stage")]
    public string? Stage { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>What fixes it: <c>reconnect</c>, <c>grant</c> or <c>fund</c>.</summary>
    [JsonPropertyName("action")]
    public string? Action { get; set; }
}
