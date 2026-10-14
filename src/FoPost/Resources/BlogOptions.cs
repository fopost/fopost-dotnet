namespace FoPost;

/// <summary>
/// The body of <see cref="Resources.BlogResource.UpdateSettingsAsync"/>. Only the fields
/// you set are sent. Setting <see cref="WebsiteUrl"/> for the first time issues a signing
/// secret, returned once on <see cref="BlogSettingsUpdate.WebsiteSecret"/>.
/// </summary>
public sealed class UpdateBlogSettingsOptions
{
    public Optional<string?> SiteUrl { get; set; }

    public Optional<string?> Topic { get; set; }

    public Optional<string?> Audience { get; set; }

    public Optional<string> Language { get; set; }

    public Optional<int> LocationCode { get; set; }

    /// <summary>0 to 7; 0 turns planning off.</summary>
    public Optional<int> CadencePerWeek { get; set; }

    /// <summary>0 to 14.</summary>
    public Optional<int> ReviewHoldDays { get; set; }

    public Optional<bool> AutoPublish { get; set; }

    /// <summary><c>draft</c>, <c>standard</c> or <c>best</c>.</summary>
    public Optional<string> ImageTier { get; set; }

    /// <summary>0 to 3.</summary>
    public Optional<int> InBodyImages { get; set; }

    public Optional<int> MinWords { get; set; }

    public Optional<int> MaxWords { get; set; }

    public Optional<string?> BrandVoiceId { get; set; }

    /// <summary>One of <see cref="BlogTargetKinds"/>.</summary>
    public Optional<string?> TargetKind { get; set; }

    public Optional<string?> TargetAccountId { get; set; }

    /// <summary>Where published articles are POSTed as a signed webhook.</summary>
    public Optional<string?> WebsiteUrl { get; set; }
}

/// <summary>Filters for <see cref="Resources.BlogResource.ListKeywordsAsync"/>.</summary>
public sealed class ListBlogKeywordsOptions
{
    /// <summary>Any of <see cref="BlogKeywordStatuses"/>; several are sent comma-separated.</summary>
    public IReadOnlyList<string>? Status { get; set; }

    /// <summary>Caps at 500.</summary>
    public int? Limit { get; set; }
}

/// <summary>The body of <see cref="Resources.BlogResource.PlanKeywordsAsync"/>.</summary>
public sealed class PlanBlogKeywordsOptions
{
    /// <summary>Terms to grow the plan from; omitted, the topic and site are used.</summary>
    public IReadOnlyList<string>? Seeds { get; set; }
}

/// <summary>The body of <see cref="Resources.BlogResource.AddKeywordAsync"/>.</summary>
public sealed class CreateBlogKeywordOptions
{
    public required string Keyword { get; set; }

    /// <summary>The day to write it; set, the keyword starts <c>planned</c>.</summary>
    public DateOnly? PlannedFor { get; set; }
}

/// <summary>The body of <see cref="Resources.BlogResource.UpdateKeywordAsync"/>. Only set fields are sent.</summary>
public sealed class UpdateBlogKeywordOptions
{
    /// <summary>One of <see cref="BlogKeywordStatuses"/>.</summary>
    public Optional<string> Status { get; set; }

    public Optional<DateOnly?> PlannedFor { get; set; }
}

/// <summary>Filters for <see cref="Resources.BlogResource.ListArticlesAsync"/>.</summary>
public sealed class ListBlogArticlesOptions
{
    /// <summary>Any of <see cref="BlogArticleStatuses"/>; several are sent comma-separated.</summary>
    public IReadOnlyList<string>? Status { get; set; }

    /// <summary>Caps at 200.</summary>
    public int? Limit { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.BlogResource.RequestArticleAsync"/>. One of
/// <see cref="Title"/>, <see cref="Keyword"/> or <see cref="KeywordId"/> is required.
/// </summary>
public sealed class RequestBlogArticleOptions
{
    public string? Title { get; set; }

    public string? Keyword { get; set; }

    /// <summary>A keyword from the plan; its term becomes the target.</summary>
    public string? KeywordId { get; set; }

    /// <summary>Up to 4000 characters of direction for the writer.</summary>
    public string? Instructions { get; set; }
}

/// <summary>The body of <see cref="Resources.BlogResource.UpdateArticleAsync"/>. Only set fields are sent.</summary>
public sealed class UpdateBlogArticleOptions
{
    public Optional<string> Title { get; set; }

    /// <summary>Lowercase letters, digits and hyphens.</summary>
    public Optional<string> Slug { get; set; }

    public Optional<string?> Excerpt { get; set; }

    public Optional<string?> MetaTitle { get; set; }

    public Optional<string?> MetaDescription { get; set; }

    public Optional<string> BodyMarkdown { get; set; }

    public Optional<IReadOnlyList<string>> Tags { get; set; }

    /// <summary>Puts the draft on the calendar; null returns it to review.</summary>
    public Optional<DateTimeOffset?> PublishAt { get; set; }
}
