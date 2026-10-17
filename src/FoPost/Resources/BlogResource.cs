using System.Globalization;
using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Blog</c> — articles written for a website, planned from keywords and
/// published to the configured target.
/// </summary>
/// <remarks>
/// Reads, settings and keyword edits need the <c>posts</c> scope. Anything that spends
/// credits (<see cref="SuggestSeedsAsync"/>, <see cref="PlanKeywordsAsync"/>, <see cref="RequestArticleAsync"/>,
/// <see cref="RegenerateArticleAsync"/>) needs <c>ai</c>, and
/// <see cref="PublishArticleAsync"/> needs <c>publish</c>. Every request is bound to one
/// workspace, passed as the first argument. Wire fields are camelCase on these routes.
/// </remarks>
/// <example>
/// <code>
/// var article = await client.Blog.RequestArticleAsync("9b2f6c1e-…", new RequestBlogArticleOptions
/// {
///     Keyword = "social media scheduling",
/// });
/// // article.Status is "queued"; poll GetArticleAsync until it is "draft", then SubmitArticleAsync.
/// </code>
/// </example>
public sealed class BlogResource
{
    private readonly FoPostHttpClient _http;

    internal BlogResource(FoPostHttpClient http) => _http = http;

    private static string KeywordPath(string keywordId) => $"/v1/blog/keywords/{keywordId}";

    private static string ArticlePath(string articleId) => $"/v1/blog/articles/{articleId}";

    private static Dictionary<string, object?> WorkspaceQuery(string workspaceId) =>
        new() { ["workspaceId"] = workspaceId };

    private static string Date(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    // ─── Settings ────────────────────────────────────────────────────

    public async Task<BlogSettings> GetSettingsAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/blog/settings", WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogSettings>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Partial update. The answer carries the website signing secret only when this call
    /// set <c>websiteUrl</c> for the first time; store it, it is never shown again.
    /// </summary>
    public async Task<BlogSettingsUpdate> UpdateSettingsAsync(
        string workspaceId,
        UpdateBlogSettingsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["workspaceId"] = workspaceId };
        Set(body, "siteUrl", options.SiteUrl);
        Set(body, "topic", options.Topic);
        Set(body, "audience", options.Audience);
        Set(body, "goal", options.Goal);
        Set(body, "focusTopics", options.FocusTopics);
        Set(body, "avoidTopics", options.AvoidTopics);
        Set(body, "language", options.Language);
        Set(body, "locationCode", options.LocationCode);
        Set(body, "cadencePerWeek", options.CadencePerWeek);
        Set(body, "reviewHoldDays", options.ReviewHoldDays);
        Set(body, "imageTier", options.ImageTier);
        Set(body, "inBodyImages", options.InBodyImages);
        Set(body, "minWords", options.MinWords);
        Set(body, "maxWords", options.MaxWords);
        Set(body, "brandVoiceId", options.BrandVoiceId);
        Set(body, "targetKind", options.TargetKind);
        Set(body, "targetAccountId", options.TargetAccountId);
        Set(body, "websiteUrl", options.WebsiteUrl);

        var response = await _http
            .RequestAsync(HttpMethod.Patch, "/v1/blog/settings", body, null, cancellationToken)
            .ConfigureAwait(false);
        var settings = Require<BlogSettings>(FoPostHttpClient.Unwrap(response));
        var secret = response is JsonObject obj &&
                     obj.TryGetPropertyValue("websiteSecret", out var node) &&
                     node is JsonValue value
            ? value.GetValue<string>()
            : null;
        return new BlogSettingsUpdate(settings, secret);
    }

