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

/// <summary>Add a keyword to an ad group.</summary>
public sealed class CreateGoogleKeywordOptions : GoogleAdsScope
{
    [JsonPropertyName("adGroupId")]
    public string AdGroupId { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>One of <see cref="GoogleMatchTypes"/>.</summary>
    [JsonPropertyName("matchType")]
    public string MatchType { get; set; } = string.Empty;

    /// <summary>The account's currency, in minor units.</summary>
    [JsonPropertyName("cpcBidMinor")]
    public long? CpcBidMinor { get; set; }
}

/// <summary>Pause, resume, or rebid a keyword.</summary>
public sealed class UpdateGoogleKeywordOptions : GoogleAdsScope
{
    /// <summary><c>active</c> or <c>paused</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("cpcBidMinor")]
    public long? CpcBidMinor { get; set; }
}

/// <summary>Ask for keyword ideas from seeds, a landing page, or both.</summary>
public sealed class GoogleKeywordIdeasOptions : GoogleAdsScope
{
    [JsonPropertyName("seeds")]
    public IList<string>? Seeds { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("languageId")]
    public string? LanguageId { get; set; }

    [JsonPropertyName("geoTargetIds")]
    public IList<string>? GeoTargetIds { get; set; }
}

/// <summary>Read the historical metrics of keywords you already have.</summary>
public sealed class GoogleKeywordMetricsOptions : GoogleAdsScope
{
    [JsonPropertyName("keywords")]
    public IList<string> Keywords { get; set; } = new List<string>();
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

/// <summary>Create a negative keyword list.</summary>
public sealed class CreateGoogleNegativeKeywordListOptions : GoogleAdsScope
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

/// <summary>One keyword in a negative list.</summary>
public sealed class GoogleNegativeKeyword
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("matchType")]
    public string MatchType { get; set; } = string.Empty;
}

/// <summary>Add keywords to a negative list.</summary>
public sealed class AddGoogleNegativeKeywordsOptions : GoogleAdsScope
{
    [JsonPropertyName("sharedSetId")]
    public string SharedSetId { get; set; } = string.Empty;

    [JsonPropertyName("keywords")]
    public IList<GoogleNegativeKeyword> Keywords { get; set; } = new List<GoogleNegativeKeyword>();
}

/// <summary>Put a negative keyword list on a campaign.</summary>
public sealed class AttachGoogleNegativeKeywordListOptions : GoogleAdsScope
{
    [JsonPropertyName("sharedSetId")]
    public string SharedSetId { get; set; } = string.Empty;

    [JsonPropertyName("campaignId")]
    public string CampaignId { get; set; } = string.Empty;
}

/// <summary>
/// The asset to create. <c>Kind</c> picks which other fields apply:
/// <c>Text</c> and <c>FinalUrl</c> for <c>sitelink</c>, <c>Text</c> alone for
/// <c>callout</c>, <c>Header</c> and <c>Values</c> for <c>snippet</c>.
/// </summary>
public sealed class GoogleAssetSpec
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("description1")]
    public string? Description1 { get; set; }

    [JsonPropertyName("description2")]
    public string? Description2 { get; set; }

    [JsonPropertyName("finalUrl")]
    public string? FinalUrl { get; set; }

    [JsonPropertyName("header")]
    public string? Header { get; set; }

    [JsonPropertyName("values")]
    public IList<string>? Values { get; set; }
}

/// <summary>Add an asset to the library.</summary>
public sealed class CreateGoogleAssetOptions : GoogleAdsScope
{
    [JsonPropertyName("spec")]
    public GoogleAssetSpec Spec { get; set; } = new();
}

/// <summary>Attach an asset to the account, or to one campaign.</summary>
public sealed class AttachGoogleAssetOptions : GoogleAdsScope
{
    [JsonPropertyName("assetId")]
    public string AssetId { get; set; } = string.Empty;

    /// <summary>One of <see cref="GoogleAssetFieldTypes"/>.</summary>
    [JsonPropertyName("fieldType")]
    public string FieldType { get; set; } = string.Empty;

    /// <summary>Attaches to the account when left null.</summary>
    [JsonPropertyName("campaignId")]
    public string? CampaignId { get; set; }
}

/// <summary>Create a Performance Max asset group.</summary>
public sealed class CreateGoogleAssetGroupOptions : GoogleAdsScope
{
    [JsonPropertyName("campaignId")]
    public string CampaignId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("finalUrls")]
    public IList<string> FinalUrls { get; set; } = new List<string>();

    /// <summary><c>active</c> or <c>paused</c>; starts paused when left null.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>Rename, pause, or resume an asset group.</summary>
public sealed class UpdateGoogleAssetGroupOptions : GoogleAdsScope
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
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
