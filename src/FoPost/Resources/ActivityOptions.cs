namespace FoPost;

/// <summary>
/// Filters for the activity log. Leave <see cref="WorkspaceId"/> unset to read every workspace
/// the key can reach.
/// </summary>
public sealed class ListActivityOptions
{
    public string? WorkspaceId { get; set; }

    /// <summary>One of <see cref="ActivityKinds"/>.</summary>
    public string? Kind { get; set; }

    /// <summary>ISO 8601. Only events at or after this time.</summary>
    public string? From { get; set; }

    /// <summary>ISO 8601. Only events at or before this time.</summary>
    public string? To { get; set; }

    /// <summary>The <c>NextCursor</c> of the previous page.</summary>
    public string? Cursor { get; set; }

    public int? Limit { get; set; }
}
