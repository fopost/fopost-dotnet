using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>The events a webhook can subscribe to.</summary>
public static class WebhookEvents
{
    public const string PostScheduled = "post.scheduled";
    public const string PostPublished = "post.published";
    public const string PostPartiallyFailed = "post.partially_failed";
    public const string PostFailed = "post.failed";
    public const string PostDeleted = "post.deleted";
    public const string DeliveryPublished = "delivery.published";
    public const string DeliveryFailed = "delivery.failed";
    public const string DeliveryDelayed = "delivery.delayed";
    public const string AccountConnected = "account.connected";
    public const string AccountDisconnected = "account.disconnected";
    public const string AccountHealthChanged = "account.health_changed";
    public const string ArticlePublished = "article.published";
    public const string InboxCommentReceived = "inbox.comment_received";
    public const string InboxMentionReceived = "inbox.mention_received";
    public const string InboxMessageReceived = "inbox.message_received";
    public const string InboxReviewReceived = "inbox.review_received";
    public const string AutomationTriggered = "automation.triggered";
    public const string AutomationCompleted = "automation.completed";
    public const string AutomationFailed = "automation.failed";
}

/// <summary>A webhook endpoint. <see cref="Secret"/> is only returned on create and update.</summary>
public sealed class Webhook : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>The signing secret; verify deliveries with <see cref="FoPostWebhooks"/>.</summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    /// <summary>See <see cref="WebhookEvents"/>.</summary>
    [JsonPropertyName("events")]
    public IList<string> Events { get; set; } = new List<string>();

    /// <summary>The names of the custom headers sent with every delivery; their values are never returned.</summary>
    [JsonPropertyName("header_names")]
    public IList<string> HeaderNames { get; set; } = new List<string>();

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("last_triggered_at")]
    public DateTimeOffset? LastTriggeredAt { get; set; }

    [JsonPropertyName("failure_count")]
    public int? FailureCount { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}

/// <summary>One attempt to deliver an event to a webhook.</summary>
public sealed class WebhookDelivery : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("webhook_id")]
    public string? WebhookId { get; set; }

    [JsonPropertyName("event")]
    public string? Event { get; set; }

    /// <summary>Shared by every attempt at the same event; what the receiver deduplicates on.</summary>
    [JsonPropertyName("delivery_key")]
    public string? DeliveryKey { get; set; }

    [JsonPropertyName("attempt")]
    public int? Attempt { get; set; }

    /// <summary><c>success</c> or <c>failed</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("response_status")]
    public int? ResponseStatus { get; set; }

    [JsonPropertyName("response_snippet")]
    public string? ResponseSnippet { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("payload_hash")]
    public string? PayloadHash { get; set; }

    [JsonPropertyName("next_retry_at")]
    public DateTimeOffset? NextRetryAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>The body that was sent. Only on <see cref="Resources.WebhooksResource.GetDeliveryAsync"/>.</summary>
    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }
}
