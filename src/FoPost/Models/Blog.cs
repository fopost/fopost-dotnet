using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>Where an article is in its life, from request to live on the site.</summary>
public static class BlogArticleStatuses
{
    public const string Queued = "queued";
    public const string Writing = "writing";
    public const string Draft = "draft";

    /// <summary>Submitted for approval; publishing and scheduling are open from here.</summary>
    public const string InReview = "in_review";

    public const string Scheduled = "scheduled";
    public const string Publishing = "publishing";
    public const string Published = "published";
    public const string Failed = "failed";
    public const string Rejected = "rejected";
}

/// <summary>Where a keyword is in the plan.</summary>
public static class BlogKeywordStatuses
{
    public const string Suggested = "suggested";
    public const string Planned = "planned";
    public const string Written = "written";
    public const string Dismissed = "dismissed";
}

/// <summary>What the blog is for; steers suggested terms and keyword ranking.</summary>
public static class BlogGoals
{
    public const string Traffic = "traffic";
    public const string Leads = "leads";
    public const string Sales = "sales";
    public const string Signups = "signups";
    public const string Authority = "authority";
}

/// <summary>Where a keyword research run is.</summary>
public static class BlogPlanRunStatuses
{
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Done = "done";
    public const string Failed = "failed";
}

/// <summary>Where published articles go.</summary>
public static class BlogTargetKinds
{
    /// <summary>The product blog, admins only.</summary>
    public const string ProductBlog = "product_blog";

    /// <summary>A connected account that can carry long-form articles.</summary>
    public const string Account = "account";

    /// <summary>Your own site, which receives each article as a signed webhook.</summary>
    public const string Website = "website";
}

/// <summary>Where articles publish, what the blog is for and how often it is planned.</summary>
public sealed class BlogSettings : FoPostModel
{
    [JsonPropertyName("siteUrl")]
    public string? SiteUrl { get; set; }

    [JsonPropertyName("topic")]
    public string? Topic { get; set; }

    [JsonPropertyName("audience")]
    public string? Audience { get; set; }

    /// <summary>One of <see cref="BlogGoals"/>, or null when unset.</summary>
    [JsonPropertyName("goal")]
    public string? Goal { get; set; }

    /// <summary>What the blog wants to write about.</summary>
    [JsonPropertyName("focusTopics")]
    public string? FocusTopics { get; set; }

    /// <summary>What the blog never writes about.</summary>
    [JsonPropertyName("avoidTopics")]
    public string? AvoidTopics { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("locationCode")]
    public int? LocationCode { get; set; }

    /// <summary>Articles planned per week; 0 turns planning off.</summary>
    [JsonPropertyName("cadencePerWeek")]
    public int CadencePerWeek { get; set; }

    /// <summary>Days before its planned date that a picked keyword starts being written.</summary>
    [JsonPropertyName("reviewHoldDays")]
    public int ReviewHoldDays { get; set; }

    /// <summary><c>draft</c>, <c>standard</c> or <c>best</c>.</summary>
    [JsonPropertyName("imageTier")]
    public string? ImageTier { get; set; }

    [JsonPropertyName("inBodyImages")]
    public int? InBodyImages { get; set; }

    [JsonPropertyName("minWords")]
    public int? MinWords { get; set; }

    [JsonPropertyName("maxWords")]
    public int? MaxWords { get; set; }

    [JsonPropertyName("brandVoiceId")]
    public string? BrandVoiceId { get; set; }

    /// <summary>One of <see cref="BlogTargetKinds"/>, or null when nothing is set.</summary>
    [JsonPropertyName("targetKind")]
    public string? TargetKind { get; set; }

    [JsonPropertyName("targetAccountId")]
    public string? TargetAccountId { get; set; }

    [JsonPropertyName("targetPlatform")]
    public string? TargetPlatform { get; set; }

    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [JsonPropertyName("hasWebsiteSecret")]
    public bool HasWebsiteSecret { get; set; }

    /// <summary>The last keyword research run; null before the first.</summary>
    [JsonPropertyName("planRun")]
    public BlogPlanRun? PlanRun { get; set; }

