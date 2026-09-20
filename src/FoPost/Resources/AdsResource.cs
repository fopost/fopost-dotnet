using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Ads</c> — boosts and ads run from a connected ad account, plus the
/// audiences, targeting, and lead forms behind them. Needs the <c>ads</c> scope.
/// </summary>
/// <remarks>
/// The calls that spend money — <see cref="BoostAsync"/>,
/// <see cref="CreateAsync"/>, <see cref="SetStatusAsync"/>,
/// <see cref="DeleteAsync"/>, <see cref="BulkSetStatusAsync"/>, and the
/// create, update, delete, and duplicate calls on campaigns, ad sets, and
/// network ads — need the <c>publish</c> scope as well as <c>ads</c>.
/// Anything created starts paused unless <c>Paused</c> is set to
/// <c>false</c>, so nothing spends until it is resumed. Campaigns, ad sets,
/// network ads, creatives, and audiences are addressed by the ad platform's
/// own ids plus a <c>connectionId</c>, and are read live, never stored.
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

    /// <summary>
    /// The login URL that connects an ad account; the caller finishes it in a browser. The
    /// options name the ad network; one that is not available on the deployment answers 503.
    /// </summary>
    public async Task<string> AuthorizeAsync(
        AuthorizeAdsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var provider = string.IsNullOrEmpty(options.Provider) ? "meta" : options.Provider;
        var response = await _http
            .PostAsync($"/v1/ads/connections/{provider}/authorize", options, cancellationToken)
            .ConfigureAwait(false);
        var url = FoPostHttpClient.Unwrap(response)?["url"]?.GetValue<string>();
        return url ?? throw new FoPostException("The API returned no authorize URL", 200);
    }

    /// <summary>The Meta login URL.</summary>
    [Obsolete("Use AuthorizeAsync, which takes a Provider.")]
    public Task<string> AuthorizeMetaAsync(
        AuthorizeAdsOptions options,
        CancellationToken cancellationToken = default) =>
        AuthorizeAsync(options, cancellationToken);

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

    /// <summary>
    /// An ad account's campaigns, each with its ad sets and their ads. Read live
    /// from the ad platform, never stored.
    /// </summary>
    public Task<AdAccountTree> AccountTreeAsync(
        string adAccountId,
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        GetObject<AdAccountTree>(
            $"/v1/ads/accounts/{Uri.EscapeDataString(adAccountId)}/tree",
            connectionId,
            workspaceId,
            cancellationToken);

    /// <summary>
    /// Create a campaign. Needs the <c>publish</c> scope as well as <c>ads</c>.
    /// Starts paused unless <c>Paused</c> is <c>false</c>.
    /// </summary>
    public Task<AdCampaign> CreateCampaignAsync(
        CreateAdCampaignOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<AdCampaign>("/v1/ads/campaigns", options, cancellationToken);

    public Task<AdCampaign> GetCampaignAsync(
        string campaignId,
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        GetObject<AdCampaign>(ObjectPath("campaigns", campaignId), connectionId, workspaceId, cancellationToken);

    /// <summary>Rename, pause, or resume a campaign. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task<AdCampaign> UpdateCampaignAsync(
        string campaignId,
        string workspaceId,
        string connectionId,
        UpdateAdCampaignOptions options,
        CancellationToken cancellationToken = default) =>
        PatchObject<AdCampaign>(ObjectPath("campaigns", campaignId), workspaceId, connectionId, options, cancellationToken);

    /// <summary>Delete a campaign on the ad platform. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task DeleteCampaignAsync(
        string campaignId,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default) =>
        DeleteObject(ObjectPath("campaigns", campaignId), workspaceId, connectionId, cancellationToken);

    /// <summary>
    /// Copy a campaign and return the copy's id. Needs the <c>publish</c> scope
    /// as well as <c>ads</c>. The copy starts paused unless <paramref name="paused"/> is <c>false</c>.
    /// </summary>
    public Task<string> DuplicateCampaignAsync(
        string campaignId,
        string workspaceId,
        string connectionId,
        bool? paused = null,
        CancellationToken cancellationToken = default) =>
        Duplicate(ObjectPath("campaigns", campaignId), workspaceId, connectionId, paused, cancellationToken);

    /// <summary>
    /// Create an ad set in a campaign. Needs the <c>publish</c> scope as well as
    /// <c>ads</c>. Starts paused unless <c>Paused</c> is <c>false</c>.
    /// </summary>
    public Task<AdSet> CreateAdSetAsync(
        CreateAdSetOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<AdSet>("/v1/ads/ad-sets", options, cancellationToken);

    public Task<AdSet> GetAdSetAsync(
        string adSetId,
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        GetObject<AdSet>(ObjectPath("ad-sets", adSetId), connectionId, workspaceId, cancellationToken);

    /// <summary>Change an ad set's name, status, budget, end, or targeting. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task<AdSet> UpdateAdSetAsync(
        string adSetId,
        string workspaceId,
        string connectionId,
        UpdateAdSetOptions options,
        CancellationToken cancellationToken = default) =>
        PatchObject<AdSet>(ObjectPath("ad-sets", adSetId), workspaceId, connectionId, options, cancellationToken);

    /// <summary>Delete an ad set on the ad platform. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task DeleteAdSetAsync(
        string adSetId,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default) =>
        DeleteObject(ObjectPath("ad-sets", adSetId), workspaceId, connectionId, cancellationToken);

    /// <summary>Copy an ad set and return the copy's id. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task<string> DuplicateAdSetAsync(
        string adSetId,
        string workspaceId,
        string connectionId,
        bool? paused = null,
        CancellationToken cancellationToken = default) =>
        Duplicate(ObjectPath("ad-sets", adSetId), workspaceId, connectionId, paused, cancellationToken);

    /// <summary>
    /// Create an ad inside an ad set from an existing creative. Needs the
    /// <c>publish</c> scope as well as <c>ads</c>. Starts paused unless
    /// <c>Paused</c> is <c>false</c>. Unlike <see cref="CreateAsync"/>, nothing is stored in FoPost.
    /// </summary>
    public Task<NetworkAd> CreateNetworkAdAsync(
        CreateNetworkAdOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<NetworkAd>("/v1/ads/ads", options, cancellationToken);

    public Task<NetworkAd> GetNetworkAdAsync(
        string adId,
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        GetObject<NetworkAd>(ObjectPath("ads", adId), connectionId, workspaceId, cancellationToken);

    /// <summary>Rename, pause, resume, or swap the creative of an ad. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task<NetworkAd> UpdateNetworkAdAsync(
        string adId,
        string workspaceId,
        string connectionId,
        UpdateNetworkAdOptions options,
        CancellationToken cancellationToken = default) =>
        PatchObject<NetworkAd>(ObjectPath("ads", adId), workspaceId, connectionId, options, cancellationToken);

    /// <summary>Delete an ad on the ad platform. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task DeleteNetworkAdAsync(
        string adId,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default) =>
        DeleteObject(ObjectPath("ads", adId), workspaceId, connectionId, cancellationToken);

    /// <summary>Copy an ad and return the copy's id. Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task<string> DuplicateNetworkAdAsync(
        string adId,
        string workspaceId,
        string connectionId,
        bool? paused = null,
        CancellationToken cancellationToken = default) =>
        Duplicate(ObjectPath("ads", adId), workspaceId, connectionId, paused, cancellationToken);

    /// <summary>
    /// Pause or resume up to 50 campaigns, ad sets, and ads at once; each object
    /// reports its own outcome. Needs the <c>publish</c> scope as well as <c>ads</c>.
    /// </summary>
    public async Task<IReadOnlyList<BulkAdStatusResult>> BulkSetStatusAsync(
        BulkAdStatusOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/status", options, cancellationToken).ConfigureAwait(false);
        return ToList<BulkAdStatusResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The creatives on one ad account.</summary>
    public async Task<CreativesResult> CreativesAsync(
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
        var body = await _http.GetAsync("/v1/ads/creatives", query, cancellationToken).ConfigureAwait(false);
        return Require<CreativesResult>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Build an image, video, or carousel creative to use in <see cref="CreateNetworkAdAsync"/>.</summary>
    public Task<NetworkCreative> CreateCreativeAsync(
        CreateAdCreativeOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<NetworkCreative>("/v1/ads/creatives", options, cancellationToken);

    public Task<NetworkCreative> GetCreativeAsync(
        string creativeId,
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        GetObject<NetworkCreative>(ObjectPath("creatives", creativeId), connectionId, workspaceId, cancellationToken);

    public Task DeleteCreativeAsync(
        string creativeId,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default) =>
        DeleteObject(ObjectPath("creatives", creativeId), workspaceId, connectionId, cancellationToken);

    public Task<Audience> GetAudienceAsync(
        string audienceId,
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        GetObject<Audience>(ObjectPath("audiences", audienceId), connectionId, workspaceId, cancellationToken);

    public Task<Audience> UpdateAudienceAsync(
        string audienceId,
        string workspaceId,
        string connectionId,
        UpdateAudienceOptions options,
        CancellationToken cancellationToken = default) =>
        PatchObject<Audience>(ObjectPath("audiences", audienceId), workspaceId, connectionId, options, cancellationToken);

    public Task DeleteAudienceAsync(
        string audienceId,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default) =>
        DeleteObject(ObjectPath("audiences", audienceId), workspaceId, connectionId, cancellationToken);

    /// <summary>
    /// Add a customer list to a custom audience; the emails are hashed before
    /// they leave the API. Returns how many were sent.
    /// </summary>
    public async Task<int> AddAudienceUsersAsync(
        string audienceId,
        string workspaceId,
        string connectionId,
        IEnumerable<string> emails,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emails);

        var body = new Dictionary<string, object?> { ["emails"] = emails.ToList() };
        var response = await _http
            .RequestAsync(
                HttpMethod.Post,
                $"{ObjectPath("audiences", audienceId)}/users",
                body,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["added"]?.GetValue<int>() ?? 0;
    }

    /// <summary>How many people a targeting spec could reach. <c>Ready</c> is false while the ad platform is still estimating.</summary>
    public Task<ReachEstimate> EstimateReachAsync(
        EstimateReachOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<ReachEstimate>("/v1/ads/reach-estimate", options, cancellationToken);

    /// <summary>
    /// Insights for any campaign, ad set, or ad on the ad platform over
    /// <paramref name="since"/> to <paramref name="until"/> (<c>YYYY-MM-DD</c>),
    /// optionally split by one of <see cref="AdInsightsBreakdowns"/> and by day.
    /// </summary>
    public async Task<AdInsightsReport> InsightsAsync(
        string connectionId,
        string objectId,
        string since,
        string until,
        string? breakdown = null,
        bool? daily = null,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["connection_id"] = connectionId,
            ["object_id"] = objectId,
            ["since"] = since,
            ["until"] = until,
            ["breakdown"] = breakdown,
            ["daily"] = daily,
        };
        var body = await _http.GetAsync("/v1/ads/insights", query, cancellationToken).ConfigureAwait(false);
        return Require<AdInsightsReport>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Insights for a boost or ad created through FoPost, by its FoPost id.</summary>
    public async Task<AdInsightsReport> AdInsightsAsync(
        string adId,
        string workspaceId,
        string since,
        string until,
        string? breakdown = null,
        bool? daily = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["since"] = since,
            ["until"] = until,
            ["breakdown"] = breakdown,
            ["daily"] = daily,
        };
        var body = await _http.GetAsync($"{AdPath(adId)}/insights", query, cancellationToken).ConfigureAwait(false);
        return Require<AdInsightsReport>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>One lead form with its questions and settings.</summary>
    public async Task<LeadFormDetail> GetLeadFormAsync(
        string formId,
        string connectionId,
        string pageId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["connection_id"] = connectionId,
            ["page_id"] = pageId,
        };
        var body = await _http
            .GetAsync(LeadFormPath(formId), query, cancellationToken)
            .ConfigureAwait(false);
        return Require<LeadFormDetail>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Archive a lead form so it stops collecting leads.</summary>
    public async Task<LeadFormDetail> ArchiveLeadFormAsync(
        string formId,
        string workspaceId,
        string connectionId,
        string pageId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["connectionId"] = connectionId,
            ["pageId"] = pageId,
        };
        var response = await _http
            .PostAsync($"{LeadFormPath(formId)}/archive", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<LeadFormDetail>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Leads stored from subscribed Pages, newest first. Pass
    /// <c>NextCursor</c> back as <paramref name="cursor"/> for the next page.
    /// </summary>
    public async Task<LeadsFeedPage> LeadsFeedAsync(
        string? workspaceId = null,
        string? formId = null,
        string? pageId = null,
        string? cursor = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["form_id"] = formId,
            ["page_id"] = pageId,
            ["cursor"] = cursor,
            ["limit"] = limit,
        };
        var body = await _http.GetAsync("/v1/ads/leads", query, cancellationToken).ConfigureAwait(false);
        return Require<LeadsFeedPage>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Pages whose new leads are collected into <see cref="LeadsFeedAsync"/>.</summary>
    public Task<IReadOnlyList<LeadPage>> LeadPagesAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListByWorkspace<LeadPage>("/v1/ads/lead-pages", workspaceId, cancellationToken);

    /// <summary>Start collecting a Page's leads; recent leads are backfilled.</summary>
    public async Task<SubscribedLeadPage> SubscribeLeadPageAsync(
        string workspaceId,
        string connectionId,
        string pageId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["connectionId"] = connectionId,
            ["pageId"] = pageId,
        };
        var response = await _http.PostAsync("/v1/ads/lead-pages", body, cancellationToken).ConfigureAwait(false);
        return Require<SubscribedLeadPage>(FoPostHttpClient.Unwrap(response));
    }

    public async Task UnsubscribeLeadPageAsync(
        string pageId,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        await _http
            .RequestAsync(
                HttpMethod.Delete,
                $"/v1/ads/lead-pages/{Uri.EscapeDataString(pageId)}",
                null,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<T> GetObject<T>(
        string path,
        string connectionId,
        string? workspaceId,
        CancellationToken cancellationToken)
    {
        var body = await _http.GetAsync(path, MetaQuery(workspaceId, connectionId), cancellationToken)
            .ConfigureAwait(false);
        return Require<T>(FoPostHttpClient.Unwrap(body));
    }

    private async Task<T> PostObject<T>(string path, object options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync(path, options, cancellationToken).ConfigureAwait(false);
        return Require<T>(FoPostHttpClient.Unwrap(response));
    }

    private async Task<T> PatchObject<T>(
        string path,
        string workspaceId,
        string connectionId,
        object options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .RequestAsync(HttpMethod.Patch, path, options, MetaQuery(workspaceId, connectionId), cancellationToken)
            .ConfigureAwait(false);
        return Require<T>(FoPostHttpClient.Unwrap(response));
    }

    private async Task DeleteObject(
        string path,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken)
    {
        await _http
            .RequestAsync(HttpMethod.Delete, path, null, MetaQuery(workspaceId, connectionId), cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<string> Duplicate(
        string path,
        string workspaceId,
        string connectionId,
        bool? paused,
        CancellationToken cancellationToken)
    {
        var body = paused is null ? null : new Dictionary<string, object?> { ["paused"] = paused };
        var response = await _http
            .RequestAsync(
                HttpMethod.Post,
                $"{path}/duplicate",
                body,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
        var id = FoPostHttpClient.Unwrap(response)?["id"]?.GetValue<string>();
        return id ?? throw new FoPostException("The API returned no id for the copy", 201);
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

    private static string ObjectPath(string kind, string id) => $"/v1/ads/{kind}/{Uri.EscapeDataString(id)}";

    private static string LeadFormPath(string formId) => $"/v1/ads/lead-forms/{Uri.EscapeDataString(formId)}";

    private static Dictionary<string, object?> MetaQuery(string? workspaceId, string connectionId) =>
        new() { ["workspace_id"] = workspaceId, ["connection_id"] = connectionId };
}
