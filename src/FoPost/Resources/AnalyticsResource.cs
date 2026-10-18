using System.Text.Json;
using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Analytics</c> — deeper posting analytics, derived from the repeated readings the
/// platform collector takes of every post as it ages.
/// </summary>
/// <remarks>Every call needs an API key carrying the <c>analytics</c> scope.</remarks>
public sealed class AnalyticsResource
{
    private readonly FoPostHttpClient _http;

    internal AnalyticsResource(FoPostHttpClient http) => _http = http;

    /// <summary>
    /// How long a post keeps earning: engagement grouped by the post's age at each reading.
    /// <c>Days</c> selects posts by publish time, not reading time.
    /// </summary>
    public async Task<ContentDecay> DecayAsync(
        AnalyticsScopeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("/v1/analytics/decay", Scope(options), cancellationToken)
            .ConfigureAwait(false);
        return Require<ContentDecay>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Whether posting more earned more: weekly cadence against what it earned per post.</summary>
    public async Task<PostingFrequency> FrequencyAsync(
        AnalyticsScopeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("/v1/analytics/frequency", Scope(options), cancellationToken)
            .ConfigureAwait(false);
        return Require<PostingFrequency>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Every reading held for one post, oldest first, with what moved between them and one
    /// timeline per delivery.
    /// </summary>
    /// <param name="idOrPermalink">
    /// A FoPost post id, or the permalink of a post made natively on the network.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<PostTimeline> TimelineAsync(
        string idOrPermalink,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(idOrPermalink);

        var path = $"/v1/analytics/posts/{Uri.EscapeDataString(idOrPermalink)}/timeline";
        var response = await _http.GetAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<PostTimeline>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Readings recorded after <c>Since</c>, oldest first, with a cursor to continue. Poll it to
    /// mirror the metrics into your own store instead of refetching the whole history.
    /// </summary>
    public async Task<MetricChangePage> ChangesAsync(
        MetricChangesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["since"] = options?.Since is { } since ? Iso(since) : null,
            ["limit"] = options?.Limit,
            ["workspace_id"] = options?.WorkspaceId,
            ["accountId"] = options?.AccountId,
        };

        var response = await _http.GetAsync("/v1/analytics/changes", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetricChangePage>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Re-read one post from the network now. Spends the same per-user budget as a full collection
    /// run, so a burst answers 429.
    /// </summary>
    /// <param name="idOrPermalink">
    /// A FoPost post id, or the permalink of a post made natively on the network.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<CollectPostResult> CollectPostAsync(
        string idOrPermalink,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(idOrPermalink);

        var path = $"/v1/posts/{Uri.EscapeDataString(idOrPermalink)}/analytics/collect";
        var response = await _http.PostAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<CollectPostResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Posts on the account that never went out through FoPost, newest first.</summary>
    public async Task<InboxPage<NativePost>> NativePostsAsync(
        string accountId,
        NativePostsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(accountId);

        var query = new Dictionary<string, object?>
        {
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
            ["days"] = options?.Days,
        };

        var body = await _http.GetAsync($"/v1/accounts/{accountId}/native-posts", query, cancellationToken)
            .ConfigureAwait(false);
        return new InboxPage<NativePost>(
            ToList<NativePost>(FoPostHttpClient.Unwrap(body)),
            ReadMeta(body));
    }

    // ─── Dashboard reports ───────────────────────────────────────────────

    /// <summary>Totals across the workspace or one account, with the change against the window before.</summary>
    public async Task<AnalyticsOverview> OverviewAsync(
        AnalyticsScopeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["days"] = options?.Days,
            ["workspace_id"] = options?.WorkspaceId,
            ["account_id"] = options?.AccountId,
        };
        var response = await _http.GetAsync("/v1/analytics/overview", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<AnalyticsOverview>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Daily engagement, impressions, followers and posts over the window.</summary>
    public async Task<AnalyticsTimeSeries> TimeSeriesAsync(
        AnalyticsRangeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("/v1/analytics/time-series", Range(options), cancellationToken)
            .ConfigureAwait(false);
        return Require<AnalyticsTimeSeries>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The posts that earned the most, best first.</summary>
    public async Task<IReadOnlyList<TopPost>> TopPostsAsync(
        TopPostsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["limit"] = options?.Limit,
            ["days"] = options?.Days,
            ["from"] = options?.From is { } from ? Iso(from) : null,
            ["to"] = options?.To is { } to ? Iso(to) : null,
            ["workspace_id"] = options?.WorkspaceId,
            ["account_id"] = options?.AccountId,
            ["sort"] = options?.Sort,
            ["label"] = options?.Label,
        };
        var response = await _http.GetAsync("/v1/analytics/top-posts", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<TopPost>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>What the posts under each label earned together: a campaign roll-up.</summary>
    public async Task<IReadOnlyList<LabelAnalytics>> LabelsAsync(
        AnalyticsRangeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync("/v1/analytics/labels", Range(options), cancellationToken)
            .ConfigureAwait(false);
        return ToList<LabelAnalytics>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Posts with their per-network delivery state, paged, plus status counts for the window.</summary>
    public async Task<PostsTable> PostsTableAsync(
        PostsTableOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
            ["days"] = options?.Days,
            ["from"] = options?.From is { } from ? Iso(from) : null,
            ["to"] = options?.To is { } to ? Iso(to) : null,
            ["workspace_id"] = options?.WorkspaceId,
            ["account_id"] = options?.AccountId,
        };
        // meta and summary sit beside data, so the body is read whole rather than unwrapped.
        var response = await _http.GetAsync("/v1/analytics/posts-table", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<PostsTable>(response);
    }

    /// <summary>Posting activity for each of the last 365 days.</summary>
    public async Task<IReadOnlyList<PostingStreakDay>> PostingStreakAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var response = await _http.GetAsync("/v1/analytics/posting-streak", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<PostingStreakDay>((response as JsonObject)?["streak"]);
    }

    /// <summary>Who the audience is, on the networks that publish it.</summary>
    public async Task<AudienceDemographics> DemographicsAsync(
        DemographicsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["audience"] = options?.Audience,
            ["workspace_id"] = options?.WorkspaceId,
            ["account_id"] = options?.AccountId,
        };
        var response = await _http.GetAsync("/v1/analytics/demographics", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<AudienceDemographics>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Inbox volume, response times and who answers, over the window.</summary>
    public async Task<InboxAnalytics> InboxAsync(
        InboxAnalyticsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["days"] = options?.Days,
            ["workspace_id"] = options?.WorkspaceId,
            ["account_id"] = options?.AccountId,
            ["tz"] = options?.TimeZone,
        };
        var response = await _http.GetAsync("/v1/analytics/inbox", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxAnalytics>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Read every account from its network now instead of waiting for the next scheduled run.
    /// Rate limited per user, so a burst answers 429.
    /// </summary>
    public async Task<AnalyticsCollectResult> CollectAsync(
        string? workspaceId = null,
        string? accountId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["account_id"] = accountId,
        };
        var response = await _http
            .RequestAsync(HttpMethod.Post, "/v1/analytics/collect", (object?)null, query, cancellationToken)
            .ConfigureAwait(false);
        return Require<AnalyticsCollectResult>(FoPostHttpClient.Unwrap(response));
    }

    private static Dictionary<string, object?> Range(AnalyticsRangeOptions? options) => new()
    {
        ["days"] = options?.Days,
        ["from"] = options?.From is { } from ? Iso(from) : null,
        ["to"] = options?.To is { } to ? Iso(to) : null,
        ["workspace_id"] = options?.WorkspaceId,
        ["account_id"] = options?.AccountId,
    };

    /// <summary>The native-posts listing carries the inbox-style page footer.</summary>
    private static InboxPageMeta ReadMeta(JsonNode? body)
    {
        if (body is JsonObject obj &&
            obj.TryGetPropertyValue("meta", out var meta) &&
            meta is JsonObject)
        {
            return meta.Deserialize<InboxPageMeta>(FoPostJson.Options) ?? new InboxPageMeta();
        }

        return new InboxPageMeta();
    }

    private static Dictionary<string, object?> Scope(AnalyticsScopeOptions? options) => new()
    {
        ["days"] = options?.Days,
        ["workspace_id"] = options?.WorkspaceId,
        ["accountId"] = options?.AccountId,
    };
}