    /// <summary>Whether search volume and difficulty can be fetched on this deployment.</summary>
    [JsonPropertyName("keywordDataAvailable")]
    public bool KeywordDataAvailable { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>The last keyword research run.</summary>
public sealed class BlogPlanRun : FoPostModel
{
    /// <summary>One of <see cref="BlogPlanRunStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>New keywords the run found.</summary>
    [JsonPropertyName("added")]
    public int? Added { get; set; }

    [JsonPropertyName("finishedAt")]
    public DateTimeOffset? FinishedAt { get; set; }
}

/// <summary>
/// What an update answers: the settings, plus the website signing secret when this
/// call set <c>websiteUrl</c> for the first time. The secret is shown once.
/// </summary>
public sealed class BlogSettingsUpdate
{
    public BlogSettingsUpdate(BlogSettings settings, string? websiteSecret)
    {
        Settings = settings;
        WebsiteSecret = websiteSecret;
    }

    public BlogSettings Settings { get; }

    public string? WebsiteSecret { get; }
}

/// <summary>A freshly issued website signing secret. Never readable again.</summary>
public sealed class BlogWebsiteSecret : FoPostModel
{
    [JsonPropertyName("websiteSecret")]
    public string WebsiteSecret { get; set; } = string.Empty;
}

/// <summary>What the website target answered to a signed test delivery.</summary>
public sealed class BlogWebsiteTest : FoPostModel
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    /// <summary>The HTTP status your site answered, when it answered at all.</summary>
    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>What a plan request answers: the job is queued.</summary>
public sealed class BlogPlanResult : FoPostModel
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>Search terms suggested from the blog goal and topics.</summary>
public sealed class BlogSeedSuggestions : FoPostModel
{
    [JsonPropertyName("seeds")]
    public List<string> Seeds { get; set; } = new();
}

/// <summary>One keyword in the plan.</summary>
public sealed class BlogKeyword : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("keyword")]
    public string Keyword { get; set; } = string.Empty;

    [JsonPropertyName("searchVolume")]
    public int? SearchVolume { get; set; }

    /// <summary>0 to 100.</summary>
    [JsonPropertyName("difficulty")]
    public int? Difficulty { get; set; }

    [JsonPropertyName("cpc")]
    public double? Cpc { get; set; }

    [JsonPropertyName("intent")]
    public string? Intent { get; set; }

    /// <summary><c>provider</c>, <c>user</c> or <c>agent</c>.</summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>One of <see cref="BlogKeywordStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>The day the article for it is planned, <c>yyyy-MM-dd</c>.</summary>
    [JsonPropertyName("plannedFor")]
    public string? PlannedFor { get; set; }

    /// <summary>Why the keyword serves the blog goal.</summary>
    [JsonPropertyName("fitReason")]
    public string? FitReason { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }
}

/// <summary>An image on an article. URLs stay fetchable for six hours.</summary>
public sealed class BlogArticleImage : FoPostModel
{
    [JsonPropertyName("mediaId")]
    public string MediaId { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("alt")]
    public string Alt { get; set; } = string.Empty;

    /// <summary>Index of the outline heading the image sits under; absent on the featured image.</summary>
    [JsonPropertyName("position")]
    public int? Position { get; set; }
}

/// <summary>An article as a list answers it: everything but the body.</summary>
public class BlogArticle : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    [JsonPropertyName("excerpt")]
    public string? Excerpt { get; set; }

    [JsonPropertyName("metaTitle")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("metaDescription")]
    public string? MetaDescription { get; set; }

    [JsonPropertyName("featuredImage")]
    public BlogArticleImage? FeaturedImage { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; }

    /// <summary>One of <see cref="BlogArticleStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary><c>plan</c>, <c>user</c> or <c>agent</c>.</summary>
    [JsonPropertyName("origin")]
    public string Origin { get; set; } = string.Empty;

    /// <summary>When the review hold set at writing ends.</summary>
    [JsonPropertyName("reviewUntil")]
    public DateTimeOffset? ReviewUntil { get; set; }

    [JsonPropertyName("publishAt")]
    public DateTimeOffset? PublishAt { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonPropertyName("publishedUrl")]
    public string? PublishedUrl { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>An article with its body, outline, brief and images.</summary>
public sealed class BlogArticleFull : BlogArticle
{
    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("outline")]
    public List<string> Outline { get; set; } = new();

    /// <summary>The research the writer worked from.</summary>
    [JsonPropertyName("brief")]
    public JsonElement? Brief { get; set; }

    [JsonPropertyName("bodyMarkdown")]
    public string BodyMarkdown { get; set; } = string.Empty;

    [JsonPropertyName("bodyHtml")]
    public string BodyHtml { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<BlogArticleImage> Images { get; set; } = new();
}

/// <summary>The <c>data</c> of an <c>article.published</c> webhook.</summary>
public sealed class BlogArticlePublishedPayload : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("excerpt")]
    public string? Excerpt { get; set; }

    [JsonPropertyName("metaTitle")]
    public string? MetaTitle { get; set; }

    [JsonPropertyName("metaDescription")]
    public string? MetaDescription { get; set; }

    [JsonPropertyName("bodyMarkdown")]
    public string BodyMarkdown { get; set; } = string.Empty;

    [JsonPropertyName("bodyHtml")]
    public string BodyHtml { get; set; } = string.Empty;

    [JsonPropertyName("featuredImage")]
    public BlogArticleImage? FeaturedImage { get; set; }

    /// <summary>Fetchable for six hours; download and keep your own copy.</summary>
    [JsonPropertyName("images")]
    public List<BlogArticleImage> Images { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("keyword")]
    public string? Keyword { get; set; }

    [JsonPropertyName("wordCount")]
    public int WordCount { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonPropertyName("publishedUrl")]
    public string? PublishedUrl { get; set; }
}
