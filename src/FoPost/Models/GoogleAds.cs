using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>A portfolio bid strategy on the account.</summary>
/// <remarks>
/// <c>Id</c> is <c>&lt;customerId&gt;~&lt;kind&gt;~&lt;id&gt;</c>:
/// a Google resource name has slashes and cannot ride in a URL path segment,
/// so every id here carries the account it belongs to.
/// </remarks>
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

/// <summary>Google portfolio bid strategy types.</summary>
public static class GoogleBidStrategyTypes
{
    public const string TargetSpend = "TARGET_SPEND";
    public const string MaximizeConversions = "MAXIMIZE_CONVERSIONS";
    public const string MaximizeConversionValue = "MAXIMIZE_CONVERSION_VALUE";
    public const string TargetCpa = "TARGET_CPA";
    public const string TargetRoas = "TARGET_ROAS";
}
