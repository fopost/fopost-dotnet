namespace FoPost;

/// <summary>The body of <see cref="Resources.AiResource.GenerateCaptionAsync"/>.</summary>
public sealed class GenerateCaptionOptions
{
    /// <summary>What the user has written so far, if anything.</summary>
    public string? CurrentCaption { get; set; }

    public IList<string>? ImageUrls { get; set; }

    /// <summary>Target platforms, from <see cref="Platforms"/>.</summary>
    public IList<string>? Platforms { get; set; }

    public int? CharLimit { get; set; }

    public string? WorkspaceId { get; set; }

    /// <summary>Write in a saved brand's voice.</summary>
    public string? BrandVoiceId { get; set; }
}

/// <summary>The body of <see cref="Resources.AiResource.RewriteAsync"/>.</summary>
public sealed class RewriteOptions
{
    public string Content { get; set; } = string.Empty;

    /// <summary>One rewrite per platform, from <see cref="Platforms"/>.</summary>
    public IList<string> Platforms { get; set; } = new List<string>();

    public string? Tone { get; set; }

    public string? WorkspaceId { get; set; }

    public string? BrandVoiceId { get; set; }
}

/// <summary>The body of <see cref="Resources.AiResource.RepurposeUrlAsync"/>.</summary>
public sealed class RepurposeUrlOptions
{
    /// <summary>The article to fan out.</summary>
    public string Url { get; set; } = string.Empty;

    public IList<string> Platforms { get; set; } = new List<string>();

    public string? WorkspaceId { get; set; }

    public string? BrandVoiceId { get; set; }
}
