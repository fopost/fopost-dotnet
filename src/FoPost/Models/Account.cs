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

    /// <summary>
    /// True when the account was connected before a permission it now needs was asked for.
    /// Reconnecting it is the fix; nothing else changes.
    /// </summary>
    [JsonPropertyName("reconnect_required")]
    public bool? ReconnectRequired { get; set; }
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

/// <summary>A subreddit a Reddit account is in, or its own profile page.</summary>
public sealed class RedditSubreddit : FoPostModel
{
    /// <summary>The name, without the <c>r/</c> prefix.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("subscribers")]
    public long? Subscribers { get; set; }

    [JsonPropertyName("over18")]
    public bool Over18 { get; set; }

    /// <summary>False where the account may read but not submit.</summary>
    [JsonPropertyName("can_post")]
    public bool CanPost { get; set; }

    /// <summary>Whether the subreddit offers post flairs at all.</summary>
    [JsonPropertyName("flair_enabled")]
    public bool FlairEnabled { get; set; }

    [JsonPropertyName("icon_url")]
    public string? IconUrl { get; set; }

    /// <summary>Where posts go when a post names no subreddit.</summary>
    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }
}

/// <summary>One rule a subreddit publishes. <see cref="AppliesTo"/> is link, comment or all.</summary>
public sealed class RedditSubredditRule : FoPostModel
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("applies_to")]
    public string? AppliesTo { get; set; }
}

/// <summary>A subreddit's rules, in its own order. Show them before publishing.</summary>
public sealed class RedditSubredditRules : FoPostModel
{
    [JsonPropertyName("subreddit")]
    public string Subreddit { get; set; } = string.Empty;

    [JsonPropertyName("rules")]
    public IList<RedditSubredditRule> Rules { get; set; } = new List<RedditSubredditRule>();
}

/// <summary>A post flair, valid only in the subreddit it came from.</summary>
public sealed class RedditFlair : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Whether the label may be replaced with your own text.</summary>
    [JsonPropertyName("editable")]
    public bool Editable { get; set; }
}

/// <summary>The post flairs one subreddit offers.</summary>
public sealed class RedditFlairs : FoPostModel
{
    [JsonPropertyName("subreddit")]
    public string Subreddit { get; set; } = string.Empty;

    [JsonPropertyName("flairs")]
    public IList<RedditFlair> Flairs { get; set; } = new List<RedditFlair>();
}

/// <summary>
/// Where posts from a Reddit account go when a post names no subreddit. Null means the
/// account's own profile page, which always accepts a post.
/// </summary>
public sealed class RedditDefaultSubreddit : FoPostModel
{
    [JsonPropertyName("subreddit")]
    public string? Subreddit { get; set; }
}
