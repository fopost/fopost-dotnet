using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>Where a messaging ad opens a conversation.</summary>
public static class MessagingDestinations
{
    public const string Messenger = "messenger";
    public const string InstagramDirect = "instagram_direct";
    public const string WhatsApp = "whatsapp";
}

/// <summary>How the ad platform reads a high-demand period's budget value.</summary>
public static class BudgetValueTypes
{
    public const string Absolute = "ABSOLUTE";
    public const string Multiplier = "MULTIPLIER";
}

/// <summary>A product catalog on the connection's business portfolio, read live.</summary>
public sealed class ProductCatalog : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vertical")]
    public string? Vertical { get; set; }

    [JsonPropertyName("productCount")]
    public long? ProductCount { get; set; }
}

/// <summary>The catalogs one connection reaches.</summary>
public sealed class ProductCatalogsResult : FoPostModel
{
    [JsonPropertyName("catalogs")]
    public IList<ProductCatalog> Catalogs { get; set; } = new List<ProductCatalog>();

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>One product in a catalog. <c>PriceMinor</c> is minor units of <c>Currency</c>.</summary>
public sealed class CatalogProduct : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Your own key for the product.</summary>
    [JsonPropertyName("retailerId")]
    public string RetailerId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("availability")]
    public string? Availability { get; set; }

    [JsonPropertyName("condition")]
    public string? Condition { get; set; }

    [JsonPropertyName("priceMinor")]
    public long? PriceMinor { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>One page of catalog products; pass <c>NextCursor</c> back as <c>after</c>.</summary>
public sealed class CatalogProductsPage : FoPostModel
{
    [JsonPropertyName("products")]
    public IList<CatalogProduct> Products { get; set; } = new List<CatalogProduct>();

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

/// <summary>What a catalog product batch was accepted as.</summary>
public sealed class CatalogBatchResult : FoPostModel
{
    [JsonPropertyName("handles")]
    public IList<string> Handles { get; set; } = new List<string>();

    /// <summary>Products sent in this batch.</summary>
    [JsonPropertyName("accepted")]
    public long Accepted { get; set; }
}

/// <summary>Keeps a catalog in step with a product file you host.</summary>
public sealed class ProductFeed : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Set when the ad platform fetches the file on a schedule.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("schedule")]
    public string? Schedule { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }
}

/// <summary>One run the ad platform made of a product feed.</summary>
public sealed class ProductFeedUpload : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("startedAt")]
    public string? StartedAt { get; set; }

    [JsonPropertyName("endedAt")]
    public string? EndedAt { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("errorCount")]
    public long? ErrorCount { get; set; }

    [JsonPropertyName("warningCount")]
    public long? WarningCount { get; set; }
}

/// <summary>The slice of a catalog one catalog ad runs from.</summary>
public sealed class ProductSet : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("productCount")]
    public long? ProductCount { get; set; }

    /// <summary>The ad platform's own product-set filter.</summary>
    [JsonPropertyName("filter")]
    public IDictionary<string, object?>? Filter { get; set; }
}

/// <summary>A priced flight. Nothing is bought until it is reserved.</summary>
public sealed class ReachFrequencyPrediction : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("reach")]
    public long? Reach { get; set; }

    [JsonPropertyName("impressions")]
    public long? Impressions { get; set; }

    [JsonPropertyName("frequencyCap")]
    public long? FrequencyCap { get; set; }

    /// <summary>Account currency, minor units.</summary>
    [JsonPropertyName("budgetMinor")]
    public long? BudgetMinor { get; set; }

    [JsonPropertyName("startAt")]
    public string? StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public string? EndAt { get; set; }

    /// <summary>True once the prediction holds inventory.</summary>
    [JsonPropertyName("reserved")]
    public bool Reserved { get; set; }
}

