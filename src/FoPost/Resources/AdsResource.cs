using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Ads</c> — boosts and ads run from a connected ad account, plus the
/// audiences, targeting, predictions, and lead forms behind them. Needs the
/// <c>ads</c> scope.
/// </summary>
/// <remarks>
/// The calls that spend money, <see cref="BoostAsync"/>,
/// <see cref="CreateAsync"/>, <see cref="SetStatusAsync"/>,
/// <see cref="DeleteAsync"/> and <see cref="BulkSetStatusAsync"/>, need the
/// <c>publish</c> scope as well as <c>ads</c>. Anything created starts paused
/// unless <c>Paused</c> is set to <c>false</c>, so nothing spends until it is
/// resumed. Audiences are addressed by the ad platform's own ids plus a
/// <c>connectionId</c>, and are read live, never stored.
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

    internal AdsResource(FoPostHttpClient http)
    {
        _http = http;
        Google = new GoogleAdsResource(http);
    }

    /// <summary>
    /// The Search surface no other network has: bid strategies, ad schedules,
    /// Local Services, conversions, and raw GAQL.
    /// </summary>
    public GoogleAdsResource Google { get; }

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

    /// <summary>The ad networks this deployment knows, with what each one supports.</summary>
    public async Task<IReadOnlyList<AdProvider>> ProvidersAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/ads/providers", null, cancellationToken).ConfigureAwait(false);
        return ToList<AdProvider>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>The login URL that connects an ad account; the caller finishes it in a browser.</summary>
    public async Task<string> AuthorizeAsync(
        string provider,
        AuthorizeAdsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var path = $"/v1/ads/connections/{Uri.EscapeDataString(provider)}/authorize";
        var response = await _http.PostAsync(path, options, cancellationToken).ConfigureAwait(false);
        var url = FoPostHttpClient.Unwrap(response)?["url"]?.GetValue<string>();
        return url ?? throw new FoPostException("The API returned no authorize URL", 200);
    }

    /// <summary>The login URL that connects a Meta ad account.</summary>
    [Obsolete("Use AuthorizeAsync with the provider id \"meta\".")]
    public Task<string> AuthorizeMetaAsync(
        AuthorizeMetaAdsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        return AuthorizeAsync(
            "meta",
            new AuthorizeAdsOptions
            {
                WorkspaceId = options.WorkspaceId,
                Method = options.Method,
                ReturnTo = options.ReturnTo,
            },
            cancellationToken);
    }

    /// <summary>
    /// The Google login URL. The caller finishes it in their own browser
    /// session: the callback checks that the same user came back.
    /// </summary>
    public async Task<string> AuthorizeGoogleAsync(
        AuthorizeGoogleAdsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/connections/google/authorize", options, cancellationToken)
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

    /// <summary>
    /// TikTok's Business Centers. The one network-named read on this resource,
    /// because no other network groups ad accounts this way.
    /// </summary>
    public async Task<IReadOnlyList<AdBusinessCenter>> TikTokBusinessCentersAsync(
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(
                "/v1/ads/tiktok/business-centers",
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
        return ToList<AdBusinessCenter>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>The accounts an ad can run as; an identity id is a page id.</summary>
    public async Task<IReadOnlyList<AdIdentity>> TikTokIdentitiesAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["ad_account_id"] = adAccountId;
        var body = await _http.GetAsync("/v1/ads/tiktok/identities", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<AdIdentity>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Posts already live under an identity, each a candidate Spark ad.</summary>
    public async Task<IReadOnlyList<SparkPost>> SparkPostsAsync(
        string connectionId,
        string adAccountId,
        string identityId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["ad_account_id"] = adAccountId;
        query["identity_id"] = identityId;
        var body = await _http.GetAsync("/v1/ads/spark-posts", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<SparkPost>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Offline conversions against a pixel the ad account owns. Identifiers are
    /// hashed before anything leaves FoPost. Pass an <paramref name="idempotencyKey"/>
    /// to make a retry safe; <see cref="SendPixelConversionsAsync"/> requires one.
    /// </summary>
    public async Task<ConversionUploadResult> UploadConversionsAsync(
        UploadConversionsOptions options,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .RequestAsync(
                HttpMethod.Post,
                "/v1/ads/conversions",
                options,
                null,
                cancellationToken,
                IdempotencyHeader(idempotencyKey))
            .ConfigureAwait(false);
        return Require<ConversionUploadResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Conversions API send with a required <c>Idempotency-Key</c>: the same key
    /// answers the stored batch with <c>Replayed</c> true and sends nothing twice.
    /// </summary>
    public async Task<ConversionUploadResult> SendPixelConversionsAsync(
        UploadConversionsOptions options,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(idempotencyKey);

        var response = await _http
            .RequestAsync(
                HttpMethod.Post,
                "/v1/ads/conversions/events",
                options,
                null,
                cancellationToken,
                IdempotencyHeader(idempotencyKey))
            .ConfigureAwait(false);
        return Require<ConversionUploadResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Stored conversion batches, newest first, with hashed identifiers only.</summary>
    public async Task<IReadOnlyList<ConversionBatch>> ConversionBatchesAsync(
        string? connectionId = null,
        int? limit = null,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["connection_id"] = connectionId,
            ["limit"] = limit,
        };
        var body = await _http.GetAsync("/v1/ads/conversions/events", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<ConversionBatch>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>The pixels an ad account owns.</summary>
    public async Task<IReadOnlyList<AdPixel>> PixelsAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/pixels", AccountQuery(workspaceId, connectionId, adAccountId), cancellationToken)
            .ConfigureAwait(false);
        return ToList<AdPixel>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Creates a pixel; the answer carries the install script in <c>Code</c>.</summary>
    public async Task<AdPixelDetail> CreatePixelAsync(
        CreatePixelOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/pixels", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<AdPixelDetail>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One pixel with its install script and settings.</summary>
    public async Task<AdPixelDetail> PixelAsync(
        string pixelId,
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(PixelPath(pixelId, ""), AccountQuery(workspaceId, connectionId, adAccountId), cancellationToken)
            .ConfigureAwait(false);
        return Require<AdPixelDetail>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<AdPixelDetail> UpdatePixelAsync(
        string pixelId,
        UpdatePixelOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .RequestAsync(HttpMethod.Patch, PixelPath(pixelId, ""), options, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AdPixelDetail>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Shares the pixel with another ad account under the same business portfolio.</summary>
    public async Task SharePixelAsync(
        string pixelId,
        SharePixelOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await _http.PostAsync(PixelPath(pixelId, "/share"), options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Pixel fires per event over the last 28 days.</summary>
    public async Task<AdPixelStats> PixelStatsAsync(
        string pixelId,
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                PixelPath(pixelId, "/stats"),
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return Require<AdPixelStats>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>The network's event match quality per event name.</summary>
    public async Task<IReadOnlyList<EventMatchQuality>> EventMatchQualityAsync(
        string pixelId,
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                PixelPath(pixelId, "/match-quality"),
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return ToList<EventMatchQuality>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>The custom conversions defined on the ad account's pixels.</summary>
    public async Task<IReadOnlyList<CustomConversion>> CustomConversionsAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                "/v1/ads/custom-conversions",
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return ToList<CustomConversion>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<CustomConversion> CreateCustomConversionAsync(
        CreateCustomConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/custom-conversions", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<CustomConversion>(FoPostHttpClient.Unwrap(response));
    }

    public async Task<CustomConversion> CustomConversionAsync(
        string customConversionId,
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                CustomConversionPath(customConversionId),
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return Require<CustomConversion>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<CustomConversion> UpdateCustomConversionAsync(
        string customConversionId,
        UpdateCustomConversionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .RequestAsync(
                HttpMethod.Patch,
                CustomConversionPath(customConversionId),
                options,
                null,
                cancellationToken)
            .ConfigureAwait(false);
        return Require<CustomConversion>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Deletes the custom conversion on the network.</summary>
    public async Task DeleteCustomConversionAsync(
        string customConversionId,
        string workspaceId,
        string connectionId,
        string adAccountId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, string>
        {
            ["workspaceId"] = workspaceId,
            ["connectionId"] = connectionId,
            ["adAccountId"] = adAccountId,
        };
        await _http.DeleteAsync(CustomConversionPath(customConversionId), body, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>One page of an ad's comments; pass <c>nextCursor</c> back as <c>after</c>.</summary>
    public async Task<AdCommentsPage> CommentsAsync(
        string connectionId,
        string adId,
        string? after = null,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["ad_id"] = adId;
        query["after"] = after;
        var body = await _http.GetAsync("/v1/ads/comments", query, cancellationToken).ConfigureAwait(false);
        return Require<AdCommentsPage>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Answer a comment on an ad; returns the reply's id on the network. Needs
    /// the <c>publish</c> scope as well as <c>ads</c>.
    /// </summary>
    public async Task<string> ReplyToCommentAsync(
        string commentId,
        AdCommentOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync(CommentPath(commentId) + "/reply", options, cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["replyId"]?.GetValue<string>() ?? string.Empty;
    }

    /// <summary>
    /// Hide or show a comment on an ad. Needs the <c>publish</c> scope as well
    /// as <c>ads</c>.
    /// </summary>
    public async Task SetCommentHiddenAsync(
        string commentId,
        AdCommentOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await _http.PostAsync(CommentPath(commentId) + "/hide", options, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Remove a comment from the ad on the network. One already gone succeeds.
    /// Needs the <c>publish</c> scope as well as <c>ads</c>.
    /// </summary>
    public async Task DeleteCommentAsync(
        string commentId,
        AdCommentOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await _http.DeleteAsync(CommentPath(commentId), options, cancellationToken).ConfigureAwait(false);
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

    /// <summary>
    /// Adds companies to a company-list audience and returns how many the network took. The rows
    /// travel with the request and are never stored.
    /// </summary>
    public async Task<int> AddAudienceCompaniesAsync(
        string audienceId,
        string workspaceId,
        string connectionId,
        IEnumerable<AdCompanyOptions> companies,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(companies);

        var body = new Dictionary<string, object?> { ["companies"] = companies.ToList() };
        var response = await _http
            .RequestAsync(
                HttpMethod.Post,
                $"{ObjectPath("audiences", audienceId)}/companies",
                body,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["added"]?.GetValue<int>() ?? 0;
    }

    /// <summary>What the auction currently costs for that audience.</summary>
    public async Task<BidPricing> BidPricingAsync(
        AdForecastOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/linkedin/bid-pricing", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<BidPricing>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>What that audience would deliver at that budget.</summary>
    public async Task<SupplyForecast> SupplyForecastAsync(
        AdForecastOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/linkedin/supply-forecast", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<SupplyForecast>(FoPostHttpClient.Unwrap(response));
    }

    public async Task<IReadOnlyList<ConversionRule>> ConversionRulesAsync(
        string? workspaceId,
        string connectionId,
        string adAccountId,
        CancellationToken cancellationToken = default)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["ad_account_id"] = adAccountId;
        var body = await _http.GetAsync("/v1/ads/linkedin/conversion-rules", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<ConversionRule>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Creates a conversion rule and returns its id.</summary>
    public async Task<string> CreateConversionRuleAsync(
        CreateConversionRuleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/ads/linkedin/conversion-rules", options, cancellationToken)
            .ConfigureAwait(false);
        var id = FoPostHttpClient.Unwrap(response)?["id"]?.GetValue<string>();
        return id ?? throw new FoPostException("The API returned no id for the rule", 201);
    }

    public async Task<ConversionRule> ConversionRuleAsync(
        string ruleId,
        string? workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(ConversionRulePath(ruleId, ""), MetaQuery(workspaceId, connectionId), cancellationToken)
            .ConfigureAwait(false);
        return Require<ConversionRule>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<ConversionRule> UpdateConversionRuleAsync(
        string ruleId,
        string workspaceId,
        string connectionId,
        UpdateConversionRuleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .RequestAsync(
                HttpMethod.Patch,
                ConversionRulePath(ruleId, ""),
                options,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
        return Require<ConversionRule>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Turns the rule off; the network keeps the history.</summary>
    public async Task DeleteConversionRuleAsync(
        string ruleId,
        string workspaceId,
        string connectionId,
        CancellationToken cancellationToken = default)
    {
        await _http
            .RequestAsync(
                HttpMethod.Delete,
                ConversionRulePath(ruleId, ""),
                null,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<ConversionRule> AttachConversionRuleAsync(
        string ruleId,
        string workspaceId,
        string connectionId,
        string campaignId,
        CancellationToken cancellationToken = default) =>
        AssociationAsync(HttpMethod.Post, ruleId, workspaceId, connectionId, campaignId, cancellationToken);

    public Task<ConversionRule> DetachConversionRuleAsync(
        string ruleId,
        string workspaceId,
        string connectionId,
        string campaignId,
        CancellationToken cancellationToken = default) =>
        AssociationAsync(HttpMethod.Delete, ruleId, workspaceId, connectionId, campaignId, cancellationToken);

    /// <summary>What the rule recorded between two <c>YYYY-MM-DD</c> days, inclusive.</summary>
    public async Task<ConversionMetrics> ConversionMetricsAsync(
        string ruleId,
        string? workspaceId,
        string connectionId,
        string since,
        string until,
        CancellationToken cancellationToken = default)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["since"] = since;
        query["until"] = until;
        var body = await _http
            .GetAsync(ConversionRulePath(ruleId, "/metrics"), query, cancellationToken)
            .ConfigureAwait(false);
        return Require<ConversionMetrics>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Sends conversions back to the network and returns how many it took. Each event needs an
    /// email or a click id; the address is hashed inside the API and nothing is stored.
    /// </summary>
    public async Task<int> SendConversionEventsAsync(
        string ruleId,
        string workspaceId,
        string connectionId,
        IEnumerable<ConversionEventOptions> events,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(events);

        var body = new Dictionary<string, object?> { ["events"] = events.ToList() };
        var response = await _http
            .RequestAsync(
                HttpMethod.Post,
                ConversionRulePath(ruleId, "/events"),
                body,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["accepted"]?.GetValue<int>() ?? 0;
    }


    private async Task<ConversionRule> AssociationAsync(
        HttpMethod method,
        string ruleId,
        string workspaceId,
        string connectionId,
        string campaignId,
        CancellationToken cancellationToken)
    {
        var body = new Dictionary<string, object?> { ["campaignId"] = campaignId };
        var response = await _http
            .RequestAsync(
                method,
                ConversionRulePath(ruleId, "/associations"),
                body,
                MetaQuery(workspaceId, connectionId),
                cancellationToken)
            .ConfigureAwait(false);
        return Require<ConversionRule>(FoPostHttpClient.Unwrap(response));
    }

    private static string ConversionRulePath(string ruleId, string suffix) =>
        $"/v1/ads/linkedin/conversion-rules/{Uri.EscapeDataString(ruleId)}{suffix}";

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

    // ─── Goals ──────────────────────────────────────────────────────

    /// <summary>
    /// The goals this connection's ad platform can run right now. Ask rather than assume: a goal
    /// the deployment is not set up for is absent here and is refused if you send it anyway.
    /// </summary>
    public async Task<IReadOnlyList<string>> GoalsAsync(
        string connectionId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/goals", MetaQuery(workspaceId, connectionId), cancellationToken)
            .ConfigureAwait(false);
        return ToList<string>(FoPostHttpClient.Unwrap(body));
    }

    // ─── Reach and frequency ────────────────────────────────────────

    public async Task<ReachFrequencyResult> ReachFrequencyAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/reach-frequency", AccountQuery(workspaceId, connectionId, adAccountId), cancellationToken)
            .ConfigureAwait(false);
        return Require<ReachFrequencyResult>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Prices a flight. Nothing is bought until you reserve it.</summary>
    public Task<ReachFrequencyPrediction> CreateReachFrequencyAsync(
        CreateReachFrequencyOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<ReachFrequencyPrediction>("/v1/ads/reach-frequency", options, cancellationToken);

    public async Task<ReachFrequencyPrediction> GetReachFrequencyAsync(
        string predictionId,
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                ObjectPath("reach-frequency", predictionId),
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return Require<ReachFrequencyPrediction>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Holds the inventory the prediction priced. Needs the <c>publish</c> scope as well as
    /// <c>ads</c>.
    /// </summary>
    public Task<ReachFrequencyPrediction> ReserveReachFrequencyAsync(
        string predictionId,
        ReachFrequencyActionOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<ReachFrequencyPrediction>(
            $"{ObjectPath("reach-frequency", predictionId)}/reserve",
            options,
            cancellationToken);

    /// <summary>Needs the <c>publish</c> scope as well as <c>ads</c>.</summary>
    public Task<ReachFrequencyPrediction> CancelReachFrequencyAsync(
        string predictionId,
        ReachFrequencyActionOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<ReachFrequencyPrediction>(
            $"{ObjectPath("reach-frequency", predictionId)}/cancel",
            options,
            cancellationToken);

    // ─── Partnership ads ────────────────────────────────────────────

    /// <summary>Creators who allowlisted this Page to run partnership ads on their posts.</summary>
    public async Task<IReadOnlyList<PartnershipCreator>> PartnershipCreatorsAsync(
        string connectionId,
        string pageId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["page_id"] = pageId;
        var body = await _http
            .GetAsync("/v1/ads/partnership/creators", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<PartnershipCreator>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Asks a creator for permission and returns the list as it now stands.</summary>
    public async Task<IReadOnlyList<PartnershipCreator>> RequestPartnershipAsync(
        PartnershipOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = await _http
            .PostAsync("/v1/ads/partnership/creators", options, cancellationToken)
            .ConfigureAwait(false);
        return ToList<PartnershipCreator>(FoPostHttpClient.Unwrap(body));
    }

    public async Task RevokePartnershipAsync(
        string creatorId,
        string workspaceId,
        string connectionId,
        string pageId,
        CancellationToken cancellationToken = default)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["page_id"] = pageId;
        await _http
            .RequestAsync(
                HttpMethod.Delete,
                $"/v1/ads/partnership/creators/{Uri.EscapeDataString(creatorId)}",
                null,
                query,
                cancellationToken)
            .ConfigureAwait(false);
    }

    // ─── Ad account settings ────────────────────────────────────────

    /// <summary>Who changed what on the ad account, and when. Dates are <c>YYYY-MM-DD</c>.</summary>
    public async Task<AdActivityResult> AccountActivityAsync(
        string connectionId,
        string adAccountId,
        string? since = null,
        string? until = null,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = AccountQuery(workspaceId, connectionId, adAccountId);
        query["since"] = since;
        query["until"] = until;
        var body = await _http
            .GetAsync("/v1/ads/account/activity", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<AdActivityResult>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<IReadOnlyList<AdLabel>> LabelsAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/account/labels", AccountQuery(workspaceId, connectionId, adAccountId), cancellationToken)
            .ConfigureAwait(false);
        return ToList<AdLabel>(FoPostHttpClient.Unwrap(body));
    }

    public Task<AdLabel> CreateLabelAsync(
        AdLabelOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<AdLabel>("/v1/ads/account/labels", options, cancellationToken);

    public Task<AdLabel> UpdateLabelAsync(
        string labelId,
        string workspaceId,
        string connectionId,
        AdLabelOptions options,
        CancellationToken cancellationToken = default) =>
        PatchObject<AdLabel>(LabelPath(labelId), workspaceId, connectionId, options, cancellationToken);

    public Task DeleteLabelAsync(
        string labelId,
        string workspaceId,
        string connectionId,
        string adAccountId,
        CancellationToken cancellationToken = default) =>
        DeleteAccountObject(LabelPath(labelId), workspaceId, connectionId, adAccountId, cancellationToken);

    /// <summary>Keeps whatever labels the object already carries.</summary>
    public async Task ApplyLabelAsync(
        string labelId,
        ApplyAdLabelOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        await _http.PostAsync($"{LabelPath(labelId)}/apply", options, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AdStudy>> StudiesAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync("/v1/ads/account/studies", AccountQuery(workspaceId, connectionId, adAccountId), cancellationToken)
            .ConfigureAwait(false);
        return ToList<AdStudy>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Splits traffic evenly across the cells for the length of the flight.</summary>
    public Task<AdStudy> CreateStudyAsync(
        CreateAdStudyOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<AdStudy>("/v1/ads/account/studies", options, cancellationToken);

    public async Task<AdStudy> GetStudyAsync(
        string studyId,
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(StudyPath(studyId), AccountQuery(workspaceId, connectionId, adAccountId), cancellationToken)
            .ConfigureAwait(false);
        return Require<AdStudy>(FoPostHttpClient.Unwrap(body));
    }

    public Task DeleteStudyAsync(
        string studyId,
        string workspaceId,
        string connectionId,
        string adAccountId,
        CancellationToken cancellationToken = default) =>
        DeleteAccountObject(StudyPath(studyId), workspaceId, connectionId, adAccountId, cancellationToken);

    /// <summary>How many iOS 14 campaigns the account may run at once, per app.</summary>
    public async Task<IReadOnlyList<IosCampaignLimits>> IosCampaignLimitsAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                "/v1/ads/account/ios-limits",
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return ToList<IosCampaignLimits>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<IReadOnlyList<HighDemandPeriod>> HighDemandPeriodsAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                "/v1/ads/account/high-demand-periods",
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return ToList<HighDemandPeriod>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Tells the ad platform to expect heavier spend over a window, so pacing allows for it.</summary>
    public Task<HighDemandPeriod> CreateHighDemandPeriodAsync(
        CreateHighDemandPeriodOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<HighDemandPeriod>("/v1/ads/account/high-demand-periods", options, cancellationToken);

    public Task DeleteHighDemandPeriodAsync(
        string periodId,
        string workspaceId,
        string connectionId,
        string adAccountId,
        CancellationToken cancellationToken = default) =>
        DeleteAccountObject(
            $"/v1/ads/account/high-demand-periods/{Uri.EscapeDataString(periodId)}",
            workspaceId,
            connectionId,
            adAccountId,
            cancellationToken);

    public async Task<IReadOnlyList<ValueRuleSet>> ValueRuleSetsAsync(
        string connectionId,
        string adAccountId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(
                "/v1/ads/account/value-rule-sets",
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
        return ToList<ValueRuleSet>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Weights conversions so some audiences count for more than others.</summary>
    public Task<ValueRuleSet> CreateValueRuleSetAsync(
        CreateValueRuleSetOptions options,
        CancellationToken cancellationToken = default) =>
        PostObject<ValueRuleSet>("/v1/ads/account/value-rule-sets", options, cancellationToken);

    public Task DeleteValueRuleSetAsync(
        string ruleSetId,
        string workspaceId,
        string connectionId,
        string adAccountId,
        CancellationToken cancellationToken = default) =>
        DeleteAccountObject(
            $"/v1/ads/account/value-rule-sets/{Uri.EscapeDataString(ruleSetId)}",
            workspaceId,
            connectionId,
            adAccountId,
            cancellationToken);

    private async Task DeleteAccountObject(
        string path,
        string workspaceId,
        string connectionId,
        string adAccountId,
        CancellationToken cancellationToken)
    {
        await _http
            .RequestAsync(
                HttpMethod.Delete,
                path,
                null,
                AccountQuery(workspaceId, connectionId, adAccountId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static Dictionary<string, object?> AccountQuery(
        string? workspaceId,
        string connectionId,
        string adAccountId)
    {
        var query = MetaQuery(workspaceId, connectionId);
        query["ad_account_id"] = adAccountId;
        return query;
    }

    private static string LabelPath(string labelId) =>
        $"/v1/ads/account/labels/{Uri.EscapeDataString(labelId)}";

    private static string StudyPath(string studyId) =>
        $"/v1/ads/account/studies/{Uri.EscapeDataString(studyId)}";

    private static string CommentPath(string commentId) =>
        $"/v1/ads/comments/{Uri.EscapeDataString(commentId)}";

    private static Dictionary<string, object?> MetaQuery(string? workspaceId, string connectionId) =>
        new() { ["workspace_id"] = workspaceId, ["connection_id"] = connectionId };

    private static string PixelPath(string pixelId, string suffix) =>
        $"/v1/ads/pixels/{Uri.EscapeDataString(pixelId)}{suffix}";

    private static string CustomConversionPath(string id) =>
        $"/v1/ads/custom-conversions/{Uri.EscapeDataString(id)}";

    private static IEnumerable<KeyValuePair<string, string>>? IdempotencyHeader(string? key) =>
        string.IsNullOrEmpty(key) ? null : new[] { KeyValuePair.Create("Idempotency-Key", key) };
}