    /// <summary>Issues a new website signing secret. The old one stops verifying at once.</summary>
    public async Task<BlogWebsiteSecret> RotateWebsiteSecretAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync("/v1/blog/settings/website-secret", WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogWebsiteSecret>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Sends a signed <c>article.test</c> delivery to the website target. A site that did
    /// not answer 2xx comes back as <see cref="BlogWebsiteTest.Ok"/> false rather than an exception.
    /// </summary>
    public async Task<BlogWebsiteTest> TestWebsiteAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _http
                .PostAsync("/v1/blog/settings/website-test", WorkspaceQuery(workspaceId), cancellationToken)
                .ConfigureAwait(false);
            return Require<BlogWebsiteTest>(FoPostHttpClient.Unwrap(response));
        }
        catch (FoPostException error) when (error.Status == 502 && FoPostHttpClient.Unwrap(error.Body) is JsonObject result)
        {
            // The API reports a failed test as 502 with the same body shape.
            return Require<BlogWebsiteTest>(result);
        }
    }

    // ─── Keywords ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<BlogKeyword>> ListKeywordsAsync(
        string workspaceId,
        ListBlogKeywordsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = WorkspaceQuery(workspaceId);
        query["status"] = Join(options?.Status);
        query["limit"] = options?.Limit;
        var body = await _http.GetAsync("/v1/blog/keywords", query, cancellationToken).ConfigureAwait(false);
        return ToList<BlogKeyword>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Suggests about 10 search terms from the blog goal and topics. Pass the current seeds
    /// to refine them and a prompt to steer. Spends credits; 402 when the balance is empty.
    /// </summary>
    public async Task<BlogSeedSuggestions> SuggestSeedsAsync(
        string workspaceId,
        SuggestBlogSeedsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var body = WorkspaceQuery(workspaceId);
        if (options?.Seeds is not null)
        {
            body["seeds"] = options.Seeds;
        }

        AddIfPresent(body, "prompt", options?.Prompt);

        var response = await _http.PostAsync("/v1/blog/keywords/suggest", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogSeedSuggestions>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Queues keyword research. Spends credits; answers once the job is queued. Results land
    /// as <c>suggested</c> keywords and do not fill the calendar; follow the run on
    /// <see cref="BlogSettings.PlanRun"/> and set a keyword to <c>planned</c> to pick it.
    /// </summary>
    public async Task<BlogPlanResult> PlanKeywordsAsync(
        string workspaceId,
        PlanBlogKeywordsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var body = WorkspaceQuery(workspaceId);
        if (options?.Seeds is not null)
        {
            body["seeds"] = options.Seeds;
        }

        var response = await _http.PostAsync("/v1/blog/keywords/plan", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogPlanResult>(FoPostHttpClient.Unwrap(response));
    }

    public async Task<BlogKeyword> AddKeywordAsync(
        string workspaceId,
        CreateBlogKeywordOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = WorkspaceQuery(workspaceId);
        body["keyword"] = options.Keyword;
        if (options.PlannedFor is { } day)
        {
            body["plannedFor"] = Date(day);
        }

        var response = await _http.PostAsync("/v1/blog/keywords", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogKeyword>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Partial update: only the fields you set are sent. Setting the status to <c>planned</c>
    /// picks the keyword, and it takes the next free date.
    /// </summary>
    public async Task<BlogKeyword> UpdateKeywordAsync(
        string workspaceId,
        string keywordId,
        UpdateBlogKeywordOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        Set(body, "status", options.Status);
        if (options.PlannedFor.IsSet)
        {
            body["plannedFor"] = options.PlannedFor.Value is { } day ? Date(day) : null;
        }

        var response = await _http
            .RequestAsync(HttpMethod.Patch, KeywordPath(keywordId), body, WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogKeyword>(FoPostHttpClient.Unwrap(response));
    }

    public async Task DeleteKeywordAsync(
        string workspaceId,
        string keywordId,
        CancellationToken cancellationToken = default)
    {
        await _http
            .RequestAsync(HttpMethod.Delete, KeywordPath(keywordId), null, WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
    }

    // ─── Articles ────────────────────────────────────────────────────

    public async Task<IReadOnlyList<BlogArticle>> ListArticlesAsync(
        string workspaceId,
        ListBlogArticlesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = WorkspaceQuery(workspaceId);
        query["status"] = Join(options?.Status);
        query["limit"] = options?.Limit;
        var body = await _http.GetAsync("/v1/blog/articles", query, cancellationToken).ConfigureAwait(false);
        return ToList<BlogArticle>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Queues an article. Spends credits; the answer is <c>queued</c>, so poll
    /// <see cref="GetArticleAsync"/> until it reaches <c>draft</c>.
    /// </summary>
    public async Task<BlogArticle> RequestArticleAsync(
        string workspaceId,
        RequestBlogArticleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = WorkspaceQuery(workspaceId);
        AddIfPresent(body, "title", options.Title);
        AddIfPresent(body, "keyword", options.Keyword);
        AddIfPresent(body, "keywordId", options.KeywordId);
        AddIfPresent(body, "instructions", options.Instructions);

        var response = await _http.PostAsync("/v1/blog/articles", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogArticle>(FoPostHttpClient.Unwrap(response));
    }

    public async Task<BlogArticleFull> GetArticleAsync(
        string workspaceId,
        string articleId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(ArticlePath(articleId), WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogArticleFull>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Partial update. Answers 409 <c>invalid_state</c> once the article is no longer editable.
    /// Setting <c>publishAt</c> needs the article <c>in_review</c> (or already scheduled or failed).
    /// </summary>
    public async Task<BlogArticleFull> UpdateArticleAsync(
        string workspaceId,
        string articleId,
        UpdateBlogArticleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        Set(body, "title", options.Title);
        Set(body, "slug", options.Slug);
        Set(body, "excerpt", options.Excerpt);
        Set(body, "metaTitle", options.MetaTitle);
        Set(body, "metaDescription", options.MetaDescription);
        Set(body, "bodyMarkdown", options.BodyMarkdown);
        Set(body, "tags", options.Tags);
        if (options.PublishAt.IsSet)
        {
            body["publishAt"] = options.PublishAt.Value is { } at ? Iso(at) : null;
        }

        var response = await _http
            .RequestAsync(HttpMethod.Patch, ArticlePath(articleId), body, WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogArticleFull>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Writes the article again from its brief. Spends credits.</summary>
    public Task<BlogArticle> RegenerateArticleAsync(
        string workspaceId,
        string articleId,
        CancellationToken cancellationToken = default) =>
        ArticleAction(workspaceId, articleId, "regenerate", cancellationToken);

    /// <summary>Moves a draft, failed or rejected article to <c>in_review</c>; 409 from any other state.</summary>
    public Task<BlogArticle> SubmitArticleAsync(
        string workspaceId,
        string articleId,
        CancellationToken cancellationToken = default) =>
        ArticleAction(workspaceId, articleId, "submit", cancellationToken);

    /// <summary>
    /// Publishes to the configured target. Needs the <c>publish</c> scope and the article
    /// <c>in_review</c> (or already scheduled or failed); a draft answers 409 until it is
    /// submitted, and 409 <c>no_target</c> when the settings name no target.
    /// </summary>
    public Task<BlogArticle> PublishArticleAsync(
        string workspaceId,
        string articleId,
        CancellationToken cancellationToken = default) =>
        ArticleAction(workspaceId, articleId, "publish", cancellationToken);

    /// <summary>Takes the draft out of review for good.</summary>
    public Task<BlogArticle> RejectArticleAsync(
        string workspaceId,
        string articleId,
        CancellationToken cancellationToken = default) =>
        ArticleAction(workspaceId, articleId, "reject", cancellationToken);

    public async Task DeleteArticleAsync(
        string workspaceId,
        string articleId,
        CancellationToken cancellationToken = default)
    {
        await _http
            .RequestAsync(HttpMethod.Delete, ArticlePath(articleId), null, WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<BlogArticle> ArticleAction(
        string workspaceId,
        string articleId,
        string action,
        CancellationToken cancellationToken)
    {
        var response = await _http
            .RequestAsync(HttpMethod.Post, $"{ArticlePath(articleId)}/{action}", null, WorkspaceQuery(workspaceId), cancellationToken)
            .ConfigureAwait(false);
        return Require<BlogArticle>(FoPostHttpClient.Unwrap(response));
    }

    private static string? Join(IReadOnlyList<string>? values) =>
        values is null || values.Count == 0 ? null : string.Join(",", values);

    private static void Set<T>(IDictionary<string, object?> target, string key, Optional<T> value)
    {
        if (value.IsSet)
        {
            target[key] = value.Value;
        }
    }

    private static void AddIfPresent(IDictionary<string, object?> target, string key, string? value)
    {
        if (value is not null)
        {
            target[key] = value;
        }
    }
}
