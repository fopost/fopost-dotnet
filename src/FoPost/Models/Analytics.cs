using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>One age band of the content decay report.</summary>
public sealed class DecayBand : FoPostModel
{
    [JsonPropertyName("bucket")]
    public string Bucket { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    /// <summary>Posts with at least one reading in this band.</summary>
    [JsonPropertyName("posts")]
    public int Posts { get; set; }

    [JsonPropertyName("avgEngagements")]
    public double AvgEngagements { get; set; }

    [JsonPropertyName("avgImpressions")]
    public double AvgImpressions { get; set; }

    /// <summary>
    /// Mean share of the post's final engagement reached by this age, 0-1. Null when nothing in
    /// the band had earned anything yet.
    /// </summary>
    [JsonPropertyName("shareOfFinal")]
    public double? ShareOfFinal { get; set; }
}

/// <summary>How engagement accumulates as a post ages.</summary>
public sealed class ContentDecay : FoPostModel
{
    [JsonPropertyName("days")]
    public int Days { get; set; }

    /// <summary>Posts with a publish time and at least one later reading.</summary>
    [JsonPropertyName("postsMeasured")]
    public int PostsMeasured { get; set; }

    /// <summary>First band where the average post had passed half its final engagement.</summary>
    [JsonPropertyName("halfLifeBucket")]
    public string? HalfLifeBucket { get; set; }

    [JsonPropertyName("bands")]
    public IReadOnlyList<DecayBand> Bands { get; set; } = Array.Empty<DecayBand>();
}

/// <summary>One week of posting. <c>WeekStart</c> is the Monday, UTC, as YYYY-MM-DD.</summary>
public sealed class FrequencyWeek : FoPostModel
{
    [JsonPropertyName("weekStart")]
    public string WeekStart { get; set; } = string.Empty;

    [JsonPropertyName("posts")]
    public int Posts { get; set; }

    [JsonPropertyName("engagements")]
    public long Engagements { get; set; }

    [JsonPropertyName("avgEngagementsPerPost")]
    public double AvgEngagementsPerPost { get; set; }
}

/// <summary>The weeks that shared a cadence, folded together.</summary>
public sealed class FrequencyBand : FoPostModel
{
    [JsonPropertyName("band")]
    public string Band { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("weeks")]
    public int Weeks { get; set; }

    [JsonPropertyName("posts")]
    public int Posts { get; set; }

    [JsonPropertyName("avgPostsPerWeek")]
    public double AvgPostsPerWeek { get; set; }

    [JsonPropertyName("avgEngagementsPerPost")]
    public double AvgEngagementsPerPost { get; set; }

    /// <summary>Engagements over reach, impressions as the stand-in; null with neither.</summary>
    [JsonPropertyName("engagementRate")]
    public double? EngagementRate { get; set; }
}

/// <summary>Weekly cadence set against what each cadence earned per post.</summary>
public sealed class PostingFrequency : FoPostModel
{
    [JsonPropertyName("days")]
    public int Days { get; set; }

    [JsonPropertyName("weeks")]
    public IReadOnlyList<FrequencyWeek> Weeks { get; set; } = Array.Empty<FrequencyWeek>();

    [JsonPropertyName("bands")]
    public IReadOnlyList<FrequencyBand> Bands { get; set; } = Array.Empty<FrequencyBand>();

    /// <summary>The cadence that earned the most per post; null without posts.</summary>
    [JsonPropertyName("best")]
    public FrequencyBand? Best { get; set; }
}

/// <summary>What moved between one reading and the one before it.</summary>
public sealed class TimelineDelta : FoPostModel
{
    [JsonPropertyName("impressions")]
    public long Impressions { get; set; }

    [JsonPropertyName("reach")]
    public long Reach { get; set; }

    [JsonPropertyName("engagements")]
    public long Engagements { get; set; }

    [JsonPropertyName("likes")]
    public long Likes { get; set; }

    [JsonPropertyName("comments")]
    public long Comments { get; set; }

    [JsonPropertyName("shares")]
    public long Shares { get; set; }
}

/// <summary>One reading of a post.</summary>
public sealed class TimelinePoint : FoPostModel
{
    [JsonPropertyName("at")]
    public DateTimeOffset? At { get; set; }

    /// <summary>Minutes since publication; null when the network never said when.</summary>
    [JsonPropertyName("ageMinutes")]
    public long? AgeMinutes { get; set; }

    [JsonPropertyName("impressions")]
    public long? Impressions { get; set; }

    [JsonPropertyName("reach")]
    public long? Reach { get; set; }

    [JsonPropertyName("engagements")]
    public long? Engagements { get; set; }

    [JsonPropertyName("likes")]
    public long? Likes { get; set; }

    [JsonPropertyName("comments")]
    public long? Comments { get; set; }

    [JsonPropertyName("shares")]
    public long? Shares { get; set; }

