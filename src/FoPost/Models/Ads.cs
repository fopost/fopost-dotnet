using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>What a boost or ad is optimised for.</summary>
public static class AdGoals
{
    public const string Engagement = "engagement";
    public const string Traffic = "traffic";
    public const string Awareness = "awareness";
    public const string VideoViews = "video_views";
}

/// <summary>How a budget is spent.</summary>
public static class AdBudgetTypes
{
    public const string Daily = "daily";
    public const string Lifetime = "lifetime";
}

/// <summary>The delivery states a caller can ask for.</summary>
public static class AdStatuses
{
    public const string Active = "active";
    public const string Paused = "paused";
}

/// <summary>What <see cref="Resources.AdsResource.SearchTargetingAsync"/> can look up.</summary>
public static class TargetingSearchTypes
{
    public const string Country = "country";
    public const string Region = "region";
    public const string City = "city";
    public const string Zip = "zip";
    public const string Metro = "metro";
    public const string Interest = "interest";
    public const string Behavior = "behavior";
    public const string Income = "income";
}

/// <summary>The kinds of audience <see cref="Resources.AdsResource.CreateAudienceAsync"/> builds.</summary>
public static class AudienceSubtypes
{
    public const string Custom = "CUSTOM";
    public const string Lookalike = "LOOKALIKE";
    public const string Website = "WEBSITE";
}

/// <summary>The questions a lead form can ask.</summary>
public static class LeadFormQuestions
{
    public const string Email = "EMAIL";
    public const string FullName = "FULL_NAME";
    public const string Phone = "PHONE";
}

/// <summary>An interest, behaviour, or income bracket, as the ad platform names it.</summary>
public sealed class AdTargetingItem : FoPostModel
{
    public AdTargetingItem()
    {
    }

    public AdTargetingItem(string id, string name)
    {
        Id = id;
        Name = name;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>A location below country level, from <see cref="Resources.AdsResource.SearchTargetingAsync"/>.</summary>
public sealed class AdTargetingLocation : FoPostModel
{
    public AdTargetingLocation()
    {
    }

    public AdTargetingLocation(string key, string name, string type)
    {
        Key = key;
        Name = name;
        Type = type;
    }

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary><c>region</c>, <c>city</c>, <c>zip</c>, or <c>geo_market</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>Who an ad is shown to. At least one country or one location is required.</summary>
public sealed class AdTargeting : FoPostModel
{
    /// <summary>ISO 3166-1 alpha-2 codes.</summary>
    [JsonPropertyName("countries")]
    public IList<string> Countries { get; set; } = new List<string>();

    [JsonPropertyName("ageMin")]
    public int AgeMin { get; set; } = 18;

    [JsonPropertyName("ageMax")]
    public int AgeMax { get; set; } = 65;

    /// <summary><c>all</c>, <c>male</c>, or <c>female</c>.</summary>
    [JsonPropertyName("gender")]
    public string Gender { get; set; } = "all";

    [JsonPropertyName("audienceIds")]
    public IList<string>? AudienceIds { get; set; }

    [JsonPropertyName("locations")]
    public IList<AdTargetingLocation>? Locations { get; set; }

    [JsonPropertyName("interests")]
    public IList<AdTargetingItem>? Interests { get; set; }

    [JsonPropertyName("behaviors")]
    public IList<AdTargetingItem>? Behaviors { get; set; }

    [JsonPropertyName("income")]
    public IList<AdTargetingItem>? Income { get; set; }
}

/// <summary>How much an ad may spend, in the ad account's currency.</summary>
public sealed class AdBudget : FoPostModel
{
    public AdBudget()
    {
    }

    public AdBudget(long minor, string type, DateTimeOffset? endAt = null)
    {
        Minor = minor;
        Type = type;
        EndAt = endAt;
    }

    /// <summary>Amount in minor units (cents for USD).</summary>
    [JsonPropertyName("minor")]
    public long Minor { get; set; }

    /// <summary>One of <see cref="AdBudgetTypes"/>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = AdBudgetTypes.Daily;

    [JsonPropertyName("endAt")]
    public DateTimeOffset? EndAt { get; set; }
}

/// <summary>Lifetime delivery figures from the last refresh.</summary>
public sealed class AdInsights : FoPostModel
{
    [JsonPropertyName("impressions")]
    public long Impressions { get; set; }

    [JsonPropertyName("reach")]
    public long Reach { get; set; }

    [JsonPropertyName("clicks")]
    public long Clicks { get; set; }

    /// <summary>Account currency, minor units.</summary>
    [JsonPropertyName("spendMinor")]
    public long SpendMinor { get; set; }
}

/// <summary>The copy and media of an ad built from scratch.</summary>
public sealed class AdCreative : FoPostModel
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("headline")]
    public string? Headline { get; set; }

    [JsonPropertyName("destinationUrl")]
    public string? DestinationUrl { get; set; }

    [JsonPropertyName("mediaUrl")]
    public string? MediaUrl { get; set; }
}

/// <summary>A boost or ad created through FoPost.</summary>
public sealed class Ad : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    /// <summary><c>boost</c> or <c>ad</c>.</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="AdGoals"/>.</summary>
    [JsonPropertyName("goal")]
    public string? Goal { get; set; }

