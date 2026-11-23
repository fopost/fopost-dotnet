using System.Text.Json.Serialization;

namespace FoPost.Resources;

/// <summary>The body of <see cref="AdsResource.CreateReachFrequencyAsync"/>. Times are ISO 8601.</summary>
public sealed class CreateReachFrequencyOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("targeting")]
    public AdTargeting Targeting { get; set; } = new();

    [JsonPropertyName("placements")]
    public IList<string> Placements { get; set; } = new List<string>();

    [JsonPropertyName("budgetMinor")]
    public long BudgetMinor { get; set; }

    [JsonPropertyName("startAt")]
    public string StartAt { get; set; } = string.Empty;

    [JsonPropertyName("endAt")]
    public string EndAt { get; set; } = string.Empty;

    /// <summary>How often one person should see the ad over the flight.</summary>
    [JsonPropertyName("frequencyCap")]
    public int? FrequencyCap { get; set; }
}

/// <summary>Reserves or cancels a prediction. Reserving spends on the ad account.</summary>
public sealed class ReachFrequencyActionOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;
}

/// <summary>Asks a creator for partnership permission.</summary>
public sealed class PartnershipOptions : AdConnectionRequestOptions
{
    [JsonPropertyName("pageId")]
    public string PageId { get; set; } = string.Empty;

    /// <summary>The creator's account id.</summary>
    [JsonPropertyName("creatorId")]
    public string CreatorId { get; set; } = string.Empty;
}

/// <summary>Creates or renames an ad label.</summary>
public sealed class AdLabelOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Puts a label on a campaign, ad set or ad, keeping whatever labels it already carries.
/// </summary>
public sealed class ApplyAdLabelOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("objectId")]
    public string ObjectId { get; set; } = string.Empty;

    /// <summary><c>campaign</c>, <c>ad_set</c> or <c>ad</c>.</summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = "campaign";
}

/// <summary>One arm of an A/B study.</summary>
public sealed class AdStudyCell
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>The campaigns this cell tests.</summary>
    [JsonPropertyName("objectIds")]
    public IList<string> ObjectIds { get; set; } = new List<string>();
}

/// <summary>
/// An A/B study splitting traffic evenly across two to five cells. Times are ISO 8601.
/// </summary>
public sealed class CreateAdStudyOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("startAt")]
    public string StartAt { get; set; } = string.Empty;

    [JsonPropertyName("endAt")]
    public string EndAt { get; set; } = string.Empty;

    [JsonPropertyName("cells")]
    public IList<AdStudyCell> Cells { get; set; } = new List<AdStudyCell>();
}

/// <summary>
/// Tells the ad platform to expect heavier spend over a window, so pacing allows for it.
/// </summary>
public sealed class CreateHighDemandPeriodOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("startAt")]
    public string StartAt { get; set; } = string.Empty;

    [JsonPropertyName("endAt")]
    public string EndAt { get; set; } = string.Empty;

    [JsonPropertyName("budgetValue")]
    public double BudgetValue { get; set; }

    /// <summary>One of <see cref="BudgetValueTypes"/>.</summary>
    [JsonPropertyName("budgetValueType")]
    public string BudgetValueType { get; set; } = BudgetValueTypes.Absolute;
}

/// <summary>Weights conversions so some audiences count for more than others.</summary>
public sealed class CreateValueRuleSetOptions : AdConnectionRequestOptions
{
    /// <summary>Ad account id, <c>act_…</c>.</summary>
    [JsonPropertyName("adAccountId")]
    public string AdAccountId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("rules")]
    public IList<ValueRule> Rules { get; set; } = new List<ValueRule>();
}
