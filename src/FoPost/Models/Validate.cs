using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>An advisory note from validation. Never blocks publishing.</summary>
public sealed class ValidationSignal : FoPostModel
{
    /// <summary>One of <c>info</c> or <c>warn</c>.</summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>One platform's verdict on a draft post.</summary>
public sealed class PlatformPostValidation : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("ready")]
    public bool Ready { get; set; }

    /// <summary>Hard blockers that would prevent publishing.</summary>
    [JsonPropertyName("issues")]
    public IList<string> Issues { get; set; } = new List<string>();

    /// <summary>Advisory 0-100, when scored.</summary>
    [JsonPropertyName("score")]
    public double? Score { get; set; }

    [JsonPropertyName("signals")]
    public IList<ValidationSignal> Signals { get; set; } = new List<ValidationSignal>();
}

/// <summary>The result of <see cref="Resources.ValidateResource.PostAsync"/>.</summary>
public sealed class PostValidation : FoPostModel
{
    /// <summary>True only when every platform is ready.</summary>
    [JsonPropertyName("ready")]
    public bool Ready { get; set; }

    [JsonPropertyName("platforms")]
    public IList<PlatformPostValidation> Platforms { get; set; } = new List<PlatformPostValidation>();
}

/// <summary>How one platform counts the text.</summary>
public sealed class PlatformLength : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    /// <summary>What the platform counts, in <see cref="Unit"/>.</summary>
    [JsonPropertyName("length")]
    public int Length { get; set; }

    /// <summary>Null when the platform has no text limit.</summary>
    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    /// <summary>One of <c>chars</c> or <c>bytes</c>.</summary>
    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("signals")]
    public IList<ValidationSignal> Signals { get; set; } = new List<ValidationSignal>();
}

/// <summary>The result of <see cref="Resources.ValidateResource.LengthAsync"/>.</summary>
public sealed class LengthValidation : FoPostModel
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("platforms")]
    public IList<PlatformLength> Platforms { get; set; } = new List<PlatformLength>();
}

/// <summary>The result of <see cref="Resources.ValidateResource.MediaAsync"/>.</summary>
public sealed class MediaValidation : FoPostModel
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("issues")]
    public IList<string> Issues { get; set; } = new List<string>();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Bytes fetched.</summary>
    [JsonPropertyName("size")]
    public long Size { get; set; }

    /// <summary>Present only when <see cref="Ok"/>.</summary>
    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    /// <summary>One of <c>image</c>, <c>video</c>, <c>audio</c>, or <c>document</c>. Present only when <see cref="Ok"/>.</summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