    /// <summary>What was asked for: one of <see cref="AdStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>The ad platform's own delivery status, from the last refresh.</summary>
    [JsonPropertyName("effectiveStatus")]
    public string? EffectiveStatus { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    /// <summary>The connected account a boost was built from.</summary>
    [JsonPropertyName("accountId")]
    public string? AccountId { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("adAccountId")]
    public string? AdAccountId { get; set; }

    /// <summary>The FoPost post a boost promotes.</summary>
    [JsonPropertyName("sourcePostId")]
    public string? SourcePostId { get; set; }

    [JsonPropertyName("budgetMinor")]
    public long BudgetMinor { get; set; }

    /// <summary>One of <see cref="AdBudgetTypes"/>.</summary>
    [JsonPropertyName("budgetType")]
    public string? BudgetType { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("endAt")]
    public DateTimeOffset? EndAt { get; set; }

    [JsonPropertyName("targeting")]
    public AdTargeting? Targeting { get; set; }

    [JsonPropertyName("creative")]
    public AdCreative? Creative { get; set; }

    [JsonPropertyName("insights")]
    public AdInsights? Insights { get; set; }

    [JsonPropertyName("insightsAt")]
    public DateTimeOffset? InsightsAt { get; set; }

    [JsonPropertyName("lastError")]
    public string? LastError { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }
}

/// <summary>An ad on a connected ad account that was made elsewhere. Read live, never stored.</summary>
public sealed class ExternalAd : FoPostModel
{
    /// <summary>The ad platform's own id.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("effectiveStatus")]
    public string? EffectiveStatus { get; set; }

    [JsonPropertyName("campaignId")]
    public string? CampaignId { get; set; }

    [JsonPropertyName("campaignName")]
    public string? CampaignName { get; set; }

    [JsonPropertyName("objective")]
    public string? Objective { get; set; }

    [JsonPropertyName("budgetMinor")]
    public long? BudgetMinor { get; set; }

    [JsonPropertyName("budgetType")]
    public string? BudgetType { get; set; }

    [JsonPropertyName("endAt")]
    public DateTimeOffset? EndAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("connectionId")]
    public string? ConnectionId { get; set; }

    [JsonPropertyName("adAccountId")]
    public string? AdAccountId { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>An ad platform grant a workspace holds.</summary>
public sealed class AdConnection : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary><c>meta</c>.</summary>
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    /// <summary><c>business</c> or <c>user</c>.</summary>
    [JsonPropertyName("authType")]
    public string? AuthType { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("businessId")]
    public string? BusinessId { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>An ad account a connection's grant reaches.</summary>
public sealed class AdSourceAccount : FoPostModel
{
    /// <summary><c>act_…</c></summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    /// <summary>The ad platform's account status code; 1 is active.</summary>
    [JsonPropertyName("status")]
    public int? Status { get; set; }
}

/// <summary>A Page a connection's grant reaches.</summary>
public sealed class AdSourcePage : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("instagramUserId")]
    public string? InstagramUserId { get; set; }
}

/// <summary>A connection with the ad accounts and Pages its grant reaches.</summary>
public sealed class AdSource : FoPostModel
{
    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("adAccounts")]
    public IList<AdSourceAccount> AdAccounts { get; set; } = new List<AdSourceAccount>();

