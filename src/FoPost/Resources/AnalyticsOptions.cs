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

/// <summary>Window and scope for the time series and label reports.</summary>
public sealed class AnalyticsRangeOptions
{
    /// <summary>Days back from now; ignored when <see cref="From"/> and <see cref="To"/> are set.</summary>
    public int? Days { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public string? WorkspaceId { get; set; }

    public string? AccountId { get; set; }
}

/// <summary>Filters for <see cref="Resources.AnalyticsResource.TopPostsAsync"/>.</summary>
public sealed class TopPostsOptions
{
    public int? Limit { get; set; }

    public int? Days { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public string? WorkspaceId { get; set; }

    public string? AccountId { get; set; }

    /// <summary><c>recent</c> orders by date instead of engagement.</summary>
    public string? Sort { get; set; }

    /// <summary>Only posts carrying this label id.</summary>
    public string? Label { get; set; }
}

/// <summary>Filters for <see cref="Resources.AnalyticsResource.PostsTableAsync"/>.</summary>
public sealed class PostsTableOptions
{
    public int? Page { get; set; }

    public int? PerPage { get; set; }

    public int? Days { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public string? WorkspaceId { get; set; }

    public string? AccountId { get; set; }
}

/// <summary>Scope for <see cref="Resources.AnalyticsResource.DemographicsAsync"/>.</summary>
public sealed class DemographicsOptions
{
    /// <summary><c>followers</c> (default), <c>engaged</c> or <c>reached</c>.</summary>
    public string? Audience { get; set; }

    public string? WorkspaceId { get; set; }

    public string? AccountId { get; set; }
}

/// <summary>Window and scope for <see cref="Resources.AnalyticsResource.InboxAsync"/>.</summary>
public sealed class InboxAnalyticsOptions
{
    public int? Days { get; set; }

    public string? WorkspaceId { get; set; }

    public string? AccountId { get; set; }

    /// <summary>IANA zone the heatmap and daily series are bucketed in.</summary>
    public string? TimeZone { get; set; }
}
