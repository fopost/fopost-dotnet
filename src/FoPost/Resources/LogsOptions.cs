namespace FoPost;

/// <summary>Filters for <see cref="Resources.LogsResource.ListAsync"/>.</summary>
public sealed class ListLogsOptions
{
    public string? WorkspaceId { get; set; }

    /// <summary>How many days back to read.</summary>
    public int? Days { get; set; }

    /// <summary><c>success</c> or <c>failed</c>.</summary>
    public string? Status { get; set; }

    public string? Method { get; set; }

    public int? StatusCode { get; set; }

    public string? Platform { get; set; }

    public string? Route { get; set; }

    /// <summary>Free-text search over the path and message.</summary>
    public string? Q { get; set; }

    public int? Limit { get; set; }

    /// <summary>The <see cref="RequestLogPage.NextCursor"/> of the previous page.</summary>
    public string? Before { get; set; }
}
