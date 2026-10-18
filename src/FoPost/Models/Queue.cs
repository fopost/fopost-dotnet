using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>The value of <c>Schedule</c> on a post that should take the next free posting slot.</summary>
public static class PostSchedules
{
    public const string Queue = "queue";
}

/// <summary>A weekly posting slot: a weekday and a wall-clock time in a timezone.</summary>
public sealed class QueueSlot : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }

    /// <summary>Null when the slot applies to every account in the workspace.</summary>
    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    /// <summary>0 (Sunday) to 6 (Saturday).</summary>
    [JsonPropertyName("weekday")]
    public int Weekday { get; set; }

    /// <summary><c>HH:mm</c>.</summary>
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>Queued posts moved to a new time by a change to this slot.</summary>
    [JsonPropertyName("moved")]
    public IList<QueueMove>? Moved { get; set; }
}

/// <summary>A queued post that moved from one slot time to another.</summary>
public sealed class QueueMove : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public DateTimeOffset? From { get; set; }

    [JsonPropertyName("to")]
    public DateTimeOffset? To { get; set; }
}

/// <summary>The outcome of removing a slot.</summary>
public sealed class QueueSlotDeletion : FoPostModel
{
    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("moved")]
    public IList<QueueMove> Moved { get; set; } = new List<QueueMove>();
}

/// <summary>The next free slot, or null when the workspace has no slots.</summary>
public sealed class QueueNext : FoPostModel
{
    [JsonPropertyName("next")]
    public DateTimeOffset? Next { get; set; }
}
