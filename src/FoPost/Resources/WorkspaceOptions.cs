namespace FoPost;

/// <summary>The body of <see cref="Resources.WorkspacesResource.CreateAsync"/>.</summary>
public sealed class CreateWorkspaceOptions
{
    public required string Name { get; set; }

    /// <summary><c>PERSONAL</c>, <c>TEAM</c>, <c>ORGANIZATION</c>, <c>CLIENT</c>, <c>PROJECT</c>, <c>DEPARTMENT</c>, <c>EVENT</c>, <c>TEMPORARY</c>, <c>COMMUNITY</c>, <c>BRAND</c> or <c>AGENCY</c>.</summary>
    public string? Type { get; set; }

    public string? Logo { get; set; }

    public string? Website { get; set; }

    public string? Timezone { get; set; }

    public string? Country { get; set; }

    public string? Description { get; set; }

    public string? Language { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.WorkspacesResource.UpdateAsync"/>. Only the fields you set
/// are sent.
/// </summary>
public sealed class UpdateWorkspaceOptions
{
    public Optional<string> Name { get; set; }

    public Optional<string> Type { get; set; }

    public Optional<string?> Logo { get; set; }

    public Optional<string?> Website { get; set; }

    public Optional<string> Timezone { get; set; }

    public Optional<string?> Country { get; set; }

    public Optional<string?> Description { get; set; }

    public Optional<string> Language { get; set; }

    public Optional<bool> RequireApproval { get; set; }

    public Optional<bool> AiAltTextEnabled { get; set; }

    public Optional<string?> BrandColor { get; set; }

    /// <summary>Domains a hosted connect link may redirect back to.</summary>
    public Optional<IList<string>> ConnectRedirectDomains { get; set; }
}
