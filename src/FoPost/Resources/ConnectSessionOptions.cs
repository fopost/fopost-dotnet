namespace FoPost;

/// <summary>The body of <see cref="Resources.ConnectSessionsResource.CreateAsync"/>.</summary>
public sealed class CreateConnectSessionOptions
{
    public string? WorkspaceId { get; set; }

    /// <summary>Your own id for the user who will open the link.</summary>
    public required string ExternalUserId { get; set; }

    /// <summary>The networks the page offers; see <see cref="FoPost.Platforms"/>.</summary>
    public required IList<string> Platforms { get; set; }

    /// <summary>Where to send the user afterwards. Its domain must be on the workspace's connect redirect domains.</summary>
    public string? RedirectUrl { get; set; }

    public string? BrandingName { get; set; }

    public string? BrandingLogoUrl { get; set; }
}