    [JsonPropertyName("videoViews")]
    public long? VideoViews { get; set; }

    [JsonPropertyName("delta")]
    public TimelineDelta Delta { get; set; } = new();
}

/// <summary>One delivery's readings: the same post on two networks decays differently.</summary>
public sealed class TimelineDelivery : FoPostModel
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("externalPostId")]
    public string ExternalPostId { get; set; } = string.Empty;

    [JsonPropertyName("postedAt")]
    public DateTimeOffset? PostedAt { get; set; }

    [JsonPropertyName("points")]
    public IReadOnlyList<TimelinePoint> Points { get; set; } = Array.Empty<TimelinePoint>();
}

/// <summary>Every reading held for one post, one timeline per delivery.</summary>
public sealed class PostTimeline : FoPostModel
{
    /// <summary>Null when the post was made natively on the network.</summary>
    [JsonPropertyName("postId")]
    public string? PostId { get; set; }

    [JsonPropertyName("deliveries")]
    public IReadOnlyList<TimelineDelivery> Deliveries { get; set; } = Array.Empty<TimelineDelivery>();
}

/// <summary>One reading, as the changes feed reports it.</summary>
public sealed class MetricChange : FoPostModel
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("externalPostId")]
    public string ExternalPostId { get; set; } = string.Empty;

    /// <summary>Null for a post made natively on the network.</summary>
    [JsonPropertyName("postId")]
    public string? PostId { get; set; }

    [JsonPropertyName("postedAt")]
    public DateTimeOffset? PostedAt { get; set; }

    [JsonPropertyName("fetchedAt")]
    public DateTimeOffset? FetchedAt { get; set; }

    [JsonPropertyName("impressions")]
    public long? Impressions { get; set; }

    [JsonPropertyName("reach")]
    public long? Reach { get; set; }

    [JsonPropertyName("engagements")]
    public long? Engagements { get; set; }

    [JsonPropertyName("likes")]
    public long? Likes { get; set; }

    [JsonPropertyName("comments")]
    public long? Comments { get; set; }

    [JsonPropertyName("shares")]
    public long? Shares { get; set; }
}

/// <summary>One page of readings. Feed <c>Cursor</c> back as the next <c>Since</c>.</summary>
public sealed class MetricChangePage : FoPostModel
{
    [JsonPropertyName("since")]
    public DateTimeOffset? Since { get; set; }

    /// <summary>Null when nothing changed.</summary>
    [JsonPropertyName("cursor")]
    public DateTimeOffset? Cursor { get; set; }

    [JsonPropertyName("hasMore")]
    public bool HasMore { get; set; }

    [JsonPropertyName("changes")]
    public IReadOnlyList<MetricChange> Changes { get; set; } = Array.Empty<MetricChange>();
}

/// <summary>What the on-demand refresh did for one delivery.</summary>
public sealed class CollectPostDelivery : FoPostModel
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("externalPostId")]
    public string ExternalPostId { get; set; } = string.Empty;

    [JsonPropertyName("collected")]
    public bool Collected { get; set; }

    [JsonPropertyName("fetchedAt")]
    public DateTimeOffset? FetchedAt { get; set; }

    /// <summary>Why the refresh did not happen.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>What one post's refresh managed.</summary>
public sealed class CollectPostResult : FoPostModel
{
    [JsonPropertyName("collected")]
    public int Collected { get; set; }

    [JsonPropertyName("deliveries")]
    public IReadOnlyList<CollectPostDelivery> Deliveries { get; set; } = Array.Empty<CollectPostDelivery>();
}

/// <summary>The freshest reading held for a post made outside FoPost.</summary>
public sealed class NativePostMetrics : FoPostModel
{
    [JsonPropertyName("impressions")]
    public long? Impressions { get; set; }

    [JsonPropertyName("reach")]
    public long? Reach { get; set; }

    [JsonPropertyName("engagements")]
    public long? Engagements { get; set; }

    [JsonPropertyName("likes")]
    public long? Likes { get; set; }

    [JsonPropertyName("comments")]
    public long? Comments { get; set; }

    [JsonPropertyName("shares")]
    public long? Shares { get; set; }

    [JsonPropertyName("videoViews")]
    public long? VideoViews { get; set; }
}

/// <summary>A post on the account that never went out through FoPost.</summary>
public sealed class NativePost : FoPostModel
{
    [JsonPropertyName("externalPostId")]
    public string ExternalPostId { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("mediaType")]
    public string? MediaType { get; set; }

    [JsonPropertyName("postedAt")]
    public DateTimeOffset? PostedAt { get; set; }

    [JsonPropertyName("fetchedAt")]
    public DateTimeOffset? FetchedAt { get; set; }

    [JsonPropertyName("metrics")]
    public NativePostMetrics Metrics { get; set; } = new();
}
