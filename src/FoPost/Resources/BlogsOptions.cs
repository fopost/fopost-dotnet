namespace FoPost;

/// <summary>Filters for listing a blog's articles. Unset fields are not sent.</summary>
public sealed class ListArticlesOptions
{
    /// <summary>How many to return, 1 to 50. The API defaults to 20.</summary>
    public int? Limit { get; set; }

    /// <summary><c>published</c>, <c>draft</c>, <c>pending</c> or <c>scheduled</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Matches the article title.</summary>
    public string? Q { get; set; }
}

/// <summary>Filters for listing a store's products. Unset fields are not sent.</summary>
public sealed class ListProductsOptions
{
    /// <summary>How many to return, 1 to 50. The API defaults to 20.</summary>
    public int? Limit { get; set; }

    /// <summary><c>active</c>, <c>draft</c> or <c>archived</c>.</summary>
    public string? Status { get; set; }

    /// <summary>Matches the product title.</summary>
    public string? Q { get; set; }
}

/// <summary>
/// The body of <c>CreateArticleAsync</c> and <c>UpdateArticleAsync</c>.
/// </summary>
/// <remarks>
/// Only the fields that are set travel. On a create, set at least <c>Title</c> and
/// <c>Body</c>; on an update, set at least one, and whatever is left null keeps
/// whatever the site already had.
/// </remarks>
public sealed class ArticleOptions
{
    public string? Title { get; set; }

    /// <summary>FoPost body markup; the site's own format is rendered from it.</summary>
    public string? Body { get; set; }

    public string? Excerpt { get; set; }

    /// <summary><c>published</c>, <c>draft</c>, <c>pending</c> or <c>scheduled</c>.</summary>
    public string? Status { get; set; }

    public IList<string>? Tags { get; set; }

    public string? AuthorName { get; set; }

    /// <summary>Public http(s) URL of the featured image.</summary>
    public string? ImageUrl { get; set; }
}

/// <summary>
/// The body of <c>UpdateProductAsync</c>. Only the fields that are set travel, so an
/// omitted one keeps whatever the store already had. Set at least one.
/// </summary>
public sealed class ProductOptions
{
    public string? Title { get; set; }

    /// <summary>Body markup, rendered to HTML on the store.</summary>
    public string? Description { get; set; }

    /// <summary><c>active</c>, <c>draft</c> or <c>archived</c>.</summary>
    public string? Status { get; set; }

    public IList<string>? Tags { get; set; }

    public string? ProductType { get; set; }

    public string? Vendor { get; set; }
}
