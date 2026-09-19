namespace FoPost.Resources;

/// <summary>Window and scope shared by the decay and frequency reports.</summary>
public sealed class AnalyticsScopeOptions
{
    /// <summary>Only posts published in the last this many days.</summary>
    public int? Days { get; set; }

    public string? WorkspaceId { get; set; }

    /// <summary>Narrow to one connected account.</summary>
    public string? AccountId { get; set; }
}

/// <summary>Filters for the changes feed.</summary>
public sealed class MetricChangesOptions
{
    /// <summary>Return readings recorded after this instant. Unset asks for the last seven days.</summary>
    public DateTimeOffset? Since { get; set; }

    public int? Limit { get; set; }

    public string? WorkspaceId { get; set; }

    public string? AccountId { get; set; }
}

/// <summary>Pagination for the posts made outside FoPost.</summary>
public sealed class NativePostsOptions
{
    public int? Page { get; set; }

    public int? PerPage { get; set; }

    /// <summary>Only posts published in the last this many days.</summary>
    public int? Days { get; set; }
}
