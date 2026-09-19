using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>The body of <see cref="Resources.AdsResource.AuthorizeMetaAsync"/>.</summary>
public sealed class AuthorizeMetaAdsOptions
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary><c>business</c> (default) for a business login, <c>user</c> for a personal one.</summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>Dashboard path to land on after the ad platform redirects back.</summary>
    [JsonPropertyName("returnTo")]
    public string? ReturnTo { get; set; }
}

/// <summary>What a boost and an ad have in common. These are serialised as sent.</summary>
public abstract class AdRequestOptions
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>An ads connection in the workspace.</summary>
    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="AdGoals"/>.</summary>
    [JsonPropertyName("goal")]
    public string Goal { get; set; } = AdGoals.Engagement;

    [JsonPropertyName("budget")]
    public AdBudget Budget { get; set; } = new();

    [JsonPropertyName("targeting")]
    public AdTargeting Targeting { get; set; } = new();

    /// <summary>
    /// Left unset, the ad is created paused and spends nothing until it is
    /// resumed. Set to <c>false</c> to go live at once.
    /// </summary>
    [JsonPropertyName("paused")]
    public bool? Paused { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.BoostAsync"/>.</summary>
public sealed class BoostPostOptions : AdRequestOptions
{
    /// <summary>A published FoPost post.</summary>
    [JsonPropertyName("postId")]
    public string PostId { get; set; } = string.Empty;

    /// <summary>The account the post was delivered to.</summary>
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;
}

/// <summary>The body of <see cref="Resources.AdsResource.CreateAsync"/>.</summary>
public sealed class CreateAdOptions : AdRequestOptions
{
    /// <summary>The Page the ad runs from.</summary>
    [JsonPropertyName("pageId")]
    public string PageId { get; set; } = string.Empty;

    /// <summary>Up to 125 characters.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Up to 40 characters.</summary>
    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("destinationUrl")]
    public string? DestinationUrl { get; set; }

    /// <summary>A media library asset url.</summary>
    [JsonPropertyName("mediaUrl")]
    public string? MediaUrl { get; set; }
}

/// <summary>
/// How an audience is built. Use <see cref="Custom"/>, <see cref="Lookalike"/>,
/// or <see cref="Website"/> rather than filling the fields by hand.
/// </summary>
public sealed class AudienceSpec
{
    /// <summary>One of <see cref="AudienceSubtypes"/>.</summary>
    [JsonPropertyName("subtype")]
    public string Subtype { get; set; } = AudienceSubtypes.Custom;

    /// <summary>Customer list; hashed before it leaves the API.</summary>
    [JsonPropertyName("emails")]
    public IList<string>? Emails { get; set; }

    [JsonPropertyName("originAudienceId")]
    public string? OriginAudienceId { get; set; }

    /// <summary>ISO 3166-1 alpha-2.</summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>0.01 to 0.2; the share of the country's users to reach.</summary>
    [JsonPropertyName("ratio")]
    public double? Ratio { get; set; }

    [JsonPropertyName("pixelId")]
    public string? PixelId { get; set; }

    /// <summary>1 to 180.</summary>
    [JsonPropertyName("retentionDays")]
    public int? RetentionDays { get; set; }

    [JsonPropertyName("urlContains")]
    public string? UrlContains { get; set; }

    public static AudienceSpec Custom(IList<string>? emails = null) =>
        new() { Subtype = AudienceSubtypes.Custom, Emails = emails ?? new List<string>() };

    public static AudienceSpec Lookalike(string originAudienceId, string country, double? ratio = null) =>
        new()
        {
            Subtype = AudienceSubtypes.Lookalike,
            OriginAudienceId = originAudienceId,
            Country = country,
            Ratio = ratio,
        };

    public static AudienceSpec Website(string pixelId, int? retentionDays = null, string? urlContains = null) =>
        new()
        {
            Subtype = AudienceSubtypes.Website,
            PixelId = pixelId,
            RetentionDays = retentionDays,
            UrlContains = urlContains,
        };
}

/// <summary>The body of <see cref="Resources.AdsResource.CreateAudienceAsync"/>.</summary>
public sealed class CreateAudienceOptions
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("spec")]
    public AudienceSpec Spec { get; set; } = AudienceSpec.Custom();
}

/// <summary>The body of <see cref="Resources.AdsResource.CreateLeadFormAsync"/>.</summary>
public sealed class CreateLeadFormOptions
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;

    [JsonPropertyName("pageId")]
    public string PageId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One to three of <see cref="LeadFormQuestions"/>.</summary>
    [JsonPropertyName("questions")]
    public IList<string> Questions { get; set; } = new List<string>();

    [JsonPropertyName("privacyPolicyUrl")]
    public string PrivacyPolicyUrl { get; set; } = string.Empty;

    [JsonPropertyName("thankYouMessage")]
    public string ThankYouMessage { get; set; } = string.Empty;

    [JsonPropertyName("followUpUrl")]
    public string? FollowUpUrl { get; set; }
}
