namespace FoPost;

/// <summary>Attestation that the person in a <c>person</c> brand asset agreed to its use.</summary>
public sealed class PersonConsent
{
    public PersonConsent(string subject, string statement)
    {
        Subject = subject;
        Statement = statement;
    }

    public string Subject { get; }

    public string Statement { get; }
}

/// <summary>The body of <see cref="Resources.MediaResource.PresignAsync(PresignOptions, CancellationToken)"/>.</summary>
public sealed class PresignOptions
{
    public string WorkspaceId { get; set; } = string.Empty;

    public string Filename { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    /// <summary>Exact byte length of the file; the upload must match it.</summary>
    public long Size { get; set; }

    /// <summary>Library folder to file the upload under.</summary>
    public string? FolderId { get; set; }

    /// <summary>Where the file came from, e.g. <c>chat</c>.</summary>
    public string? Origin { get; set; }

    /// <summary>One of <c>logo</c>, <c>product</c>, <c>person</c>, or <c>background</c>.</summary>
    public string? BrandRole { get; set; }

    public string? BrandVoiceId { get; set; }

    /// <summary>Required when <see cref="BrandRole"/> is <c>person</c>.</summary>
    public PersonConsent? PersonConsent { get; set; }
}
