using System.Collections;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>Activity kinds. <see cref="Security"/> is the audit log.</summary>
public static class ActivityKinds
{
    public const string Publish = "publish";
    public const string Connection = "connection";
    public const string Webhook = "webhook";
    public const string Inbox = "inbox";
    public const string Automation = "automation";
    public const string Billing = "billing";

    /// <summary>The audit trail. Its rows are append-only and never expire.</summary>
    public const string Security = "security";
}

/// <summary>Who did it: <c>user</c>, <c>api_key</c>, <c>agent</c> or <c>system</c>.</summary>
public sealed class ActivityActor : FoPostModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>One thing that happened in a workspace.</summary>
public sealed class ActivityEvent : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("ref_type")]
    public string? RefType { get; set; }

    [JsonPropertyName("ref_id")]
    public string? RefId { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("actor")]
    public ActivityActor Actor { get; set; } = new();

    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }
}

/// <summary>
/// One page of activity, newest first. Pass <see cref="NextCursor"/> back as the cursor for the
/// next page; it is <c>null</c> at the end of the list.
/// </summary>
public sealed class ActivityPage : IReadOnlyList<ActivityEvent>
{
    public ActivityPage(IReadOnlyList<ActivityEvent> items, string? nextCursor)
    {
        Items = items;
        NextCursor = nextCursor;
    }

    public IReadOnlyList<ActivityEvent> Items { get; }

    public string? NextCursor { get; }

    public int Count => Items.Count;

    public ActivityEvent this[int index] => Items[index];

    public IEnumerator<ActivityEvent> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
