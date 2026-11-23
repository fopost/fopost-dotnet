using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Ads.Google</c> — the Google Ads surface no other network has:
/// bid strategies, ad schedules, Local Services leads, conversions, and raw GAQL.
/// </summary>
/// <remarks>
/// Ads, audiences, and insights are on
/// <see cref="AdsResource"/> and dispatch by connection; a connection on
/// another network answers 400 here. Every call needs the <c>ads</c> scope,
/// and anything that changes what a live account serves or bids also needs
/// <c>publish</c>. Amounts are in the account's currency, in minor units.
/// </remarks>
/// <example>
/// <code>
/// var scope = new GoogleAdsScope { ConnectionId = "c1d2e3f4-…", CustomerId = "1234567890" };
/// var strategies = await client.Ads.Google.BidStrategiesAsync(scope);
/// </code>
/// </example>
public sealed class GoogleAdsResource
{
    private readonly FoPostHttpClient _http;

    internal GoogleAdsResource(FoPostHttpClient http) => _http = http;

    // ── Bid strategies and ad schedule ──

    /// <summary>The account's portfolio bid strategies.</summary>
    public async Task<IReadOnlyList<GoogleBidStrategy>> BidStrategiesAsync(
        GoogleAdsScope scope,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/ads/google/bid-strategies", Query(scope), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleBidStrategy>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Add a bid strategy. Needs <c>publish</c> as well as <c>ads</c>.</summary>
    public Task<string> CreateBidStrategyAsync(
        CreateGoogleBidStrategyOptions options,
        CancellationToken cancellationToken = default) =>
        PostIdAsync("/v1/ads/google/bid-strategies", options, cancellationToken);

    /// <summary>A campaign's ad schedule.</summary>
    public async Task<IReadOnlyList<GoogleAdScheduleSlot>> AdScheduleAsync(
        GoogleAdsScope scope,
        string campaignId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/google/ad-schedule", Query(scope, ("campaign_id", campaignId)), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleAdScheduleSlot>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Replace a campaign's schedule; the slots given replace every slot on it.
    /// Needs <c>publish</c> as well as <c>ads</c>.
    /// </summary>
    public async Task<int> SetAdScheduleAsync(
        SetGoogleAdScheduleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PutAsync("/v1/ads/google/ad-schedule", options, cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["slots"]?.GetValue<int>() ?? 0;
    }

    // ── Local Services leads ──

    /// <summary>Leads from Local Services Ads, read live and never stored.</summary>
    public async Task<IReadOnlyList<GoogleLocalServicesLead>> LocalServicesLeadsAsync(
        GoogleAdsScope scope,
        string since,
        string until,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/google/local-services", Query(scope, ("since", since), ("until", until)), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleLocalServicesLead>(FoPostHttpClient.Unwrap(body));
    }

    // ── Conversions ──

    /// <summary>The account's conversion actions.</summary>
    public async Task<IReadOnlyList<GoogleConversionAction>> ConversionActionsAsync(
        GoogleAdsScope scope,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/ads/google/conversions", Query(scope), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleConversionAction>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Add a conversion action. Needs <c>publish</c> as well as <c>ads</c>.</summary>
    public Task<string> CreateConversionActionAsync(
        CreateGoogleConversionActionOptions options,
        CancellationToken cancellationToken = default) =>
        PostIdAsync("/v1/ads/google/conversions", options, cancellationToken);

    /// <summary>Send offline conversions; answers how many landed. Needs <c>publish</c>.</summary>
    public Task<int> UploadConversionsAsync(
        UploadGoogleConversionsOptions options,
        CancellationToken cancellationToken = default) =>
        UploadedAsync("/v1/ads/google/conversions/upload", options, cancellationToken);

    /// <summary>Send conversion adjustments; answers how many landed. Needs <c>publish</c>.</summary>
    public Task<int> UploadConversionAdjustmentsAsync(
        UploadGoogleConversionAdjustmentsOptions options,
        CancellationToken cancellationToken = default) =>
        UploadedAsync("/v1/ads/google/conversions/adjustments", options, cancellationToken);

    // ── GAQL ──

    /// <summary>Run a read-only GAQL SELECT; rows come back as Google sends them.</summary>
    public async Task<GoogleQueryResult> QueryAsync(
        GoogleQueryOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/insights/query", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<GoogleQueryResult>(FoPostHttpClient.Unwrap(response));
    }

    private async Task<string> PostIdAsync(string path, object options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync(path, options, cancellationToken).ConfigureAwait(false);
        return ReadId(response);
    }

    private Task<int> UploadedAsync(string path, object options, CancellationToken cancellationToken) =>
        CountedAsync(path, "uploaded", options, cancellationToken);

    private async Task<int> CountedAsync(
        string path,
        string key,
        object options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync(path, options, cancellationToken).ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?[key]?.GetValue<int>() ?? 0;
    }

    private static string ReadId(System.Text.Json.Nodes.JsonNode? response) =>
        FoPostHttpClient.Unwrap(response)?["id"]?.GetValue<string>() ?? string.Empty;

    private static Dictionary<string, object?> Query(
        GoogleAdsScope scope,
        params (string Key, string? Value)[] extra)
    {
        ArgumentNullException.ThrowIfNull(scope);

        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = scope.WorkspaceId,
            ["connection_id"] = scope.ConnectionId,
            ["customer_id"] = scope.CustomerId,
        };
        foreach (var (key, value) in extra)
        {
            query[key] = value;
        }

        return query;
    }
}
