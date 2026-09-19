namespace FoPost;

/// <summary>
/// The body of <see cref="Resources.AccountsResource.UpdateSlackIdentityAsync"/>. An unset field keeps
/// its value and null clears it. Set <see cref="IconUrl"/> or <see cref="IconEmoji"/>, not both;
/// setting one clears the other.
/// </summary>
public sealed class UpdateSlackIdentityOptions
{
    /// <summary>1-80 characters, or null for the app name.</summary>
    public Optional<string?> Username { get; set; }

    /// <summary>An http(s) image URL, or null to clear it.</summary>
    public Optional<string?> IconUrl { get; set; }

    /// <summary>An emoji code such as <c>:rocket:</c>, or null to clear it.</summary>
    public Optional<string?> IconEmoji { get; set; }
}