    [JsonPropertyName("pages")]
    public IList<AdSourcePage> Pages { get; set; } = new List<AdSourcePage>();

    /// <summary>Set when the ad platform refused the listing, usually a revoked grant.</summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>Where a boostable post was delivered.</summary>
public sealed class BoostableDelivery : FoPostModel
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("externalUrl")]
    public string? ExternalUrl { get; set; }

    [JsonPropertyName("postedAt")]
    public DateTimeOffset? PostedAt { get; set; }
}

/// <summary>A published post that can be boosted.</summary>
public sealed class BoostablePost : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("deliveries")]
    public IList<BoostableDelivery> Deliveries { get; set; } = new List<BoostableDelivery>();
}

/// <summary>A saved audience on an ad account.</summary>
public sealed class Audience : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="AudienceSubtypes"/>.</summary>
    [JsonPropertyName("subtype")]
    public string? Subtype { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("sizeLower")]
    public long? SizeLower { get; set; }

    [JsonPropertyName("sizeUpper")]
    public long? SizeUpper { get; set; }

    [JsonPropertyName("deliveryStatus")]
    public string? DeliveryStatus { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }
}

/// <summary>A tracking pixel on an ad account.</summary>
public sealed class AdPixel : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>The audiences and pixels on one ad account.</summary>
public sealed class AudiencesResult : FoPostModel
{
    [JsonPropertyName("audiences")]
    public IList<Audience> Audiences { get; set; } = new List<Audience>();

    [JsonPropertyName("pixels")]
    public IList<AdPixel> Pixels { get; set; } = new List<AdPixel>();

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>A newly created audience.</summary>
public sealed class CreatedAudience : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Emails the ad platform accepted into a custom audience.</summary>
    [JsonPropertyName("added")]
    public int Added { get; set; }
}

/// <summary>A location, interest, behaviour, or income bracket the ad platform can target.</summary>
public sealed class TargetingOption : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }
}

/// <summary>A lead form on a Page.</summary>
public sealed class LeadForm : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("leadsCount")]
    public int LeadsCount { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("questions")]
    public IList<string> Questions { get; set; } = new List<string>();
}

/// <summary>A connection's Page with the lead forms on it.</summary>
public sealed class LeadFormSource : FoPostModel
{
    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;

    [JsonPropertyName("connectionName")]
    public string? ConnectionName { get; set; }

    [JsonPropertyName("pageId")]
    public string? PageId { get; set; }

    [JsonPropertyName("pageName")]
    public string? PageName { get; set; }

    [JsonPropertyName("forms")]
    public IList<LeadForm> Forms { get; set; } = new List<LeadForm>();

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>One answer on a lead.</summary>
public sealed class LeadField : FoPostModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public IList<string> Values { get; set; } = new List<string>();
}

/// <summary>A lead a form collected.</summary>
public sealed class Lead : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("fields")]
    public IList<LeadField> Fields { get; set; } = new List<LeadField>();

    [JsonPropertyName("adName")]
    public string? AdName { get; set; }

    [JsonPropertyName("campaignName")]
    public string? CampaignName { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("isOrganic")]
    public bool IsOrganic { get; set; }
}

/// <summary>One page of leads; pass <see cref="NextCursor"/> back as <c>after</c> for the next.</summary>
public sealed class LeadsPage : FoPostModel
{
    [JsonPropertyName("leads")]
    public IList<Lead> Leads { get; set; } = new List<Lead>();

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}
