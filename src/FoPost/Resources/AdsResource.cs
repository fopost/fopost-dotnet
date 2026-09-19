using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Ads</c> — boosts and ads run from a connected ad account, plus the
/// audiences, targeting, and lead forms behind them. Needs the <c>ads</c> scope.
/// </summary>
/// <remarks>
/// The four calls that spend money — <see cref="BoostAsync"/>,
/// <see cref="CreateAsync"/>, <see cref="SetStatusAsync"/>, and
/// <see cref="DeleteAsync"/> — need the <c>publish</c> scope as well as
/// <c>ads</c>. A boost or ad starts paused unless <c>Paused</c> is set to
/// <c>false</c>, so nothing spends until it is resumed.
/// </remarks>
/// <example>
/// <code>
/// var ad = await client.Ads.BoostAsync(new BoostPostOptions
/// {
///     WorkspaceId = "9b2f6c1e-…",
///     ConnectionId = "c1d2e3f4-…",
///     AdAccountId = "act_123",
///     PostId = post.Id,
///     AccountId = post.Accounts[0].Id,
///     Name = "Launch boost",
///     Goal = AdGoals.Engagement,
///     Budget = new AdBudget(2000, AdBudgetTypes.Daily),
///     Targeting = new AdTargeting { Countries = new List&lt;string&gt; { "US" } },
/// });
///
/// await client.Ads.SetStatusAsync(ad.Id, ad.WorkspaceId!, AdStatuses.Active);
/// </code>
/// </example>
public sealed class AdsResource
{
    private readonly FoPostHttpClient _http;

    internal AdsResource(FoPostHttpClient http) => _http = http;

    /// <summary>Boosts and ads created through FoPost, with insights from their last refresh.</summary>
    public Task<IReadOnlyList<Ad>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListByWorkspace<Ad>("/v1/ads", workspaceId, cancellationToken);

    /// <summary>Ads on the connected ad accounts that were made elsewhere. Read live, never stored.</summary>
    public Task<IReadOnlyList<ExternalAd>> ExternalAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListByWorkspace<ExternalAd>("/v1/ads/external", workspaceId, cancellationToken);

    /// <summary>Published posts that can be boosted.</summary>
    public Task<IReadOnlyList<BoostablePost>> BoostableAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListByWorkspace<BoostablePost>("/v1/ads/boostable", workspaceId, cancellationToken);

    public Task<IReadOnlyList<AdConnection>> ConnectionsAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListByWorkspace<AdConnection>("/v1/ads/connections", workspaceId, cancellationToken);

    /// <summary>Each connection with the ad accounts and Pages its grant reaches.</summary>
    public Task<IReadOnlyList<AdSource>> SourcesAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListByWorkspace<AdSource>("/v1/ads/sources", workspaceId, cancellationToken);

    /// <summary>The login URL that connects an ad account; the caller finishes it in a browser.</summary>
    public async Task<string> AuthorizeMetaAsync(
        AuthorizeMetaAdsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/connections/meta/authorize", options, cancellationToken)
            .ConfigureAwait(false);
        var url = FoPostHttpClient.Unwrap(response)?["url"]?.GetValue<string>();
        return url ?? throw new FoPostException("The API returned no authorize URL", 200);
    }

