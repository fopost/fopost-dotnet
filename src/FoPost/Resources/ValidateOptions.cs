using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>A media item as sent to <see cref="Resources.ValidateResource.PostAsync"/>.</summary>
public sealed class ValidateMediaItem
{
    /// <summary>Public http(s) URL of the file.</summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("mime_type")]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Size in bytes, when known.</summary>
    [JsonPropertyName("size")]
    public long? Size { get; set; }
}

/// <summary>The body of <see cref="Resources.ValidateResource.PostAsync"/>.</summary>
public sealed class ValidatePostOptions
{
    public string Content { get; set; } = string.Empty;

    /// <summary>Up to 20 items.</summary>
    public IList<ValidateMediaItem> Media { get; set; } = new List<ValidateMediaItem>();

    /// <summary>At least one, from <see cref="Platforms"/>.</summary>
    public IList<string> Platforms { get; set; } = new List<string>();
}

/// <summary>The body of <see cref="Resources.ValidateResource.LengthAsync"/>.</summary>
public sealed class ValidateLengthOptions
{
    public string Text { get; set; } = string.Empty;

    /// <summary>At least one, from <see cref="Platforms"/>.</summary>
    public IList<string> Platforms { get; set; } = new List<string>();
}
