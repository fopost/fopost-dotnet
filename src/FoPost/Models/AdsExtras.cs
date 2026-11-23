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
