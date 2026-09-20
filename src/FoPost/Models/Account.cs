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

/// <summary>A tappable prompt Messenger or Instagram shows before the first message.</summary>
public sealed class MetaIceBreaker : FoPostModel
{
    /// <summary>Up to 80 characters.</summary>
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    /// <summary>What your webhook receives when the prompt is tapped.</summary>
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;
}

/// <summary>The ice breakers set on one account.</summary>
public sealed class MetaIceBreakers : FoPostModel
{
    [JsonPropertyName("ice_breakers")]
    public IList<MetaIceBreaker> IceBreakers { get; set; } = new List<MetaIceBreaker>();
}

/// <summary>
/// A persistent-menu item: a <c>postback</c> carrying <see cref="Payload"/>, or a
/// <c>web_url</c> carrying an http(s) <see cref="Url"/>. The unused one stays null.
/// </summary>
public sealed class MetaMenuItem : FoPostModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Payload { get; set; }

    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }

    /// <summary>An item that sends <paramref name="payload"/> to your webhook when tapped.</summary>
    public static MetaMenuItem Postback(string title, string payload) =>
        new() { Type = "postback", Title = title, Payload = payload };

    /// <summary>An item that opens <paramref name="url"/>.</summary>
    public static MetaMenuItem Link(string title, string url) =>
        new() { Type = "web_url", Title = title, Url = url };
}

/// <summary>One locale's menu; <c>default</c> is the fallback every language uses.</summary>
public sealed class MetaPersistentMenuEntry : FoPostModel
{
    [JsonPropertyName("locale")]
    public string Locale { get; set; } = "default";

    [JsonPropertyName("call_to_actions")]
    public IList<MetaMenuItem> CallToActions { get; set; } = new List<MetaMenuItem>();

    [JsonPropertyName("composer_input_disabled")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ComposerInputDisabled { get; set; }

    /// <summary>The default-locale menu, the one every language falls back to.</summary>
    public static MetaPersistentMenuEntry DefaultLocale(IEnumerable<MetaMenuItem> items) =>
        new() { Locale = "default", CallToActions = items.ToList() };
}

/// <summary>The persistent menu set on one account, one entry per locale.</summary>
public sealed class MetaPersistentMenu : FoPostModel
{
    [JsonPropertyName("persistent_menu")]
    public IList<MetaPersistentMenuEntry> PersistentMenu { get; set; } = new List<MetaPersistentMenuEntry>();
}

/// <summary>One locale's greeting, up to 160 characters.</summary>
public sealed class MetaGreetingText : FoPostModel
{
    [JsonPropertyName("locale")]
    public string Locale { get; set; } = "default";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>The default-locale greeting.</summary>
    public static MetaGreetingText Of(string text) => new() { Text = text };
}

/// <summary>The greeting set on one account, one entry per locale.</summary>
public sealed class MetaGreeting : FoPostModel
{
    [JsonPropertyName("greeting")]
    public IList<MetaGreetingText> Greeting { get; set; } = new List<MetaGreetingText>();
}

/// <summary>
/// What the network delivers to the FoPost webhook for one account. <see cref="Subscribed"/>
/// is false when the subscription lapsed or a required field is missing.
/// </summary>
public sealed class WebhookSubscription : FoPostModel
{
    [JsonPropertyName("subscribed")]
    public bool Subscribed { get; set; }

    [JsonPropertyName("fields")]
    public IList<string> Fields { get; set; } = new List<string>();

    [JsonPropertyName("missing_fields")]
    public IList<string> MissingFields { get; set; } = new List<string>();
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
