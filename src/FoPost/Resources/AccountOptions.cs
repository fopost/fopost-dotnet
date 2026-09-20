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

/// <summary>
/// The body of <see cref="Resources.AccountsResource.UpdateDiscordIdentityAsync"/>. An unset field
/// keeps its value and null clears it.
/// </summary>
public sealed class UpdateDiscordIdentityOptions
{
    /// <summary>1-32 characters, or null for the application's own name.</summary>
    public Optional<string?> Username { get; set; }

    /// <summary>An http(s) image URL, or null to clear it.</summary>
    public Optional<string?> AvatarUrl { get; set; }
}

/// <summary>
/// The body of the Discord scheduled-event create and update calls. Give <see cref="ChannelId"/>
/// for an event in a voice or stage channel, or <see cref="Location"/> with an
/// <see cref="EndTime"/> for one elsewhere. On an update, an unset field is left as it is.
/// </summary>
public sealed class DiscordEventOptions
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    /// <summary>RFC 3339.</summary>
    public string? StartTime { get; set; }

    /// <summary>RFC 3339; required for an event at a location.</summary>
    public string? EndTime { get; set; }

    public string? ChannelId { get; set; }

    public string? Location { get; set; }

    /// <summary>scheduled, active, completed or canceled; only meaningful on an update.</summary>
    public string? Status { get; set; }
}

/// <summary>The body of the Discord role create and update calls.</summary>
public sealed class DiscordRoleOptions
{
    public string? Name { get; set; }

    /// <summary>An RGB integer, e.g. 5793266.</summary>
    public int? Color { get; set; }

    /// <summary>Show members with this role separately in the member list.</summary>
    public bool? Hoist { get; set; }

    public bool? Mentionable { get; set; }

    /// <summary>Discord's permission bitfield as a decimal string.</summary>
    public string? Permissions { get; set; }
}
