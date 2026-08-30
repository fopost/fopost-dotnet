using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>Credits charged by one AI call, and what is left afterwards.</summary>
public sealed class AiCredits : FoPostModel
{
    [JsonPropertyName("charged")]
    public int Charged { get; set; }

    [JsonPropertyName("remaining")]
    public int Remaining { get; set; }
}

/// <summary>AI credit balance for the current billing period.</summary>
public sealed class AiCreditBalance : FoPostModel
{
    [JsonPropertyName("credits_remaining")]
    public int CreditsRemaining { get; set; }

    [JsonPropertyName("credits_used")]
    public int CreditsUsed { get; set; }

    [JsonPropertyName("credits_total")]
    public int CreditsTotal { get; set; }

    [JsonPropertyName("period_start")]
    public DateTimeOffset? PeriodStart { get; set; }

    [JsonPropertyName("period_end")]
    public DateTimeOffset? PeriodEnd { get; set; }
}

/// <summary>A generated caption, plus what it cost.</summary>
public sealed class CaptionResult : FoPostModel
{
    [JsonPropertyName("caption")]
    public string Caption { get; set; } = string.Empty;

    [JsonPropertyName("credits")]
    public AiCredits? Credits { get; set; }
}

/// <summary>One platform's rewrite of a draft.</summary>
public sealed class RewriteVariant : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("credits")]
    public int? Credits { get; set; }
}

/// <summary>A draft rewritten once per target platform.</summary>
public sealed class RewriteResult : FoPostModel
{
    [JsonPropertyName("results")]
    public IList<RewriteVariant> Results { get; set; } = new List<RewriteVariant>();

    [JsonPropertyName("credits")]
    public AiCredits? Credits { get; set; }
}

/// <summary>An article turned into a post per platform.</summary>
public sealed class RepurposeResult : FoPostModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>The generated post for each platform, keyed by platform name.</summary>
    [JsonPropertyName("posts")]
    public IDictionary<string, string> Posts { get; set; } = new Dictionary<string, string>();

    [JsonPropertyName("credits")]
    public AiCredits? Credits { get; set; }
}