    /// <summary>Also deletes every ad record created through the connection.</summary>
    public async Task DeleteConnectionAsync(
        string connectionId,
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        await _http
            .RequestAsync(
                HttpMethod.Delete,
                $"/v1/ads/connections/{Uri.EscapeDataString(connectionId)}",
                null,
                WorkspaceQuery(workspaceId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Promote a published post. Needs the <c>publish</c> scope as well as
    /// <c>ads</c>. Starts paused unless <c>Paused</c> is <c>false</c>.
    /// </summary>
    public async Task<Ad> BoostAsync(
        BoostPostOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/boost", options, cancellationToken).ConfigureAwait(false);
        return Require<Ad>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Build an ad from scratch. Needs the <c>publish</c> scope as well as
    /// <c>ads</c>. Starts paused unless <c>Paused</c> is <c>false</c>.
    /// </summary>
    public async Task<Ad> CreateAsync(
        CreateAdOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads", options, cancellationToken).ConfigureAwait(false);
        return Require<Ad>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Read the delivery status and lifetime insights from the ad platform.</summary>
    public async Task<Ad> RefreshAsync(
        string adId,
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .RequestAsync(
                HttpMethod.Post,
                $"{AdPath(adId)}/refresh",
                null,
                WorkspaceQuery(workspaceId),
                cancellationToken)
            .ConfigureAwait(false);
        return Require<Ad>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Resume or pause delivery with one of <see cref="AdStatuses"/>. Needs the
    /// <c>publish</c> scope as well as <c>ads</c>.
    /// </summary>
    public async Task<Ad> SetStatusAsync(
        string adId,
        string workspaceId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["status"] = status };
        var response = await _http
            .RequestAsync(HttpMethod.Patch, AdPath(adId), body, WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
        return Require<Ad>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// End delivery and delete the ad on the ad platform. Needs the
    /// <c>publish</c> scope as well as <c>ads</c>.
    /// </summary>
    public async Task DeleteAsync(
        string adId,
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        await _http
            .RequestAsync(HttpMethod.Delete, AdPath(adId), null, WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>The saved audiences and pixels on one ad account.</summary>
    public async Task<AudiencesResult> AudiencesAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["connection_id"] = connectionId,
            ["ad_account_id"] = adAccountId,
        };
        var body = await _http.GetAsync("/v1/ads/audiences", query, cancellationToken).ConfigureAwait(false);
        return Require<AudiencesResult>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Build a custom, lookalike, or website audience; see <see cref="AudienceSpec"/>.</summary>
    public async Task<CreatedAudience> CreateAudienceAsync(
        CreateAudienceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/audiences", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<CreatedAudience>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Locations, interests, behaviours, and income brackets as the ad platform
    /// names them; <c>type</c> is one of <see cref="TargetingSearchTypes"/>.
    /// </summary>
    public async Task<IReadOnlyList<TargetingOption>> SearchTargetingAsync(
        string connectionId,
        string type,
        string? q = null,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["connection_id"] = connectionId,
            ["type"] = type,
            ["q"] = q,
        };
        var body = await _http.GetAsync("/v1/ads/targeting/search", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<TargetingOption>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Each connection's Page with the lead forms on it.</summary>
    public Task<IReadOnlyList<LeadFormSource>> LeadFormsAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListByWorkspace<LeadFormSource>("/v1/ads/lead-forms", workspaceId, cancellationToken);

    /// <summary>Create a lead form on a Page. Returns the form id.</summary>
    public async Task<string> CreateLeadFormAsync(
        CreateLeadFormOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/lead-forms", options, cancellationToken)
            .ConfigureAwait(false);
        var id = FoPostHttpClient.Unwrap(response)?["id"]?.GetValue<string>();
        return id ?? throw new FoPostException("The API returned no lead form id", 200);
    }

    /// <summary>One page of leads; pass <c>NextCursor</c> back as <paramref name="after"/> for the next.</summary>
    public async Task<LeadsPage> LeadsAsync(
        string formId,
        string connectionId,
        string pageId,
        string? after = null,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["connection_id"] = connectionId,
            ["page_id"] = pageId,
            ["after"] = after,
        };
        var body = await _http
            .GetAsync($"/v1/ads/lead-forms/{Uri.EscapeDataString(formId)}/leads", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<LeadsPage>(FoPostHttpClient.Unwrap(body));
    }

    private async Task<IReadOnlyList<T>> ListByWorkspace<T>(
        string path,
        string? workspaceId,
        CancellationToken cancellationToken)
    {
        var body = await _http.GetAsync(path, WorkspaceQuery(workspaceId), cancellationToken).ConfigureAwait(false);
        return ToList<T>(FoPostHttpClient.Unwrap(body));
    }

    private static Dictionary<string, object?> WorkspaceQuery(string? workspaceId) =>
        new() { ["workspace_id"] = workspaceId };

    private static string AdPath(string adId) => $"/v1/ads/{Uri.EscapeDataString(adId)}";
}
