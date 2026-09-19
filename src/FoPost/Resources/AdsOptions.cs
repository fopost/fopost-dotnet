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

    /// <summary>Query string appended to every link in the ad, e.g. <c>utm_source=meta&amp;utm_medium=paid</c>.</summary>
    [JsonPropertyName("urlTags")]
    public string? UrlTags { get; set; }
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

/// <summary>The workspace and ads connection every campaign-tree write names.</summary>
public abstract class AdConnectionRequestOptions
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>An ads connection in the workspace.</summary>
    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;
}

/// <summary>The body of <see cref="Resources.AdsResource.CreateCampaignAsync"/>.</summary>
public sealed class CreateAdCampaignOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="AdGoals"/>.</summary>
    [JsonPropertyName("goal")]
    public string Goal { get; set; } = AdGoals.Engagement;

    /// <summary>Left unset, the campaign is created paused. Set to <c>false</c> to go live at once.</summary>
    [JsonPropertyName("paused")]
    public bool? Paused { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.UpdateCampaignAsync"/>; unset fields are left alone.</summary>
public sealed class UpdateAdCampaignOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>One of <see cref="AdStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.CreateAdSetAsync"/>.</summary>
public sealed class CreateAdSetOptions : AdConnectionRequestOptions
{
    [JsonPropertyName("campaignId")]
    public string CampaignId { get; set; } = string.Empty;

    /// <summary>The Page the ads in this set run as.</summary>
    [JsonPropertyName("pageId")]
    public string PageId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="AdGoals"/>.</summary>
    [JsonPropertyName("goal")]
    public string Goal { get; set; } = AdGoals.Engagement;

    [JsonPropertyName("budget")]
    public AdBudget Budget { get; set; } = new();

    [JsonPropertyName("targeting")]
    public AdTargeting Targeting { get; set; } = new();

    /// <summary>Left unset, the ad set is created paused. Set to <c>false</c> to go live at once.</summary>
    [JsonPropertyName("paused")]
    public bool? Paused { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.UpdateAdSetAsync"/>; unset fields are left alone.</summary>
public sealed class UpdateAdSetOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>One of <see cref="AdStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>New budget in minor units; the budget type set at creation stays.</summary>
    [JsonPropertyName("budgetMinor")]
    public long? BudgetMinor { get; set; }

    [JsonPropertyName("endAt")]
    public DateTimeOffset? EndAt { get; set; }

    [JsonPropertyName("targeting")]
    public AdTargeting? Targeting { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.CreateNetworkAdAsync"/>.</summary>
public sealed class CreateNetworkAdOptions : AdConnectionRequestOptions
{
    [JsonPropertyName("adSetId")]
    public string AdSetId { get; set; } = string.Empty;

    /// <summary>From <see cref="Resources.AdsResource.CreateCreativeAsync"/> or the creative library.</summary>
    [JsonPropertyName("creativeId")]
    public string CreativeId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Left unset, the ad is created paused. Set to <c>false</c> to go live at once.</summary>
    [JsonPropertyName("paused")]
    public bool? Paused { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.UpdateNetworkAdAsync"/>; unset fields are left alone.</summary>
public sealed class UpdateNetworkAdOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>One of <see cref="AdStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("creativeId")]
    public string? CreativeId { get; set; }
}

/// <summary>A campaign, ad set, or ad named in <see cref="BulkAdStatusOptions"/>.</summary>
public sealed class AdObjectRef
{
    public AdObjectRef()
    {
    }

    public AdObjectRef(string id, string level)
    {
        Id = id;
        Level = level;
    }

    /// <summary>The ad platform's id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>One of <see cref="AdObjectLevels"/>.</summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = AdObjectLevels.Campaign;
}

/// <summary>The body of <see cref="Resources.AdsResource.BulkSetStatusAsync"/>.</summary>
public sealed class BulkAdStatusOptions : AdConnectionRequestOptions
{
    /// <summary>One of <see cref="AdStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = AdStatuses.Paused;

    /// <summary>One to 50 objects.</summary>
    [JsonPropertyName("objects")]
    public IList<AdObjectRef> Objects { get; set; } = new List<AdObjectRef>();
}

/// <summary>One card of a carousel creative.</summary>
public sealed class AdCreativeCard
{
    /// <summary>A media library image.</summary>
    [JsonPropertyName("mediaUrl")]
    public string MediaUrl { get; set; } = string.Empty;

    [JsonPropertyName("destinationUrl")]
    public string? DestinationUrl { get; set; }

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.CreateCreativeAsync"/>.</summary>
public sealed class CreateAdCreativeOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("pageId")]
    public string PageId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="AdCreativeFormats"/>.</summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = AdCreativeFormats.Image;

    /// <summary>Primary text, up to 2000 characters.</summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("destinationUrl")]
    public string? DestinationUrl { get; set; }

    /// <summary>For example <c>LEARN_MORE</c> (the default), <c>SHOP_NOW</c>, or <c>SIGN_UP</c>.</summary>
    [JsonPropertyName("callToAction")]
    public string? CallToAction { get; set; }

    /// <summary>Query string appended to every link in the ad, e.g. <c>utm_source=meta&amp;utm_medium=paid</c>.</summary>
    [JsonPropertyName("urlTags")]
    public string? UrlTags { get; set; }

    /// <summary>A media library asset url: the image, or the video. Required for a video.</summary>
    [JsonPropertyName("mediaUrl")]
    public string? MediaUrl { get; set; }

    /// <summary>A video's poster frame, as a library image.</summary>
    [JsonPropertyName("thumbnailMediaUrl")]
    public string? ThumbnailMediaUrl { get; set; }

    /// <summary>Two to ten cards; required for a carousel.</summary>
    [JsonPropertyName("cards")]
    public IList<AdCreativeCard>? Cards { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.UpdateAudienceAsync"/>; unset fields are left alone.</summary>
public sealed class UpdateAudienceOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>The body of <see cref="Resources.AdsResource.EstimateReachAsync"/>.</summary>
public sealed class EstimateReachOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("pageId")]
    public string PageId { get; set; } = string.Empty;

    [JsonPropertyName("targeting")]
    public AdTargeting Targeting { get; set; } = new();
}