/// <summary>The predictions on one ad account.</summary>
public sealed class ReachFrequencyResult : FoPostModel
{
    [JsonPropertyName("predictions")]
    public IList<ReachFrequencyPrediction> Predictions { get; set; } = new List<ReachFrequencyPrediction>();

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>One public archive entry. Read live on every search and stored nowhere.</summary>
public sealed class AdLibraryEntry : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("pageId")]
    public string? PageId { get; set; }

    [JsonPropertyName("pageName")]
    public string? PageName { get; set; }

    [JsonPropertyName("bodies")]
    public IList<string> Bodies { get; set; } = new List<string>();

    [JsonPropertyName("titles")]
    public IList<string> Titles { get; set; } = new List<string>();

    [JsonPropertyName("linkUrls")]
    public IList<string> LinkUrls { get; set; } = new List<string>();

    [JsonPropertyName("snapshotUrl")]
    public string? SnapshotUrl { get; set; }

    [JsonPropertyName("publisherPlatforms")]
    public IList<string> PublisherPlatforms { get; set; } = new List<string>();

    [JsonPropertyName("startedAt")]
    public string? StartedAt { get; set; }

    [JsonPropertyName("endedAt")]
    public string? EndedAt { get; set; }

    /// <summary>Only on the archive's disclosure entries.</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("spendLower")]
    public long? SpendLower { get; set; }

    [JsonPropertyName("spendUpper")]
    public long? SpendUpper { get; set; }

    [JsonPropertyName("impressionsLower")]
    public long? ImpressionsLower { get; set; }

    [JsonPropertyName("impressionsUpper")]
    public long? ImpressionsUpper { get; set; }
}

/// <summary>One page of archive results.</summary>
public sealed class AdLibraryPage : FoPostModel
{
    [JsonPropertyName("entries")]
    public IList<AdLibraryEntry> Entries { get; set; } = new List<AdLibraryEntry>();

    [JsonPropertyName("nextCursor")]
    public string? NextCursor { get; set; }
}

/// <summary>A creator who allowlisted this advertiser for partnership ads.</summary>
public sealed class PartnershipCreator : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("permissions")]
    public IList<string> Permissions { get; set; } = new List<string>();
}

/// <summary>One change recorded on an ad account.</summary>
public sealed class AdActivity : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("eventType")]
    public string? EventType { get; set; }

    [JsonPropertyName("actorName")]
    public string? ActorName { get; set; }

    [JsonPropertyName("objectName")]
    public string? ObjectName { get; set; }

    [JsonPropertyName("objectType")]
    public string? ObjectType { get; set; }

    [JsonPropertyName("extraData")]
    public string? ExtraData { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }
}

/// <summary>The change log of one ad account.</summary>
public sealed class AdActivityResult : FoPostModel
{
    [JsonPropertyName("activity")]
    public IList<AdActivity> Activity { get; set; } = new List<AdActivity>();

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}

/// <summary>Groups campaigns, ad sets and ads for reporting.</summary>
public sealed class AdLabel : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }
}

/// <summary>An A/B study splitting traffic across its cells.</summary>
public sealed class AdStudy : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("startAt")]
    public string? StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public string? EndAt { get; set; }
}

/// <summary>How many iOS 14 campaigns an ad account may run at once, per app.</summary>
public sealed class IosCampaignLimits : FoPostModel
{
    [JsonPropertyName("limit")]
    public long? Limit { get; set; }

    [JsonPropertyName("used")]
    public long? Used { get; set; }

    [JsonPropertyName("appId")]
    public string? AppId { get; set; }
}

/// <summary>A window the ad platform should expect heavier spend over.</summary>
public sealed class HighDemandPeriod : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("startAt")]
    public string? StartAt { get; set; }

    [JsonPropertyName("endAt")]
    public string? EndAt { get; set; }

    [JsonPropertyName("budgetValue")]
    public double? BudgetValue { get; set; }

    [JsonPropertyName("budgetValueType")]
    public string? BudgetValueType { get; set; }
}

/// <summary>Weights one condition's conversions.</summary>
public sealed class ValueRule
{
    [JsonPropertyName("condition")]
    public string? Condition { get; set; }

    [JsonPropertyName("multiplier")]
    public double? Multiplier { get; set; }
}

/// <summary>Weights conversions so some audiences count for more than others.</summary>
public sealed class ValueRuleSet : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("rules")]
    public IList<ValueRule> Rules { get; set; } = new List<ValueRule>();
}
