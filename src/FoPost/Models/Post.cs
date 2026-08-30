using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>An account a post is targeted at, plus its per-account delivery state.</summary>
public sealed class PostAccount : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("publish_status")]
    public string? PublishStatus { get; set; }

    [JsonPropertyName("posted_at")]
    public DateTimeOffset? PostedAt { get; set; }

    [JsonPropertyName("platform_post_id")]
    public string? PlatformPostId { get; set; }

    [JsonPropertyName("external_url")]
    public string? ExternalUrl { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("attempts")]
    public int? Attempts { get; set; }

    [JsonPropertyName("max_attempts")]
    public int? MaxAttempts { get; set; }
}

/// <summary>A scheduled, drafted, or published post.</summary>
public sealed class Post : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }

    /// <summary>See <see cref="PostStatuses"/> for the values the API uses.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    [JsonPropertyName("schedule_at")]
    public DateTimeOffset? ScheduleAt { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("repeatable")]
    public bool? Repeatable { get; set; }

    [JsonPropertyName("repeatable_times")]
    public int? RepeatableTimes { get; set; }

    [JsonPropertyName("repeatable_gap")]
    public int? RepeatableGap { get; set; }

    [JsonPropertyName("repeatable_gap_unit")]
    public string? RepeatableGapUnit { get; set; }

    [JsonPropertyName("remaining_posts")]
    public int? RemainingPosts { get; set; }

    [JsonPropertyName("auto_plug")]
    public bool? AutoPlug { get; set; }

    [JsonPropertyName("auto_plug_content")]
    public string? AutoPlugContent { get; set; }

    [JsonPropertyName("approved_at")]
    public DateTimeOffset? ApprovedAt { get; set; }

    [JsonPropertyName("rejection_reason")]
    public string? RejectionReason { get; set; }

    [JsonPropertyName("content")]
    public IList<ContentBlock> Content { get; set; } = new List<ContentBlock>();

    [JsonPropertyName("accounts")]
    public IList<PostAccount> Accounts { get; set; } = new List<PostAccount>();

    [JsonPropertyName("labels")]
    public IList<Label> Labels { get; set; } = new List<Label>();

    /// <summary>Per-platform overrides, keyed by platform name.</summary>
    [JsonPropertyName("settings")]
    public IDictionary<string, JsonElement> Settings { get; set; } = new Dictionary<string, JsonElement>();

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>One post-to-account delivery attempt.</summary>
public sealed class Delivery : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("account_name")]
    public string? AccountName { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("attempts")]
    public int? Attempts { get; set; }

    [JsonPropertyName("max_attempts")]
    public int? MaxAttempts { get; set; }

    [JsonPropertyName("scheduled_publish_at")]
    public DateTimeOffset? ScheduledPublishAt { get; set; }

    [JsonPropertyName("delay_reason")]
    public string? DelayReason { get; set; }

    [JsonPropertyName("delay_message")]
    public string? DelayMessage { get; set; }

    [JsonPropertyName("posted_at")]
    public DateTimeOffset? PostedAt { get; set; }

    [JsonPropertyName("last_attempt_at")]
    public DateTimeOffset? LastAttemptAt { get; set; }

    [JsonPropertyName("platform_post_id")]
    public string? PlatformPostId { get; set; }

    [JsonPropertyName("external_url")]
    public string? ExternalUrl { get; set; }
}
