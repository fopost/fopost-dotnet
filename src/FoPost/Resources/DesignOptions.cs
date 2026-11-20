namespace FoPost;

/// <summary>Filters for <see cref="Resources.DesignsResource.ListAsync"/>.</summary>
public sealed class ListDesignsOptions
{
    /// <summary>One of <see cref="DesignKinds"/>.</summary>
    public string? Kind { get; set; }

    public int? Limit { get; set; }

    public int? Offset { get; set; }
}

/// <summary>The body of <see cref="Resources.DesignsResource.CreateAsync"/>.</summary>
public sealed class CreateDesignOptions
{
    public required string WorkspaceId { get; set; }

    /// <summary>One of <see cref="DesignKinds"/>.</summary>
    public required string Kind { get; set; }

    /// <summary>Defaults to "Untitled Design".</summary>
    public string? Name { get; set; }

    /// <summary>
    /// The pages and layers, sent as given: a dictionary, a <c>JsonNode</c>, or the
    /// <see cref="Design.Document"/> read back from another design. Omitted, the kind's blank canvas.
    /// </summary>
    public object? Document { get; set; }

    /// <summary>Applies this brand's colours and fonts; omitted, the workspace default.</summary>
    public string? BrandVoiceId { get; set; }
}

/// <summary>The body of <see cref="Resources.DesignsResource.UpdateAsync"/>. Only the fields you set are sent.</summary>
public sealed class UpdateDesignOptions
{
    public string? Name { get; set; }

    /// <summary>One of <see cref="DesignKinds"/>.</summary>
    public string? Kind { get; set; }

    /// <summary>The new pages and layers, sent as given.</summary>
    public object? Document { get; set; }

    /// <summary>Save the new document as a version the design can be restored to.</summary>
    public bool? CreateVersion { get; set; }
}

/// <summary>The body of <see cref="Resources.DesignsResource.ExportAsync"/>.</summary>
public sealed class ExportDesignOptions
{
    /// <summary>One of <see cref="DesignExportFormats"/>.</summary>
    public required string Format { get; set; }

    /// <summary>Zero-based pages to render; omitted, every page.</summary>
    public IList<int>? PageIndexes { get; set; }

    /// <summary>Seconds for an <c>mp4</c> or <c>gif</c>: 5, 10 or 15.</summary>
    public int? DurationSec { get; set; }
}

/// <summary>The body of <see cref="Resources.DesignsResource.TextToSlidesAsync"/>. Pass <see cref="Text"/> or <see cref="PostId"/>.</summary>
public sealed class TextToSlidesOptions
{
    public required string WorkspaceId { get; set; }

    public string? Text { get; set; }

    /// <summary>Split this post's text instead of <see cref="Text"/>.</summary>
    public string? PostId { get; set; }

    /// <summary>Between 2 and 18.</summary>
    public int? MaxSlides { get; set; }

    public string? BrandVoiceId { get; set; }
}

/// <summary>The body of <see cref="Resources.DesignsResource.FromPostAsync"/>. Pass <see cref="Text"/> or <see cref="PostId"/>.</summary>
public sealed class DesignFromPostOptions
{
    public required string WorkspaceId { get; set; }

    public string? Text { get; set; }

    public string? PostId { get; set; }

    /// <summary>One of <see cref="DesignFromPostStyles"/>; defaults to <c>quote</c>.</summary>
    public string? Style { get; set; }

    public string? Preset { get; set; }

    /// <summary>The connected account whose name and avatar a <c>post</c> style shows.</summary>
    public string? AccountId { get; set; }

    /// <summary>Split long text across carousel pages.</summary>
    public bool? Carousel { get; set; }

    public string? Name { get; set; }

    public string? BrandVoiceId { get; set; }
}
