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

    private static string AccountPath(string accountId) => $"/v1/accounts/{Uri.EscapeDataString(accountId)}";
}
