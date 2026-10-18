using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>The states a hosted connect link moves through.</summary>
public static class ConnectSessionStatuses
{
    public const string Pending = "pending";
    public const string Used = "used";
    public const string Expired = "expired";
}

/// <summary>A hosted connect link: a URL your own user opens to connect a network to the workspace.</summary>
public sealed class ConnectSession : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>One of <see cref="ConnectSessionStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Your own id for the user the link was made for.</summary>
    [JsonPropertyName("external_user_id")]
    public string ExternalUserId { get; set; } = string.Empty;

    [JsonPropertyName("platforms")]
    public IList<string> Platforms { get; set; } = new List<string>();

    [JsonPropertyName("redirect_url")]
    public string? RedirectUrl { get; set; }

    [JsonPropertyName("branding")]
    public ConnectSessionBranding? Branding { get; set; }

    /// <summary>The account that was connected, once the link was used.</summary>
    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    /// <summary>The URL to send the user to. Only present while the link is pending.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }

    [JsonPropertyName("used_at")]
    public DateTimeOffset? UsedAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}

/// <summary>What the hosted page shows as the brand.</summary>
public sealed class ConnectSessionBranding : FoPostModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }
}
