using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>A keyword on an ad group.</summary>
/// <remarks>
/// <c>Id</c> is <c>&lt;customerId&gt;~keyword~&lt;adGroupId&gt;~&lt;criterionId&gt;</c>:
/// a Google resource name has slashes and cannot ride in a URL path segment,
/// so every id here carries the account it belongs to.
/// </remarks>
public sealed record GoogleKeyword
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("adGroupId")]
    public string AdGroupId { get; init; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("matchType")]
    public string MatchType { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    /// <summary>The account's currency, in minor units.</summary>
    [JsonPropertyName("cpcBidMinor")]
    public long? CpcBidMinor { get; init; }

    [JsonPropertyName("negative")]
    public bool Negative { get; init; }
}

/// <summary>A keyword idea, or the historical metrics of one.</summary>
public sealed record GoogleKeywordIdea
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("avgMonthlySearches")]
    public long AvgMonthlySearches { get; init; }

    [JsonPropertyName("competition")]
    public string? Competition { get; init; }

    [JsonPropertyName("lowTopOfPageBidMinor")]
    public long? LowTopOfPageBidMinor { get; init; }

    [JsonPropertyName("highTopOfPageBidMinor")]
    public long? HighTopOfPageBidMinor { get; init; }
}

/// <summary>What someone actually searched, with the metrics it earned.</summary>
public sealed record GoogleSearchTerm
{
    [JsonPropertyName("term")]
    public string Term { get; init; } = string.Empty;

    [JsonPropertyName("adGroupId")]
    public string? AdGroupId { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("metrics")]
    public InsightsMetrics? Metrics { get; init; }
}

/// <summary>A portfolio bid strategy on the account.</summary>
public sealed record GoogleBidStrategy
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("campaignCount")]
    public int CampaignCount { get; init; }
}

/// <summary>One slot of a campaign's ad schedule.</summary>
public sealed record GoogleAdScheduleSlot
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("dayOfWeek")]
    public string DayOfWeek { get; init; } = string.Empty;

    [JsonPropertyName("startHour")]
    public int StartHour { get; init; }

    [JsonPropertyName("endHour")]
    public int EndHour { get; init; }

    [JsonPropertyName("bidModifier")]
    public double? BidModifier { get; init; }
}

/// <summary>A negative keyword list.</summary>
public sealed record GoogleSharedSet
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("memberCount")]
    public int MemberCount { get; init; }
}

/// <summary>A sitelink, callout, or structured snippet.</summary>
public sealed record GoogleAsset
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    /// <summary>What a sitelink, callout, or snippet renders.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("finalUrl")]
    public string? FinalUrl { get; init; }
}

/// <summary>Where an asset is attached; one with no links serves nowhere.</summary>
public sealed record GoogleAssetLink
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("assetId")]
    public string AssetId { get; init; } = string.Empty;

    [JsonPropertyName("level")]
    public string Level { get; init; } = string.Empty;

    [JsonPropertyName("ownerId")]
    public string? OwnerId { get; init; }

    [JsonPropertyName("fieldType")]
    public string FieldType { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;
}

/// <summary>The account's assets with the links that place them.</summary>
public sealed record GoogleAssetsResult
{
    [JsonPropertyName("assets")]
    public IReadOnlyList<GoogleAsset> Assets { get; init; } = Array.Empty<GoogleAsset>();

    [JsonPropertyName("links")]
    public IReadOnlyList<GoogleAssetLink> Links { get; init; } = Array.Empty<GoogleAssetLink>();
}

/// <summary>A Performance Max asset group.</summary>
public sealed record GoogleAssetGroup
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("campaignId")]
    public string CampaignId { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("finalUrls")]
    public IReadOnlyList<string> FinalUrls { get; init; } = Array.Empty<string>();
}

/// <summary>A lead from Local Services Ads, read live and never stored.</summary>
public sealed record GoogleLocalServicesLead
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; init; }

    [JsonPropertyName("service")]
    public string? Service { get; init; }

    [JsonPropertyName("contactName")]
    public string? ContactName { get; init; }

    [JsonPropertyName("phone")]
    public string? Phone { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; init; }
}

/// <summary>A conversion action on the account.</summary>
public sealed record GoogleConversionAction
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("countingType")]
    public string? CountingType { get; init; }

    [JsonPropertyName("valueMinor")]
    public long? ValueMinor { get; init; }
}

/// <summary>Rows exactly as Google returns them.</summary>
public sealed record GoogleQueryResult
{
    [JsonPropertyName("rows")]
    public IReadOnlyList<JsonNode?> Rows { get; init; } = Array.Empty<JsonNode?>();
}

/// <summary>Google keyword match types.</summary>
public static class GoogleMatchTypes
{
    public const string Exact = "EXACT";
    public const string Phrase = "PHRASE";
    public const string Broad = "BROAD";
}

/// <summary>Google portfolio bid strategy types.</summary>
public static class GoogleBidStrategyTypes
{
    public const string TargetSpend = "TARGET_SPEND";
    public const string MaximizeConversions = "MAXIMIZE_CONVERSIONS";
    public const string MaximizeConversionValue = "MAXIMIZE_CONVERSION_VALUE";
    public const string TargetCpa = "TARGET_CPA";
    public const string TargetRoas = "TARGET_ROAS";
}

/// <summary>Where an asset renders.</summary>
public static class GoogleAssetFieldTypes
{
    public const string Sitelink = "SITELINK";
    public const string Callout = "CALLOUT";
    public const string StructuredSnippet = "STRUCTURED_SNIPPET";
}
