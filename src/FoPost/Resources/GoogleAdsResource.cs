using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Ads.Google</c> — the Google Ads surface no other network has:
/// keywords, assets, Performance Max asset groups, Local Services leads,
/// conversions, and raw GAQL.
/// </summary>
/// <remarks>
/// Campaigns, ad groups, ads, audiences, and insights are on
/// <see cref="AdsResource"/> and dispatch by connection; a connection on
/// another network answers 400 here. Every call needs the <c>ads</c> scope,
/// and anything that changes what a live account serves or bids also needs
/// <c>publish</c>. Amounts are in the account's currency, in minor units.
/// </remarks>
/// <example>
/// <code>
/// var scope = new GoogleAdsScope { ConnectionId = "c1d2e3f4-…", CustomerId = "1234567890" };
/// var keywords = await client.Ads.Google.KeywordsAsync(scope);
/// </code>
/// </example>
public sealed class GoogleAdsResource
{
    private readonly FoPostHttpClient _http;

    internal GoogleAdsResource(FoPostHttpClient http) => _http = http;

    // ── Keywords ──

    /// <summary>Keywords on the account, or on one ad group.</summary>
    public async Task<IReadOnlyList<GoogleKeyword>> KeywordsAsync(
        GoogleAdsScope scope,
        string? adGroupId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/ads/google/keywords", Query(scope, ("ad_group_id", adGroupId)), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleKeyword>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Add a keyword. Needs <c>publish</c> as well as <c>ads</c>.</summary>
    public Task<string> CreateKeywordAsync(
        CreateGoogleKeywordOptions options,
        CancellationToken cancellationToken = default) =>
        PostIdAsync("/v1/ads/google/keywords", options, cancellationToken);

    /// <summary>Pause, resume, or rebid a keyword. Needs <c>publish</c>.</summary>
    public async Task<string> UpdateKeywordAsync(
        string keywordId,
        UpdateGoogleKeywordOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .RequestAsync(HttpMethod.Patch, $"/v1/ads/google/keywords/{Uri.EscapeDataString(keywordId)}", options, null, cancellationToken)
            .ConfigureAwait(false);
        return ReadId(response);
    }

    /// <summary>Remove a keyword. Needs <c>publish</c> as well as <c>ads</c>.</summary>
    public Task DeleteKeywordAsync(
        string keywordId,
        GoogleAdsScope scope,
        CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"/v1/ads/google/keywords/{Uri.EscapeDataString(keywordId)}", scope, cancellationToken);

    /// <summary>Ideas from seed keywords, a landing page, or both.</summary>
    public async Task<IReadOnlyList<GoogleKeywordIdea>> KeywordIdeasAsync(
        GoogleKeywordIdeasOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/google/keyword-ideas", options, cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleKeywordIdea>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Historical metrics for keywords you already have.</summary>
    public async Task<IReadOnlyList<GoogleKeywordIdea>> KeywordMetricsAsync(
        GoogleKeywordMetricsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/google/keyword-metrics", options, cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleKeywordIdea>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>What people actually searched, with the metrics each term earned.</summary>
    public async Task<IReadOnlyList<GoogleSearchTerm>> SearchTermsAsync(
        GoogleAdsScope scope,
        string since,
        string until,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/google/search-terms", Query(scope, ("since", since), ("until", until)), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleSearchTerm>(FoPostHttpClient.Unwrap(body));
    }

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

    // ── Negative keyword lists ──

    /// <summary>The account's negative keyword lists.</summary>
    public async Task<IReadOnlyList<GoogleSharedSet>> NegativeKeywordListsAsync(
        GoogleAdsScope scope,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/ads/google/negative-keywords", Query(scope), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleSharedSet>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Create a negative keyword list. Needs <c>publish</c>.</summary>
    public Task<string> CreateNegativeKeywordListAsync(
        CreateGoogleNegativeKeywordListOptions options,
        CancellationToken cancellationToken = default) =>
        PostIdAsync("/v1/ads/google/negative-keywords", options, cancellationToken);

    /// <summary>Add keywords to a list; answers how many landed. Needs <c>publish</c>.</summary>
    public async Task<int> AddNegativeKeywordsAsync(
        AddGoogleNegativeKeywordsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/google/negative-keywords/keywords", options, cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["added"]?.GetValue<int>() ?? 0;
    }

    /// <summary>Put a list on a campaign. Needs <c>publish</c> as well as <c>ads</c>.</summary>
    public Task AttachNegativeKeywordListAsync(
        AttachGoogleNegativeKeywordListOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        return _http.PostAsync("/v1/ads/google/negative-keywords/attach", options, cancellationToken);
    }

    // ── Assets ──

    /// <summary>Sitelinks, callouts, and snippets, with the links that place each one.</summary>
    public async Task<GoogleAssetsResult> AssetsAsync(
        GoogleAdsScope scope,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/ads/google/assets", Query(scope), cancellationToken)
            .ConfigureAwait(false);
        return Require<GoogleAssetsResult>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Add an asset to the library. Needs <c>publish</c>.</summary>
    public Task<string> CreateAssetAsync(
        CreateGoogleAssetOptions options,
        CancellationToken cancellationToken = default) =>
        PostIdAsync("/v1/ads/google/assets", options, cancellationToken);

    /// <summary>Put an asset under the ads it belongs to. Needs <c>publish</c>.</summary>
    public Task AttachAssetAsync(
        AttachGoogleAssetOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        return _http.PostAsync("/v1/ads/google/assets/attach", options, cancellationToken);
    }

    /// <summary>
    /// Remove the links that put an asset under an ad; on Google the asset
    /// itself is permanent. Needs <c>publish</c> as well as <c>ads</c>.
    /// </summary>
    public Task DeleteAssetAsync(
        string assetId,
        GoogleAdsScope scope,
        CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"/v1/ads/google/assets/{Uri.EscapeDataString(assetId)}", scope, cancellationToken);

    // ── Performance Max asset groups ──

    /// <summary>Performance Max asset groups on the account, or on one campaign.</summary>
    public async Task<IReadOnlyList<GoogleAssetGroup>> AssetGroupsAsync(
        GoogleAdsScope scope,
        string? campaignId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/google/asset-groups", Query(scope, ("campaign_id", campaignId)), cancellationToken)
            .ConfigureAwait(false);
        return ToList<GoogleAssetGroup>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Create an asset group. Needs <c>publish</c> as well as <c>ads</c>.</summary>
    public Task<string> CreateAssetGroupAsync(
        CreateGoogleAssetGroupOptions options,
        CancellationToken cancellationToken = default) =>
        PostIdAsync("/v1/ads/google/asset-groups", options, cancellationToken);

    /// <summary>Rename, pause, or resume an asset group. Needs <c>publish</c>.</summary>
    public async Task<string> UpdateAssetGroupAsync(
        string assetGroupId,
        UpdateGoogleAssetGroupOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .RequestAsync(HttpMethod.Patch, $"/v1/ads/google/asset-groups/{Uri.EscapeDataString(assetGroupId)}", options, null, cancellationToken)
            .ConfigureAwait(false);
        return ReadId(response);
    }

    /// <summary>Remove an asset group. Needs <c>publish</c> as well as <c>ads</c>.</summary>
    public Task DeleteAssetGroupAsync(
        string assetGroupId,
        GoogleAdsScope scope,
        CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"/v1/ads/google/asset-groups/{Uri.EscapeDataString(assetGroupId)}", scope, cancellationToken);

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

    private async Task<int> UploadedAsync(string path, object options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync(path, options, cancellationToken).ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["uploaded"]?.GetValue<int>() ?? 0;
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
