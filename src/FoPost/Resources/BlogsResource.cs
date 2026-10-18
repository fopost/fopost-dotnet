using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Blogs</c> — articles and products that already live on a connected
/// site, addressed by the platform's own ids rather than FoPost ids.
/// </summary>
/// <remarks>
/// Reads need the <c>posts</c> scope; anything that changes the site needs
/// <c>posts</c> and <c>publish</c>. An account on a platform that cannot manage
/// articles answers 400 <c>unsupported_platform</c>. An update changes the live
/// article in place and never creates a second post.
/// </remarks>
public sealed class BlogsResource
{
    private readonly FoPostHttpClient _http;

    internal BlogsResource(FoPostHttpClient http) => _http = http;

    private static string BlogsPath(string accountId) => $"/v1/accounts/{accountId}/blogs";

    private static string ArticlesPath(string accountId, string blogId) =>
        $"{BlogsPath(accountId)}/{blogId}/articles";

    private static string ArticlePath(string accountId, string blogId, string articleId) =>
        $"{ArticlesPath(accountId, blogId)}/{articleId}";

    private static Dictionary<string, object?> ArticleBody(ArticleOptions options)
    {
        var body = new Dictionary<string, object?>();
        if (options.Title is not null) body["title"] = options.Title;
        if (options.Body is not null) body["body"] = options.Body;
        if (options.Excerpt is not null) body["excerpt"] = options.Excerpt;
        if (options.Status is not null) body["status"] = options.Status;
        if (options.Tags is not null) body["tags"] = options.Tags.ToList();
        if (options.AuthorName is not null) body["author_name"] = options.AuthorName;
        if (options.ImageUrl is not null) body["image_url"] = options.ImageUrl;
        return body;
    }

    /// <summary>The blogs the account can write to. WordPress reports one, under <c>default</c>.</summary>
    public async Task<IReadOnlyList<RemoteBlog>> ListBlogsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(BlogsPath(accountId), null, cancellationToken).ConfigureAwait(false);
        return ToList<RemoteBlog>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Articles on the blog, newest first, drafts included.</summary>
    public async Task<IReadOnlyList<RemoteArticle>> ListArticlesAsync(
        string accountId,
        string blogId,
        ListArticlesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["limit"] = options?.Limit,
            ["status"] = options?.Status,
            ["q"] = options?.Q,
        };
        var body = await _http
            .GetAsync(ArticlesPath(accountId, blogId), query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<RemoteArticle>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<RemoteArticle> GetArticleAsync(
        string accountId,
        string blogId,
        string articleId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(ArticlePath(accountId, blogId, articleId), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<RemoteArticle>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Writes a new article. Needs the <c>publish</c> scope.</summary>
    public async Task<RemoteArticle> CreateArticleAsync(
        string accountId,
        string blogId,
        ArticleOptions options,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync(ArticlesPath(accountId, blogId), ArticleBody(options), cancellationToken)
            .ConfigureAwait(false);
        return Require<RemoteArticle>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Changes the live article in place; never creates a duplicate.</summary>
    public async Task<RemoteArticle> UpdateArticleAsync(
        string accountId,
        string blogId,
        string articleId,
        ArticleOptions options,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .RequestAsync(
                HttpMethod.Patch,
                ArticlePath(accountId, blogId, articleId),
                ArticleBody(options),
                null,
                cancellationToken)
            .ConfigureAwait(false);
        return Require<RemoteArticle>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Removes the article from the site. This cannot be undone.</summary>
    public Task DeleteArticleAsync(
        string accountId,
        string blogId,
        string articleId,
        CancellationToken cancellationToken = default) =>
        _http.DeleteAsync(ArticlePath(accountId, blogId, articleId), null, cancellationToken);

    public async Task<IReadOnlyList<RemoteProduct>> ListProductsAsync(
        string accountId,
        ListProductsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["limit"] = options?.Limit,
            ["status"] = options?.Status,
            ["q"] = options?.Q,
        };
        var body = await _http
            .GetAsync($"/v1/accounts/{accountId}/products", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<RemoteProduct>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Changes the product on the store. Only what is set travels.</summary>
    public async Task<RemoteProduct> UpdateProductAsync(
        string accountId,
        string productId,
        ProductOptions options,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();
        if (options.Title is not null) body["title"] = options.Title;
        if (options.Description is not null) body["description"] = options.Description;
        if (options.Status is not null) body["status"] = options.Status;
        if (options.Tags is not null) body["tags"] = options.Tags.ToList();
        if (options.ProductType is not null) body["product_type"] = options.ProductType;
        if (options.Vendor is not null) body["vendor"] = options.Vendor;

        var response = await _http
            .RequestAsync(
                HttpMethod.Patch,
                $"/v1/accounts/{accountId}/products/{productId}",
                body,
                null,
                cancellationToken)
            .ConfigureAwait(false);
        return Require<RemoteProduct>(FoPostHttpClient.Unwrap(response));
    }
}
