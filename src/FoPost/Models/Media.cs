using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>An image, video, or GIF attached to a content block.</summary>
public sealed class MediaItem : FoPostModel
{
    /// <summary>One of <c>image</c>, <c>video</c>, or <c>gif</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "image";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public double? Size { get; set; }

    /// <summary>Alt text. Worth setting — several platforms surface it.</summary>
    [JsonPropertyName("alt")]
    public string? Alt { get; set; }

    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}

/// <summary>
/// One block of a post's content. A single-block post is an ordinary post; a
/// multi-block one is a thread.
/// </summary>
public sealed class ContentBlock : FoPostModel
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("media")]
    public IList<MediaItem> Media { get; set; } = new List<MediaItem>();

    [JsonPropertyName("position")]
    public int? Position { get; set; }
}

/// <summary>A one-time upload slot: PUT the bytes to <see cref="UploadUrl"/> before <see cref="ExpiresAt"/>.</summary>
public sealed class PresignedUpload : FoPostModel
{
    [JsonPropertyName("uploadId")]
    public string UploadId { get; set; } = string.Empty;

    [JsonPropertyName("uploadUrl")]
    public string UploadUrl { get; set; } = string.Empty;

    [JsonPropertyName("method")]
    public string Method { get; set; } = "PUT";

    /// <summary>Headers the PUT must carry, exactly as returned.</summary>
    [JsonPropertyName("headers")]
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    [JsonPropertyName("expiresAt")]
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>The largest file this account may upload, in bytes.</summary>
    [JsonPropertyName("maxFileBytes")]
    public long? MaxFileBytes { get; set; }
}

/// <summary>What the account may upload, from <c>GET /v1/media/limits</c>.</summary>
public sealed class UploadLimits : FoPostModel
{
    /// <summary>The largest file allowed by any route, in bytes.</summary>
    [JsonPropertyName("maxFileBytes")]
    public long MaxFileBytes { get; set; }

    /// <summary>What set the ceiling: <c>accounts</c>, <c>add_on</c>, or <c>bypass</c>.</summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("connectedAccounts")]
    public int ConnectedAccounts { get; set; }

    [JsonPropertyName("largeUploadsEnabled")]
    public bool LargeUploadsEnabled { get; set; }

    /// <summary>The next ceiling more connected accounts would unlock; null at the top.</summary>
    [JsonPropertyName("nextTier")]
    public UploadLimitTier? NextTier { get; set; }

    /// <summary>The most a multipart request can carry per file; larger files use direct upload.</summary>
    [JsonPropertyName("multipartMaxFileBytes")]
    public long MultipartMaxFileBytes { get; set; }

    [JsonPropertyName("maxFilesPerUpload")]
    public int MaxFilesPerUpload { get; set; }

    [JsonPropertyName("directUploadAvailable")]
    public bool DirectUploadAvailable { get; set; }

    [JsonPropertyName("allowedTypes")]
    public IList<string> AllowedTypes { get; set; } = new List<string>();
}

/// <summary>One step of the connected-account ladder in <see cref="UploadLimits.NextTier"/>.</summary>
public sealed class UploadLimitTier : FoPostModel
{
    /// <summary>The ceiling applies up to this many connected accounts; null means unbounded.</summary>
    [JsonPropertyName("upToAccounts")]
    public int? UpToAccounts { get; set; }

    [JsonPropertyName("maxFileBytes")]
    public long MaxFileBytes { get; set; }
}

/// <summary>A file in the workspace media library.</summary>
public sealed class MediaLibraryItem : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>One of <c>image</c>, <c>video</c>, <c>gif</c>, or <c>document</c>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "image";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("previewUrl")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }
}
