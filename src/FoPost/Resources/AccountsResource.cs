using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Accounts</c> — the social accounts connected to a workspace.</summary>
public sealed class AccountsResource
{
    private readonly FoPostHttpClient _http;

    internal AccountsResource(FoPostHttpClient http) => _http = http;

    /// <summary>Connected accounts, across every workspace unless one is named.</summary>
    public Task<IReadOnlyList<SocialAccount>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListAsync(workspaceId, null, cancellationToken);

    /// <summary>
    /// Connected accounts, narrowed to one account group when <paramref name="groupId"/> is set.
    /// </summary>
    public async Task<IReadOnlyList<SocialAccount>> ListAsync(
        string? workspaceId,
        string? groupId,
        CancellationToken cancellationToken = default)
    {
        // This endpoint reads a camelCase query param; posts and labels use snake.
        var query = new Dictionary<string, object?> { ["workspaceId"] = workspaceId, ["group_id"] = groupId };
        var body = await _http.GetAsync("/v1/accounts", query, cancellationToken).ConfigureAwait(false);
        return ToList<SocialAccount>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<SocialAccount> GetAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"/v1/accounts/{Uri.EscapeDataString(accountId)}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<SocialAccount>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Set the name shown instead of the platform name. <c>null</c> or an empty string restores
    /// the platform name. The result carries <c>Id</c>, <c>Name</c> and <c>PlatformName</c>.
    /// </summary>
    public async Task<SocialAccount> RenameAsync(
        string accountId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["display_name"] = displayName };
        var response = await _http
            .RequestAsync(HttpMethod.Patch, AccountPath(accountId), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<SocialAccount>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Move the account to another workspace the caller owns. The result carries <c>Id</c> and
    /// <c>WorkspaceId</c>. A 409 with code <c>move_blocked</c> lists <c>blocking_tables</c> on
    /// <see cref="FoPostException.Body"/>; any other 409 means the target already has an account
    /// on that network.
    /// </summary>
    public async Task<SocialAccount> MoveAsync(
        string accountId,
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var response = await _http
            .PostAsync($"{AccountPath(accountId)}/move", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<SocialAccount>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Token validity and last-check detail for one account.</summary>
    public async Task<AccountHealth> HealthAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"/v1/accounts/{Uri.EscapeDataString(accountId)}/health", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AccountHealth>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// The numbers only this account's network reports, in its own vocabulary: ad-break
    /// earnings, story taps, a retention curve, the search terms behind a listing. Keyed by the
    /// platform's own metric names, read from the newest collected snapshot rather than fetched
    /// live. Needs the <c>analytics</c> scope.
    /// </summary>
    /// <remarks>
    /// A network whose metric access has not been granted yet answers 503
    /// (<c>platform_metrics_unavailable</c>) rather than an empty set.
    /// </remarks>
    public async Task<AccountPlatformMetrics> PlatformMetricsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["raw"] = "true" };
        var body = await _http
            .GetAsync($"{AccountPath(accountId)}/insights", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<AccountPlatformMetrics>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Mint a one-time code, valid for 15 minutes. Sending <c>/connect &lt;code&gt;</c> to the bot in
    /// a Telegram chat connects that chat. <paramref name="workspaceId"/> may be omitted for a key
    /// bound to one workspace.
    /// </summary>
    public async Task<TelegramConnectCode> CreateTelegramConnectCodeAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();
        if (workspaceId is not null)
        {
            body["workspaceId"] = workspaceId;
        }

        var response = await _http
            .PostAsync("/v1/accounts/telegram/connect-code", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<TelegramConnectCode>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Whether a connect code has been used yet, and the account it connected.</summary>
    public async Task<TelegramConnectStatus> GetTelegramConnectStatusAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["code"] = code };
        var response = await _http
            .GetAsync("/v1/accounts/telegram/connect-code/status", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<TelegramConnectStatus>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The command menu the bot shows in this Telegram chat.</summary>
    public async Task<TelegramBotCommands> GetTelegramBotCommandsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/telegram/commands", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<TelegramBotCommands>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Replace the command menu for this Telegram chat, 1-100 commands.</summary>
    public async Task<TelegramBotCommands> SetTelegramBotCommandsAsync(
        string accountId,
        IEnumerable<TelegramBotCommand> commands,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["commands"] = commands
                .Select(c => new Dictionary<string, object?> { ["command"] = c.Command, ["description"] = c.Description })
                .ToList(),
        };
        var response = await _http
            .PutAsync($"{AccountPath(accountId)}/telegram/commands", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<TelegramBotCommands>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Clear the command menu for this Telegram chat.</summary>
    public async Task<TelegramBotCommands> DeleteTelegramBotCommandsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .DeleteAsync($"{AccountPath(accountId)}/telegram/commands", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<TelegramBotCommands>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Channels the Slack app can post to: every public channel, and private ones it was invited to.
    /// A 409 <c>webhook_connection</c> means the account posts through a webhook; reconnect it with
    /// the Slack app. The same applies to the other Slack calls.
    /// </summary>
    public async Task<IReadOnlyList<SlackChannel>> ListSlackChannelsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/slack/channels", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<SlackChannel>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>People in the connected Slack workspace, for addressing a DM.</summary>
    public async Task<IReadOnlyList<SlackMember>> ListSlackMembersAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/slack/members", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<SlackMember>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The name and icon this Slack account posts under.</summary>
    public async Task<SlackIdentity> GetSlackIdentityAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/slack/identity", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<SlackIdentity>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Change the name or icon this Slack account posts under. Only the fields you set are sent.</summary>
    public async Task<SlackIdentity> UpdateSlackIdentityAsync(
        string accountId,
        UpdateSlackIdentityOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Username.IsSet)
        {
            body["username"] = options.Username.Value;
        }
        if (options.IconUrl.IsSet)
        {
            body["icon_url"] = options.IconUrl.Value;
        }
        if (options.IconEmoji.IsSet)
        {
            body["icon_emoji"] = options.IconEmoji.Value;
        }

        var response = await _http
            .RequestAsync(HttpMethod.Patch, $"{AccountPath(accountId)}/slack/identity", body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<SlackIdentity>(FoPostHttpClient.Unwrap(response));
    }

    // ─── Meta messaging settings (Facebook Pages, Instagram) ─────────

    /// <summary>The prompts shown before the first message. A network without them answers 400.</summary>
    public async Task<MetaIceBreakers> GetIceBreakersAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/messaging/ice-breakers", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaIceBreakers>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Replace the ice breakers, up to four.</summary>
    public async Task<MetaIceBreakers> SetIceBreakersAsync(
        string accountId,
        IEnumerable<MetaIceBreaker> iceBreakers,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["ice_breakers"] = iceBreakers.ToList() };
        var response = await _http
            .PutAsync($"{AccountPath(accountId)}/messaging/ice-breakers", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaIceBreakers>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Clear the ice breakers.</summary>
    public async Task<MetaIceBreakers> DeleteIceBreakersAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .DeleteAsync($"{AccountPath(accountId)}/messaging/ice-breakers", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaIceBreakers>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The always-visible Messenger menu. Facebook Pages only; other networks answer 400.</summary>
    public async Task<MetaPersistentMenu> GetPersistentMenuAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/messaging/persistent-menu", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaPersistentMenu>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Replace the menu, one entry per locale, up to three items each.</summary>
    public async Task<MetaPersistentMenu> SetPersistentMenuAsync(
        string accountId,
        IEnumerable<MetaPersistentMenuEntry> menu,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["persistent_menu"] = menu.ToList() };
        var response = await _http
            .PutAsync($"{AccountPath(accountId)}/messaging/persistent-menu", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaPersistentMenu>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Clear the menu.</summary>
    public async Task<MetaPersistentMenu> DeletePersistentMenuAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .DeleteAsync($"{AccountPath(accountId)}/messaging/persistent-menu", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaPersistentMenu>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The text shown before a Messenger conversation starts. Facebook Pages only.</summary>
    public async Task<MetaGreeting> GetGreetingAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/messaging/greeting", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaGreeting>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Replace the greeting, one entry per locale, each up to 160 characters.</summary>
    public async Task<MetaGreeting> SetGreetingAsync(
        string accountId,
        IEnumerable<MetaGreetingText> greeting,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["greeting"] = greeting.ToList() };
        var response = await _http
            .PutAsync($"{AccountPath(accountId)}/messaging/greeting", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaGreeting>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Clear the greeting.</summary>
    public async Task<MetaGreeting> DeleteGreetingAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .DeleteAsync($"{AccountPath(accountId)}/messaging/greeting", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<MetaGreeting>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>What the network is delivering to the FoPost webhook for this account.</summary>
    public async Task<WebhookSubscription> GetWebhookSubscriptionAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/webhook-subscription", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WebhookSubscription>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Subscribe to every field this account needs, lapsed or not.</summary>
    public async Task<WebhookSubscription> ResubscribeWebhookAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"{AccountPath(accountId)}/webhook-subscription", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WebhookSubscription>(FoPostHttpClient.Unwrap(response));
    }

    // ── Discord (bot connections) ────────────────────────────────────────

    /// <summary>
    /// Text channels the bot can post to in the connected server. A 409 <c>webhook_connection</c>
    /// means the account posts through a webhook; upgrade it to the bot first. The same applies to
    /// every other Discord call here.
    /// </summary>
    public async Task<IReadOnlyList<DiscordChannel>> ListDiscordChannelsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync(DiscordPath(accountId, "/channels"), null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<DiscordChannel>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Move the account to another channel in the same server.</summary>
    public async Task<DiscordChannel> SwitchDiscordChannelAsync(
        string accountId,
        string channelId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["channel_id"] = channelId };
        var response = await _http
            .RequestAsync(HttpMethod.Patch, DiscordPath(accountId, "/channels/current"), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordChannel>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The nickname and avatar the bot wears in the server.</summary>
    public async Task<DiscordIdentity> GetDiscordIdentityAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync(DiscordPath(accountId, "/identity"), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordIdentity>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Change the nickname or avatar the bot wears. Only the fields you set are sent.</summary>
    public async Task<DiscordIdentity> UpdateDiscordIdentityAsync(
        string accountId,
        UpdateDiscordIdentityOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Username.IsSet)
        {
            body["username"] = options.Username.Value;
        }
        if (options.AvatarUrl.IsSet)
        {
            body["avatar_url"] = options.AvatarUrl.Value;
        }

        var response = await _http
            .RequestAsync(HttpMethod.Patch, DiscordPath(accountId, "/identity"), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordIdentity>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Pinned messages in the account's channel.</summary>
    public async Task<IReadOnlyList<DiscordMessage>> ListDiscordPinsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync(DiscordPath(accountId, "/messages/pinned"), null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<DiscordMessage>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Remove a message from the account's channel.</summary>
    public async Task<DiscordAck> DeleteDiscordMessageAsync(
        string accountId,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .DeleteAsync(DiscordPath(accountId, $"/messages/{Uri.EscapeDataString(messageId)}"), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordAck>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Pin a message in the account's channel.</summary>
    public async Task<DiscordAck> PinDiscordMessageAsync(
        string accountId,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        var path = DiscordPath(accountId, $"/messages/{Uri.EscapeDataString(messageId)}/pin");
        var response = await _http.PostAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<DiscordAck>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Unpin a message in the account's channel.</summary>
    public async Task<DiscordAck> UnpinDiscordMessageAsync(
        string accountId,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        var path = DiscordPath(accountId, $"/messages/{Uri.EscapeDataString(messageId)}/pin");
        var response = await _http.DeleteAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<DiscordAck>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Publish an announcement-channel message to every server following the channel.</summary>
    public async Task<DiscordMessageRef> CrosspostDiscordMessageAsync(
        string accountId,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        var path = DiscordPath(accountId, $"/messages/{Uri.EscapeDataString(messageId)}/crosspost");
        var response = await _http.PostAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<DiscordMessageRef>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Start a thread on a message. <paramref name="autoArchiveDuration"/> is 60, 1440, 4320 or
    /// 10080 minutes, or null for the server's default.
    /// </summary>
    public async Task<DiscordThread> CreateDiscordThreadAsync(
        string accountId,
        string messageId,
        string name,
        int? autoArchiveDuration = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["name"] = name };
        if (autoArchiveDuration is not null)
        {
            body["auto_archive_duration"] = autoArchiveDuration;
        }

        var path = DiscordPath(accountId, $"/messages/{Uri.EscapeDataString(messageId)}/thread");
        var response = await _http.PostAsync(path, body, cancellationToken).ConfigureAwait(false);
        return Require<DiscordThread>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Send one message to a member of the server.</summary>
    public async Task<DiscordMessageRef> SendDiscordDmAsync(
        string accountId,
        string memberId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["member_id"] = memberId, ["content"] = content };
        var response = await _http
            .PostAsync(DiscordPath(accountId, "/dm"), body, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordMessageRef>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The server's scheduled events.</summary>
    public async Task<IReadOnlyList<DiscordScheduledEvent>> ListDiscordEventsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync(DiscordPath(accountId, "/events"), null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<DiscordScheduledEvent>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One scheduled event.</summary>
    public async Task<DiscordScheduledEvent> GetDiscordEventAsync(
        string accountId,
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var path = DiscordPath(accountId, $"/events/{Uri.EscapeDataString(eventId)}");
        var response = await _http.GetAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<DiscordScheduledEvent>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Add an event to the server's calendar.</summary>
    public async Task<DiscordScheduledEvent> CreateDiscordEventAsync(
        string accountId,
        DiscordEventOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .PostAsync(DiscordPath(accountId, "/events"), EventBody(options), cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordScheduledEvent>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Change a scheduled event; only the fields you set are sent.</summary>
    public async Task<DiscordScheduledEvent> UpdateDiscordEventAsync(
        string accountId,
        string eventId,
        DiscordEventOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var path = DiscordPath(accountId, $"/events/{Uri.EscapeDataString(eventId)}");
        var response = await _http
            .RequestAsync(HttpMethod.Patch, path, EventBody(options), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordScheduledEvent>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Remove a scheduled event.</summary>
    public async Task<DiscordAck> DeleteDiscordEventAsync(
        string accountId,
        string eventId,
        CancellationToken cancellationToken = default)
    {
        var path = DiscordPath(accountId, $"/events/{Uri.EscapeDataString(eventId)}");
        var response = await _http.DeleteAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<DiscordAck>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// The server's roster, or the members whose name starts with <paramref name="query"/>.
    /// </summary>
    public async Task<IReadOnlyList<DiscordMember>> ListDiscordMembersAsync(
        string accountId,
        string? query = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var search = new Dictionary<string, object?> { ["q"] = query, ["limit"] = limit };
        var response = await _http
            .GetAsync(DiscordPath(accountId, "/members"), search, cancellationToken)
            .ConfigureAwait(false);
        return ToList<DiscordMember>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One member of the server.</summary>
    public async Task<DiscordMember> GetDiscordMemberAsync(
        string accountId,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var path = DiscordPath(accountId, $"/members/{Uri.EscapeDataString(memberId)}");
        var response = await _http.GetAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<DiscordMember>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The server's roles, highest first.</summary>
    public async Task<IReadOnlyList<DiscordRole>> ListDiscordRolesAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync(DiscordPath(accountId, "/roles"), null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<DiscordRole>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Add a role to the server.</summary>
    public async Task<DiscordRole> CreateDiscordRoleAsync(
        string accountId,
        DiscordRoleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http
            .PostAsync(DiscordPath(accountId, "/roles"), RoleBody(options), cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordRole>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Change a role on the server; only the fields you set are sent.</summary>
    public async Task<DiscordRole> UpdateDiscordRoleAsync(
        string accountId,
        string roleId,
        DiscordRoleOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var path = DiscordPath(accountId, $"/roles/{Uri.EscapeDataString(roleId)}");
        var response = await _http
            .RequestAsync(HttpMethod.Patch, path, RoleBody(options), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordRole>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Remove a role from the server.</summary>
    public async Task<DiscordAck> DeleteDiscordRoleAsync(
        string accountId,
        string roleId,
        CancellationToken cancellationToken = default)
    {
        var path = DiscordPath(accountId, $"/roles/{Uri.EscapeDataString(roleId)}");
        var response = await _http.DeleteAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<DiscordAck>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Give a member a role.</summary>
    public async Task<DiscordAck> AddDiscordMemberRoleAsync(
        string accountId,
        string roleId,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PutAsync(MemberRolePath(accountId, roleId, memberId), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordAck>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Take a role from a member.</summary>
    public async Task<DiscordAck> RemoveDiscordMemberRoleAsync(
        string accountId,
        string roleId,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .DeleteAsync(MemberRolePath(accountId, roleId, memberId), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<DiscordAck>(FoPostHttpClient.Unwrap(response));
    }

    private static Dictionary<string, object?> EventBody(DiscordEventOptions options)
    {
        var body = new Dictionary<string, object?>();
        Set(body, "name", options.Name);
        Set(body, "description", options.Description);
        Set(body, "start_time", options.StartTime);
        Set(body, "end_time", options.EndTime);
        Set(body, "channel_id", options.ChannelId);
        Set(body, "location", options.Location);
        Set(body, "status", options.Status);
        return body;
    }

    private static Dictionary<string, object?> RoleBody(DiscordRoleOptions options)
    {
        var body = new Dictionary<string, object?>();
        Set(body, "name", options.Name);
        if (options.Color is not null)
        {
            body["color"] = options.Color;
        }
        if (options.Hoist is not null)
        {
            body["hoist"] = options.Hoist;
        }
        if (options.Mentionable is not null)
        {
            body["mentionable"] = options.Mentionable;
        }
        Set(body, "permissions", options.Permissions);
        return body;
    }

    // A field the caller left null never goes out, so Discord keeps it as it is.
    private static void Set(Dictionary<string, object?> body, string key, string? value)
    {
        if (value is not null)
        {
            body[key] = value;
        }
    }

    private static string DiscordPath(string accountId, string suffix) =>
        $"{AccountPath(accountId)}/discord{suffix}";

    private static string MemberRolePath(string accountId, string roleId, string memberId) =>
        DiscordPath(accountId, $"/roles/{Uri.EscapeDataString(roleId)}/members/{Uri.EscapeDataString(memberId)}");

    private static string AccountPath(string accountId) => $"/v1/accounts/{Uri.EscapeDataString(accountId)}";
    // --- Per-network extras ------------------------------------------------

    /// <summary>Boards this Pinterest connection can pin to.</summary>
    public async Task<IReadOnlyList<PinterestBoard>> ListPinterestBoardsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/pinterest/boards", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<PinterestBoard>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Create a board on the connected Pinterest account.</summary>
    public async Task<PinterestBoard> CreatePinterestBoardAsync(
        string accountId,
        CreatePinterestBoardOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["name"] = options.Name };
        if (options.Description is not null)
        {
            body["description"] = options.Description;
        }
        if (options.Privacy is not null)
        {
            body["privacy"] = options.Privacy;
        }

        var response = await _http
            .PostAsync($"{AccountPath(accountId)}/pinterest/boards", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<PinterestBoard>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The channel's own playlists, with the stored default marked.</summary>
    public async Task<IReadOnlyList<YouTubePlaylist>> ListYouTubePlaylistsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/youtube/playlists", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<YouTubePlaylist>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Create a playlist on the connected channel.</summary>
    public async Task<YouTubePlaylist> CreateYouTubePlaylistAsync(
        string accountId,
        CreateYouTubePlaylistOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["title"] = options.Title };
        if (options.Description is not null)
        {
            body["description"] = options.Description;
        }
        if (options.Privacy is not null)
        {
            body["privacy"] = options.Privacy;
        }

        var response = await _http
            .PostAsync($"{AccountPath(accountId)}/youtube/playlists", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<YouTubePlaylist>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// The playlist a new video joins when the post picks none. A null <paramref name="playlistId"/>
    /// clears it. Returns what is stored afterwards.
    /// </summary>
    public async Task<string?> SetDefaultYouTubePlaylistAsync(
        string accountId,
        string? playlistId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["playlist_id"] = playlistId };
        var response = await _http
            .PutAsync($"{AccountPath(accountId)}/youtube/playlists/default", body, cancellationToken)
            .ConfigureAwait(false);
        var data = FoPostHttpClient.Unwrap(response);
        return data?["playlist_id"]?.GetValue<string>();
    }

    /// <summary>Caption tracks on one of the channel's videos.</summary>
    public async Task<IReadOnlyList<YouTubeCaptionTrack>> ListYouTubeCaptionsAsync(
        string accountId,
        string videoId,
        CancellationToken cancellationToken = default)
    {
        var path = $"{AccountPath(accountId)}/youtube/videos/{Uri.EscapeDataString(videoId)}/captions";
        var response = await _http.GetAsync(path, null, cancellationToken).ConfigureAwait(false);
        return ToList<YouTubeCaptionTrack>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Upload a caption track to a video.</summary>
    public async Task<YouTubeCaptionTrack> UploadYouTubeCaptionsAsync(
        string accountId,
        string videoId,
        UploadYouTubeCaptionsOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["language"] = options.Language,
            ["body"] = options.Body,
        };
        if (options.Name is not null)
        {
            body["name"] = options.Name;
        }
        if (options.IsDraft is not null)
        {
            body["is_draft"] = options.IsDraft;
        }

        var path = $"{AccountPath(accountId)}/youtube/videos/{Uri.EscapeDataString(videoId)}/captions";
        var response = await _http.PostAsync(path, body, cancellationToken).ConfigureAwait(false);
        return Require<YouTubeCaptionTrack>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One caption track read back as text.</summary>
    public async Task<YouTubeTranscript> ReadYouTubeTranscriptAsync(
        string accountId,
        string captionId,
        CancellationToken cancellationToken = default)
    {
        var path = $"{AccountPath(accountId)}/youtube/captions/{Uri.EscapeDataString(captionId)}";
        var response = await _http.GetAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<YouTubeTranscript>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>What a post from this Bluesky connection is written in when it does not say.</summary>
    public async Task<BlueskyLanguages> GetBlueskyLanguagesAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/bluesky/languages", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<BlueskyLanguages>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Store up to three BCP-47 tags. An empty list clears the default.</summary>
    public async Task<BlueskyLanguages> SetBlueskyLanguagesAsync(
        string accountId,
        IEnumerable<string> languages,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["languages"] = languages?.ToList() ?? new List<string>(),
        };
        var response = await _http
            .PutAsync($"{AccountPath(accountId)}/bluesky/languages", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<BlueskyLanguages>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The switches TikTok enforces at publish time, changed in the TikTok app.</summary>
    public async Task<TikTokCreatorInfo> GetTikTokCreatorInfoAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/tiktok/creator-info", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<TikTokCreatorInfo>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// TikTok's Commercial Music Library. Needs the Marketing API product on the TikTok
    /// app; without it the call fails with 403 rather than answering an empty list.
    /// </summary>
    public async Task<IReadOnlyList<TikTokMusic>> SearchTikTokMusicAsync(
        string accountId,
        string query,
        TikTokSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/tiktok/music", SearchQuery(query, options), cancellationToken)
            .ConfigureAwait(false);
        return ToList<TikTokMusic>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Places a post can be tagged with. Same TikTok product as the music library.</summary>
    public async Task<IReadOnlyList<TikTokPlace>> SearchTikTokLocationsAsync(
        string accountId,
        string query,
        TikTokSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/tiktok/locations", SearchQuery(query, options), cancellationToken)
            .ConfigureAwait(false);
        return ToList<TikTokPlace>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Resolves a share link to one of this account's own videos, for repurposing.</summary>
    public async Task<TikTokVideoSource> LookupTikTokVideoAsync(
        string accountId,
        string url,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["url"] = url };
        var response = await _http
            .PostAsync($"{AccountPath(accountId)}/tiktok/video-download", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<TikTokVideoSource>(FoPostHttpClient.Unwrap(response));
    }

    private static Dictionary<string, object?> SearchQuery(string query, TikTokSearchOptions? options)
    {
        var result = new Dictionary<string, object?> { ["q"] = query };
        if (options?.Limit is not null)
        {
            result["limit"] = options.Limit;
        }

        return result;
    }

    /// <summary>Tracks a Reel can carry. With no query Instagram answers with what is trending.</summary>
    public async Task<IReadOnlyList<InstagramAudio>> SearchInstagramAudioAsync(
        string accountId,
        InstagramAudioSearchOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>();
        if (options?.Query is not null)
        {
            query["q"] = options.Query;
        }
        if (options?.AudioType is not null)
        {
            query["audio_type"] = options.AudioType;
        }

        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/instagram/audio", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<InstagramAudio>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>How many posts are left before Instagram refuses the next one.</summary>
    public async Task<InstagramPublishingLimit> GetInstagramPublishingLimitAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/instagram/publishing-limit", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<InstagramPublishingLimit>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Stories still inside their 24 hours, posted through FoPost or not. Asking for insights costs
    /// one extra call per story.
    /// </summary>
    public async Task<IReadOnlyList<InstagramStory>> ListInstagramStoriesAsync(
        string accountId,
        bool insights = false,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>();
        if (insights)
        {
            query["insights"] = true;
        }

        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/instagram/stories", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<InstagramStory>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The insight set for one story.</summary>
    public async Task<InstagramStoryInsights> GetInstagramStoryInsightsAsync(
        string accountId,
        string storyId,
        CancellationToken cancellationToken = default)
    {
        var path = $"{AccountPath(accountId)}/instagram/stories/{Uri.EscapeDataString(storyId)}/insights";
        var response = await _http.GetAsync(path, null, cancellationToken).ConfigureAwait(false);
        return Require<InstagramStoryInsights>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Organizations a LinkedIn post can mention. People are not searchable: LinkedIn has no public
    /// person search, so a member mention needs a URN the caller already holds.
    /// </summary>
    public async Task<IReadOnlyList<LinkedInMention>> SearchLinkedInMentionsAsync(
        string accountId,
        string query,
        CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, object?> { ["q"] = query };
        var response = await _http
            .GetAsync($"{AccountPath(accountId)}/linkedin/mentions", parameters, cancellationToken)
            .ConfigureAwait(false);
        return ToList<LinkedInMention>(FoPostHttpClient.Unwrap(response));
    }

}
