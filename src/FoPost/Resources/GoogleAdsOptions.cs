using System.Text.Json.Serialization;

namespace FoPost.Resources;

/// <summary>
/// The connection and the Google Ads account a call runs against. Every
/// Google Ads request carries these three.
/// </summary>
/// <remarks>
/// <c>CustomerId</c> is digits only and has to name an account the
/// connection's grant reaches: any other answers 404. <c>WorkspaceId</c> may
/// be left out on a read, and is required on a write.
/// </remarks>
public class GoogleAdsScope
{
    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;

    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;
}

/// <summary>Start a Google Ads connection.</summary>
public sealed class AuthorizeGoogleAdsOptions
{
    [JsonPropertyName("workspaceId")]
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>Dashboard path to land on after Google redirects back.</summary>
    [JsonPropertyName("returnTo")]
    public string? ReturnTo { get; set; }
}

/// <summary>Add a portfolio bid strategy.</summary>
public sealed class CreateGoogleBidStrategyOptions : GoogleAdsScope
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="GoogleBidStrategyTypes"/>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>The account's currency, where the strategy takes a target.</summary>
    [JsonPropertyName("targetMinor")]
    public long? TargetMinor { get; set; }
}

/// <summary>One slot to put on a campaign's schedule.</summary>
public sealed class GoogleAdScheduleInput
{
    [JsonPropertyName("dayOfWeek")]
    public string DayOfWeek { get; set; } = string.Empty;

    [JsonPropertyName("startHour")]
    public int StartHour { get; set; }

    [JsonPropertyName("endHour")]
    public int EndHour { get; set; }

    [JsonPropertyName("bidModifier")]
    public double? BidModifier { get; set; }
}

/// <summary>Replace a campaign's schedule; Google has no partial edit for one.</summary>
public sealed class SetGoogleAdScheduleOptions : GoogleAdsScope
{
    [JsonPropertyName("campaignId")]
    public string CampaignId { get; set; } = string.Empty;

    [JsonPropertyName("slots")]
    public IList<GoogleAdScheduleInput> Slots { get; set; } = new List<GoogleAdScheduleInput>();
}

/// <summary>Create a conversion action.</summary>
public sealed class CreateGoogleConversionActionOptions : GoogleAdsScope
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("valueMinor")]
    public long? ValueMinor { get; set; }

    [JsonPropertyName("countingType")]
    public string? CountingType { get; set; }
}

/// <summary>
/// One offline conversion. One of <c>Gclid</c>, <c>Gbraid</c>, or
/// <c>Wbraid</c> is required: it is what matches the click.
/// </summary>
public sealed class GoogleClickConversion
{
    [JsonPropertyName("gclid")]
    public string? Gclid { get; set; }

    [JsonPropertyName("gbraid")]
    public string? Gbraid { get; set; }

    [JsonPropertyName("wbraid")]
    public string? Wbraid { get; set; }

    [JsonPropertyName("conversionActionId")]
    public string ConversionActionId { get; set; } = string.Empty;

    /// <summary><c>yyyy-MM-dd HH:mm:ss+|-HH:mm</c>, the only shape Google accepts.</summary>
    [JsonPropertyName("conversionDateTime")]
    public string ConversionDateTime { get; set; } = string.Empty;

    [JsonPropertyName("valueMinor")]
    public long? ValueMinor { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }

    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }
}

/// <summary>Send offline conversions.</summary>
public sealed class UploadGoogleConversionsOptions : GoogleAdsScope
{
    [JsonPropertyName("conversions")]
    public IList<GoogleClickConversion> Conversions { get; set; } = new List<GoogleClickConversion>();
}

/// <summary>Restate, retract, or enhance a conversion already counted.</summary>
public sealed class GoogleConversionAdjustment
{
    [JsonPropertyName("conversionActionId")]
    public string ConversionActionId { get; set; } = string.Empty;

    /// <summary><c>RESTATEMENT</c>, <c>RETRACTION</c>, or <c>ENHANCEMENT</c>.</summary>
    [JsonPropertyName("adjustmentType")]
    public string AdjustmentType { get; set; } = string.Empty;

    [JsonPropertyName("adjustmentDateTime")]
    public string AdjustmentDateTime { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string? OrderId { get; set; }

    [JsonPropertyName("gclid")]
    public string? Gclid { get; set; }

    [JsonPropertyName("conversionDateTime")]
    public string? ConversionDateTime { get; set; }

    [JsonPropertyName("restatementValueMinor")]
    public long? RestatementValueMinor { get; set; }

    [JsonPropertyName("currencyCode")]
    public string? CurrencyCode { get; set; }
}

/// <summary>Send conversion adjustments.</summary>
public sealed class UploadGoogleConversionAdjustmentsOptions : GoogleAdsScope
{
    [JsonPropertyName("adjustments")]
    public IList<GoogleConversionAdjustment> Adjustments { get; set; } = new List<GoogleConversionAdjustment>();
}

/// <summary>
/// A raw read-only GAQL SELECT. The account read is <c>CustomerId</c>, never
/// anything named inside <c>Query</c>.
/// </summary>
public sealed class GoogleQueryOptions : GoogleAdsScope
{
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;
}
