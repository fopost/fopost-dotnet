namespace FoPost;

/// <summary>The body of <see cref="Resources.KnowledgeResource.CreateAsync"/>.</summary>
/// <remarks>
/// A <c>faq</c> or <c>text</c> source needs <see cref="Content"/>, a <c>url</c>
/// source needs <see cref="Url"/>, and a <c>file</c> source needs
/// <see cref="MediaId"/> pointing at a plain-text or CSV item in the same
/// workspace.
/// </remarks>
public sealed class CreateKnowledgeSourceOptions
{
    /// <summary>One of <see cref="KnowledgeSourceKinds"/>.</summary>
    public required string Kind { get; set; }

    public required string Title { get; set; }

    public string? Content { get; set; }

    public string? Url { get; set; }

    public string? MediaId { get; set; }

    /// <summary>Narrows the source to one brand; omitted, it serves the whole workspace.</summary>
    public string? BrandVoiceId { get; set; }

    public string? WorkspaceId { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.KnowledgeResource.UpdateAsync"/>. Only the
/// fields you set are sent, so it stays a partial update. Changing the content
/// or the URL returns the source to <c>pending</c> and re-indexes it.
/// </summary>
public sealed class UpdateKnowledgeSourceOptions
{
    public Optional<string> Title { get; set; }

    public Optional<string> Content { get; set; }

    public Optional<string> Url { get; set; }

    public Optional<string?> BrandVoiceId { get; set; }
}

/// <summary>Filters for <see cref="Resources.KnowledgeResource.SearchAsync"/>.</summary>
public sealed class SearchKnowledgeOptions
{
    /// <summary>How many passages to return. Defaults to 5, caps at 20.</summary>
    public int? TopK { get; set; }

    public string? BrandVoiceId { get; set; }

    public string? WorkspaceId { get; set; }
}
