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

/// <summary>The body of <c>CreateArticleAsync</c>. Title and Body are required.</summary>
public sealed class CreateArticleOptions
{
    public string Title { get; set; } = string.Empty;

    /// <summary>FoPost body markup; the site's own format is rendered from it.</summary>
    public string Body { get; set; } = string.Empty;

    public string? Excerpt { get; set; }

    /// <summary><c>published</c>, <c>draft</c>, <c>pending</c> or <c>scheduled</c>.</summary>
    public string? Status { get; set; }

    public IList<string>? Tags { get; set; }

    public string? AuthorName { get; set; }

    /// <summary>Public http(s) URL of the featured image.</summary>
    public string? ImageUrl { get; set; }
}

/// <summary>
/// The body of <c>UpdateArticleAsync</c>. Only the fields that were set travel, so an omitted
/// one keeps whatever the site already had. Set at least one.
/// </summary>
public sealed class UpdateArticleOptions
{
    public Optional<string> Title { get; set; }

    public Optional<string> Body { get; set; }

    public Optional<string> Excerpt { get; set; }

    public Optional<string> Status { get; set; }

    public Optional<IList<string>> Tags { get; set; }

    public Optional<string> AuthorName { get; set; }

    public Optional<string> ImageUrl { get; set; }
}

/// <summary>
/// The body of <c>UpdateProductAsync</c>. Only the fields that were set travel. Set at least one.
/// </summary>
public sealed class UpdateProductOptions
{
    public Optional<string> Title { get; set; }

    public Optional<string> Description { get; set; }

    /// <summary><c>active</c>, <c>draft</c> or <c>archived</c>.</summary>
    public Optional<string> Status { get; set; }

    public Optional<IList<string>> Tags { get; set; }

    public Optional<string> ProductType { get; set; }

    public Optional<string> Vendor { get; set; }
}
