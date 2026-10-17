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
