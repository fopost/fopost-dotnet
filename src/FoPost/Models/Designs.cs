using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>What a design is made for; each kind has its own canvas size.</summary>
public static class DesignKinds
{
    public const string Story = "story";
    public const string Post = "post";
    public const string Carousel = "carousel";
    public const string Thumbnail = "thumbnail";
    public const string Card = "card";
    public const string Collage = "collage";
}

/// <summary>What <see cref="Resources.DesignsResource.ExportAsync"/> renders.</summary>
public static class DesignExportFormats
{
    public const string Png = "png";
    public const string Jpg = "jpg";
    public const string Pdf = "pdf";
    public const string Mp4 = "mp4";
    public const string Gif = "gif";
}

/// <summary>The layouts <see cref="Resources.DesignsResource.FromPostAsync"/> builds.</summary>
public static class DesignFromPostStyles
{
    public const string Quote = "quote";
    public const string Post = "post";
    public const string Plain = "plain";
}

/// <summary>A design: its canvas and its document of pages and layers.</summary>
public sealed class Design : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="DesignKinds"/>.</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    /// <summary>The pages and layers, as sent. Absent from list results.</summary>
    [JsonPropertyName("document")]
    public JsonElement? Document { get; set; }

    [JsonPropertyName("thumbnail_media_id")]
    public string? ThumbnailMediaId { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>A saved version of a design's document.</summary>
public sealed class DesignVersion : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>Present only when the versions were listed with the document included.</summary>
    [JsonPropertyName("document")]
    public JsonElement? Document { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}

/// <summary>A design after an update or a restore, and the version that saved, if any.</summary>
public sealed class DesignChange
{
    public DesignChange(Design design, DesignVersion? version)
    {
        Design = design;
        Version = version;
    }

    public Design Design { get; }

    public DesignVersion? Version { get; }
}

/// <summary>The answer to a design delete.</summary>
public sealed class DesignDeletion : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }
}

/// <summary>The files an export rendered into the media library.</summary>
public sealed class DesignExport : FoPostModel
{
    /// <summary>One of <see cref="DesignExportFormats"/>.</summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("files")]
    public IList<DesignExportFile> Files { get; set; } = new List<DesignExportFile>();
}

/// <summary>One rendered file: a media library item.</summary>
public sealed class DesignExportFile : FoPostModel
{
    [JsonPropertyName("media_id")]
    public string MediaId { get; set; } = string.Empty;

    [JsonPropertyName("page_index")]
    public int? PageIndex { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }
}

/// <summary>A post that used a design's exported media, with its metrics.</summary>
public sealed class DesignPost : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("schedule_at")]
    public DateTimeOffset? ScheduleAt { get; set; }

    [JsonPropertyName("published_at")]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonPropertyName("platforms")]
    public IList<string> Platforms { get; set; } = new List<string>();

    [JsonPropertyName("engagement")]
    public long? Engagement { get; set; }

    [JsonPropertyName("views")]
    public long? Views { get; set; }
}

/// <summary>Carousel copy split from long text: a cover, the slides, and a closing call to action.</summary>
public sealed class DesignSlides : FoPostModel
{
    [JsonPropertyName("cover")]
    public SlideCover? Cover { get; set; }

    [JsonPropertyName("slides")]
    public IList<SlidePoint> Slides { get; set; } = new List<SlidePoint>();

    [JsonPropertyName("cta")]
    public SlidePoint? Cta { get; set; }

    [JsonPropertyName("brand")]
    public SlidesBrand? Brand { get; set; }
}

public sealed class SlideCover : FoPostModel
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("subtitle")]
    public string Subtitle { get; set; } = string.Empty;
}

public sealed class SlidePoint : FoPostModel
{
    [JsonPropertyName("heading")]
    public string Heading { get; set; } = string.Empty;

    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
}

/// <summary>The brand styling the slides were written for.</summary>
public sealed class SlidesBrand : FoPostModel
{
    [JsonPropertyName("colors")]
    public IList<string> Colors { get; set; } = new List<string>();

    [JsonPropertyName("fonts")]
    public SlidesBrandFonts? Fonts { get; set; }

    [JsonPropertyName("logo_media_id")]
    public string? LogoMediaId { get; set; }
}

public sealed class SlidesBrandFonts : FoPostModel
{
    [JsonPropertyName("heading")]
    public string? Heading { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }
}
