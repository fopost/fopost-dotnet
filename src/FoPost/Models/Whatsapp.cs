using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>
/// The business profile on a WhatsApp number, plus how the platform rates it.
/// </summary>
public sealed class WhatsappProfile : FoPostModel
{
    [JsonPropertyName("about")]
    public string? About { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("vertical")]
    public string? Vertical { get; set; }

    [JsonPropertyName("websites")]
    public IReadOnlyList<string> Websites { get; set; } = Array.Empty<string>();

    [JsonPropertyName("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>The platform's review state for the display name.</summary>
    [JsonPropertyName("displayNameStatus")]
    public string? DisplayNameStatus { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("qualityRating")]
    public string? QualityRating { get; set; }

    [JsonPropertyName("messagingLimitTier")]
    public string? MessagingLimitTier { get; set; }
}

/// <summary>
/// A message template. <c>Status</c> is the review outcome the platform
/// assigned; nothing marks a template approved but the platform.
/// </summary>
public sealed class WhatsappTemplate : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("rejectedReason")]
    public string? RejectedReason { get; set; }

    [JsonPropertyName("components")]
    public IReadOnlyList<JsonNode?> Components { get; set; } = Array.Empty<JsonNode?>();

    [JsonPropertyName("qualityScore")]
    public string? QualityScore { get; set; }
}

/// <summary>
/// A group on the business number. Participation is invite-only: no endpoint
/// adds anyone, so <c>InviteLink</c> is how they join.
/// </summary>
public sealed class WhatsappGroup : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("participantCount")]
    public int? ParticipantCount { get; set; }

    [JsonPropertyName("inviteLink")]
    public string? InviteLink { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }
}

/// <summary>What the platform took and what it refused.</summary>
public sealed class WhatsappBlockResult : FoPostModel
{
    [JsonPropertyName("blocked")]
    public IReadOnlyList<string> Blocked { get; set; } = Array.Empty<string>();

    [JsonPropertyName("unblocked")]
    public IReadOnlyList<string> Unblocked { get; set; } = Array.Empty<string>();

    [JsonPropertyName("failed")]
    public IReadOnlyList<string> Failed { get; set; } = Array.Empty<string>();
}

/// <summary>Whether the cart and catalog show on the number.</summary>
public sealed class WhatsappCommerceSettings : FoPostModel
{
    [JsonPropertyName("cartEnabled")]
    public bool? CartEnabled { get; set; }

    [JsonPropertyName("catalogVisible")]
    public bool? CatalogVisible { get; set; }

    [JsonPropertyName("catalogId")]
    public string? CatalogId { get; set; }
}

/// <summary>One problem the platform found in a flow definition.</summary>
public sealed class WhatsappFlowValidationError : FoPostModel
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>An in-chat form. The platform validates it and owns its status.</summary>
public sealed class WhatsappFlow : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>DRAFT, PUBLISHED, DEPRECATED or BLOCKED.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public IReadOnlyList<string> Categories { get; set; } = Array.Empty<string>();

    [JsonPropertyName("validationErrors")]
    public IReadOnlyList<WhatsappFlowValidationError> ValidationErrors { get; set; } =
        Array.Empty<WhatsappFlowValidationError>();

    [JsonPropertyName("endpointUri")]
    public string? EndpointUri { get; set; }

    [JsonPropertyName("jsonVersion")]
    public string? JsonVersion { get; set; }

    [JsonPropertyName("previewUrl")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("previewExpiresAt")]
    public DateTimeOffset? PreviewExpiresAt { get; set; }
}

/// <summary>
/// The platform's verdict on an uploaded definition. It answers with the errors
/// rather than refusing the upload, so they arrive as data.
/// </summary>
public sealed class WhatsappFlowJsonResult : FoPostModel
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("validationErrors")]
    public IReadOnlyList<WhatsappFlowValidationError> ValidationErrors { get; set; } =
        Array.Empty<WhatsappFlowValidationError>();
}

/// <summary>What one person submitted through a flow.</summary>
public sealed class WhatsappFlowResponse : FoPostModel
{
    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("waId")]
    public string? WaId { get; set; }

    [JsonPropertyName("flowToken")]
    public string? FlowToken { get; set; }

    [JsonPropertyName("answers")]
    public JsonNode? Answers { get; set; }

    [JsonPropertyName("respondedAt")]
    public DateTimeOffset? RespondedAt { get; set; }
}

/// <summary>
/// Whether a business public key is registered. The key itself never comes back.
/// </summary>
public sealed class WhatsappEncryptionKeyStatus : FoPostModel
{
    [JsonPropertyName("hasKey")]
    public bool HasKey { get; set; }

    [JsonPropertyName("signatureStatus")]
    public string? SignatureStatus { get; set; }
}

/// <summary>
/// A sandbox invitation. Only the last four digits of the tester's number
/// travel; the number itself is never stored.
/// </summary>
public sealed class WhatsappSandboxSession : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary><c>invited</c>, <c>active</c> or <c>expired</c>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("phoneNumberLast4")]
    public string PhoneNumberLast4 { get; set; } = string.Empty;

    [JsonPropertyName("invitedAt")]
    public DateTimeOffset? InvitedAt { get; set; }

    [JsonPropertyName("activatedAt")]
    public DateTimeOffset? ActivatedAt { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }
}
