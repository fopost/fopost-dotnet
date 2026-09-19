using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>
/// A blog on a connected site. <c>Id</c> is the platform's own id, never a FoPost id.
/// </summary>
/// <remarks>
/// A Shopify store reports every blog it has; WordPress has one implicit blog and reports it
/// under the id <c>default</c>, so both answer the same shape.
/// </remarks>
public sealed class RemoteBlog : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>An article that already lives on a connected site.</summary>
public sealed class RemoteArticle : FoPostModel
{
    /// <summary>The article id on the platform.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("blog_id")]
    public string? BlogId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("body_html")]
    public string? BodyHtml { get; set; }

    [JsonPropertyName("excerpt")]
    public string? Excerpt { get; set; }

    /// <summary><c>published</c>, <c>draft</c>, <c>pending</c> or <c>scheduled</c>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("author_name")]
    public string? AuthorName { get; set; }

    [JsonPropertyName("tags")]
    public IList<string> Tags { get; set; } = new List<string>();

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("published_at")]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>A product on a connected store.</summary>
public sealed class RemoteProduct : FoPostModel
{
    /// <summary>The product id on the platform.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    /// <summary><c>active</c>, <c>draft</c> or <c>archived</c>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("vendor")]
    public string? Vendor { get; set; }

    [JsonPropertyName("product_type")]
    public string? ProductType { get; set; }

    [JsonPropertyName("tags")]
    public IList<string> Tags { get; set; } = new List<string>();

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Lowest variant price, as a decimal string.</summary>
    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
