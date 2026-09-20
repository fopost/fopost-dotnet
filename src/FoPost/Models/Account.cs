using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>
/// A connected social account. Named <c>SocialAccount</c> so it does not read
/// as a FoPost user account.
/// </summary>
public sealed class SocialAccount : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }

    /// <summary>See <see cref="Platforms"/> for the networks the API publishes to.</summary>
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>The display name when one is set, else the platform name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>The name from the platform, whatever the display name.</summary>
    [JsonPropertyName("platform_name")]
    public string? PlatformName { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("is_primary")]
    public bool? IsPrimary { get; set; }

    [JsonPropertyName("health_status")]
    public string? HealthStatus { get; set; }

    [JsonPropertyName("last_health_check")]
    public DateTimeOffset? LastHealthCheck { get; set; }
}

/// <summary>Token validity and last-check detail for one connected account.</summary>
public sealed class AccountHealth : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("health_status")]
    public string? HealthStatus { get; set; }

    [JsonPropertyName("last_health_check")]
    public DateTimeOffset? LastHealthCheck { get; set; }
}

/// <summary>A workspace: the tenant every post, account, and label belongs to.</summary>
public sealed class Workspace : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string? Slug { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("accounts")]
    public IList<SocialAccount> Accounts { get; set; } = new List<SocialAccount>();
}

/// <summary>A label you can attach to posts to group and filter them.</summary>
public sealed class Label : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("workspace")]
    public JsonElement? Workspace { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>A named set of accounts in one workspace, for posting to all of them at once.</summary>
public sealed class AccountGroup : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("account_ids")]
    public IList<string> AccountIds { get; set; } = new List<string>();

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// A one-time code, valid for 15 minutes. Sending <see cref="Command"/> to the bot in a Telegram
/// chat connects that chat.
/// </summary>
public sealed class TelegramConnectCode : FoPostModel
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("bot_username")]
    public string? BotUsername { get; set; }

    [JsonPropertyName("deep_link")]
    public string? DeepLink { get; set; }

    [JsonPropertyName("group_link")]
    public string? GroupLink { get; set; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; set; }
}

/// <summary>
/// Where a connect code stands: <c>pending</c>, <c>connected</c> (with <see cref="AccountId"/>),
/// <c>failed</c> (with <see cref="Reason"/>) or <c>expired</c>.
/// </summary>
public sealed class TelegramConnectStatus : FoPostModel
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}

/// <summary>One entry in a Telegram bot's command menu.</summary>
public sealed class TelegramBotCommand : FoPostModel
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>The command menu the bot shows in a connected Telegram chat.</summary>
public sealed class TelegramBotCommands : FoPostModel
{
    [JsonPropertyName("commands")]
    public IList<TelegramBotCommand> Commands { get; set; } = new List<TelegramBotCommand>();
}

/// <summary>A channel the Slack app can post to; <see cref="IsCurrent"/> marks the one this account posts to.</summary>
public sealed class SlackChannel : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>Whether the bot is in the channel.</summary>
    [JsonPropertyName("is_member")]
    public bool IsMember { get; set; }

    [JsonPropertyName("is_current")]
    public bool IsCurrent { get; set; }
}

/// <summary>A person in the connected Slack workspace; pass <see cref="Id"/> as the handle to start a DM.</summary>
public sealed class SlackMember : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("real_name")]
    public string? RealName { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }
}

/// <summary>The name and icon a Slack account posts under; each is null when unset.</summary>
public sealed class SlackIdentity : FoPostModel
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    [JsonPropertyName("icon_emoji")]
    public string? IconEmoji { get; set; }
}

// ─── Discord (bot connections) ───────────────────────────────────────────

/// <summary>A Discord text channel the bot can post to; <see cref="IsCurrent"/> marks this account's.</summary>
public sealed class DiscordChannel : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Discord's channel type: 0 text, 5 announcement, 15 forum.</summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }

    [JsonPropertyName("nsfw")]
    public bool Nsfw { get; set; }

    [JsonPropertyName("is_current")]
    public bool IsCurrent { get; set; }
}

/// <summary>The nickname and avatar the bot wears in the server; null means its own.</summary>
public sealed class DiscordIdentity : FoPostModel
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}

/// <summary>A message in the connected channel.</summary>
public sealed class DiscordMessage : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("author_id")]
    public string AuthorId { get; set; } = string.Empty;

    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; } = string.Empty;

    [JsonPropertyName("pinned")]
    public bool Pinned { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }
}

/// <summary>A message the bot put somewhere.</summary>
public sealed class DiscordMessageRef : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;
}

/// <summary>A thread started on a message.</summary>
public sealed class DiscordThread : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("parent_id")]
    public string? ParentId { get; set; }
}

/// <summary>
/// An event on the server's calendar. <see cref="ChannelId"/> names a voice or stage channel;
/// otherwise <see cref="Location"/> says where it happens.
/// </summary>
public sealed class DiscordScheduledEvent : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("start_time")]
    public string StartTime { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    public string? EndTime { get; set; }

    /// <summary>scheduled, active, completed or canceled.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "scheduled";

    [JsonPropertyName("user_count")]
    public int? UserCount { get; set; }
}

/// <summary>A person in the connected server; <see cref="Id"/> is the member id for a DM or a role.</summary>
public sealed class DiscordMember : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>Nickname in this server.</summary>
    [JsonPropertyName("nick")]
    public string? Nick { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("roles")]
    public IList<string> Roles { get; set; } = new List<string>();

    [JsonPropertyName("joined_at")]
    public string? JoinedAt { get; set; }
}

/// <summary>
/// A role in the connected server. A managed role belongs to an integration and cannot be edited;
/// <see cref="Permissions"/> is Discord's bitfield as a decimal string.
/// </summary>
public sealed class DiscordRole : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public int Color { get; set; }

    [JsonPropertyName("hoist")]
    public bool Hoist { get; set; }

    [JsonPropertyName("mentionable")]
    public bool Mentionable { get; set; }

    [JsonPropertyName("managed")]
    public bool Managed { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("permissions")]
    public string Permissions { get; set; } = "0";
}

/// <summary>What a Discord delete, pin or role assignment answers.</summary>
public sealed class DiscordAck : FoPostModel
{
    [JsonPropertyName("deleted")]
    public bool? Deleted { get; set; }

    [JsonPropertyName("pinned")]
    public bool? Pinned { get; set; }

    [JsonPropertyName("assigned")]
    public bool? Assigned { get; set; }
}
