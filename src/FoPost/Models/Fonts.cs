using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>The bundled font library and the fonts a workspace uploaded.</summary>
public sealed class FontList : FoPostModel
{
    [JsonPropertyName("library")]
    public IList<LibraryFont> Library { get; set; } = new List<LibraryFont>();

    [JsonPropertyName("uploaded")]
    public IList<UploadedFont> Uploaded { get; set; } = new List<UploadedFont>();
}

/// <summary>A font family that ships with FoPost.</summary>
public sealed class LibraryFont : FoPostModel
{
    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("family")]
    public string Family { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("scripts")]
    public IList<string> Scripts { get; set; } = new List<string>();

    [JsonPropertyName("weights")]
    public IList<int> Weights { get; set; } = new List<int>();

    [JsonPropertyName("files")]
    public IList<LibraryFontFile> Files { get; set; } = new List<LibraryFontFile>();
}

public sealed class LibraryFontFile : FoPostModel
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("subset")]
    public string? Subset { get; set; }

    [JsonPropertyName("weight")]
    public int? Weight { get; set; }

    [JsonPropertyName("style")]
    public string? Style { get; set; }

    [JsonPropertyName("unicode_range")]
    public string? UnicodeRange { get; set; }
}

/// <summary>A font file uploaded into a workspace.</summary>
public sealed class UploadedFont : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("family")]
    public string Family { get; set; } = string.Empty;

    /// <summary><c>normal</c> or <c>italic</c>.</summary>
    [JsonPropertyName("style")]
    public string Style { get; set; } = "normal";

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("file_url")]
    public string? FileUrl { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}
