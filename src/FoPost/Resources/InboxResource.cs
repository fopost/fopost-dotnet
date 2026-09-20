using System.Text.Json;
using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Inbox</c> — comments, mentions, reviews, and DMs read from connected
/// accounts, and the replies drafted for them. Needs the <c>inbox</c> scope.
/// </summary>
/// <example>
/// <code>
/// var unread = await client.Inbox.ListAsync(new ListInboxOptions
/// {
///     WorkspaceId = "9b2f6c1e-…",
///     State = InboxItemStates.Unread,
/// });
///
/// foreach (var item in unread)
/// {
///     if (item.CanReply)
///     {
///         await client.Inbox.ReplyAsync(item.Id, "Thanks for reaching out!");
///     }
/// }
/// </code>
/// </example>
public sealed class InboxResource
{
    private readonly FoPostHttpClient _http;

    internal InboxResource(FoPostHttpClient http) => _http = http;

    /// <summary>Comments, mentions, reviews, and DMs, newest first unless <c>Sort</c> says otherwise.</summary>
    public async Task<InboxPage<InboxItem>> ListAsync(
        ListInboxOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ListInboxOptions();

        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["type"] = options.Type,
            ["state"] = options.State,
            ["platform"] = options.Platform,
            ["account_id"] = options.AccountId,
            ["post_id"] = options.PostId,
            ["post_external_id"] = options.PostExternalId,
            ["conversation_id"] = options.ConversationId,
            ["direction"] = options.Direction,
            ["q"] = options.Q,
            ["sort"] = options.Sort,
            ["page"] = options.Page,
            ["per_page"] = options.PerPage,
        };

        var body = await _http.GetAsync("/v1/inbox", query, cancellationToken).ConfigureAwait(false);
        return new InboxPage<InboxItem>(ToList<InboxItem>(FoPostHttpClient.Unwrap(body)), ReadInboxMeta(body));
    }

    /// <summary>One row per platform post with comments, or per post we were mentioned in.</summary>
    public async Task<InboxPage<InboxThread>> ThreadsAsync(
        ListInboxThreadsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ListInboxThreadsOptions();

        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["kind"] = options.Kind,
            ["platform"] = options.Platform,
            ["account_id"] = options.AccountId,
            ["state"] = options.State,
            ["q"] = options.Q,
            ["sort"] = options.Sort,
            ["page"] = options.Page,
            ["per_page"] = options.PerPage,
        };

        var body = await _http.GetAsync("/v1/inbox/posts", query, cancellationToken).ConfigureAwait(false);
        return new InboxPage<InboxThread>(ToList<InboxThread>(FoPostHttpClient.Unwrap(body)), ReadInboxMeta(body));
    }

    /// <summary>One row per DM thread, latest first.</summary>
    public async Task<InboxPage<InboxConversation>> ConversationsAsync(
        ListInboxConversationsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ListInboxConversationsOptions();

        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["platform"] = options.Platform,
            ["account_id"] = options.AccountId,
            ["state"] = options.State,
            ["q"] = options.Q,
            ["sort"] = options.Sort,
            ["page"] = options.Page,
            ["per_page"] = options.PerPage,
        };

        var body = await _http.GetAsync("/v1/inbox/conversations", query, cancellationToken)
            .ConfigureAwait(false);
        return new InboxPage<InboxConversation>(
            ToList<InboxConversation>(FoPostHttpClient.Unwrap(body)),
            ReadInboxMeta(body));
    }

    public async Task<int> UnreadCountAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        // Answers { count } bare, with no data envelope.
        var body = await _http.GetAsync("/v1/inbox/unread-count", query, cancellationToken)
            .ConfigureAwait(false);
        return ReadInt(body, "count");
    }

    /// <summary>Connected accounts and whether their comments and DMs can be read.</summary>
    public async Task<IReadOnlyList<InboxAccount>> AccountsAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/inbox/accounts", query, cancellationToken).ConfigureAwait(false);
        return ToList<InboxAccount>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>What the inbox can read on each platform.</summary>
    public async Task<IReadOnlyList<InboxPlatform>> PlatformsAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/inbox/platforms", null, cancellationToken).ConfigureAwait(false);
        return ToList<InboxPlatform>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Mark every item in a thread read. Returns how many changed.</summary>
    public async Task<int> MarkThreadReadAsync(
        MarkInboxThreadReadOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["account_id"] = options.AccountId,
        };
        if (options.PostExternalId is not null)
        {
            body["post_external_id"] = options.PostExternalId;
        }
        if (options.ConversationId is not null)
        {
            body["conversation_id"] = options.ConversationId;
        }

        var response = await _http.PostAsync("/v1/inbox/read", body, cancellationToken).ConfigureAwait(false);
        return ReadInt(FoPostHttpClient.Unwrap(response), "updated");
    }

    /// <summary>Poll every inbox-capable account in the workspace now.</summary>
    public async Task<InboxRefreshResult> RefreshAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var response = await _http.PostAsync("/v1/inbox/refresh", body, cancellationToken).ConfigureAwait(false);
        return Require<InboxRefreshResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Change an item's state; a snooze needs <c>SnoozedUntil</c>.</summary>
    public async Task<InboxItem> UpdateAsync(
        string itemId,
        UpdateInboxItemOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        // This body is camelCase where the read and refresh bodies are snake_case.
        var body = new Dictionary<string, object?> { ["state"] = options.State };
        if (options.SnoozedUntil is { } snoozedUntil)
        {
            body["snoozedUntil"] = Iso(snoozedUntil);
        }

        var response = await _http
            .RequestAsync(HttpMethod.Patch, ItemPath(itemId), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxItem>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Edit our own comment on the platform. Needs the <c>publish</c> scope.</summary>
    public async Task<InboxItem> EditCommentAsync(
        string itemId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["text"] = text };
        var response = await _http
            .RequestAsync(HttpMethod.Patch, ItemPath(itemId), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxItem>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Send a reply on the platform as the connected account.</summary>
    public Task<InboxReplyResult> ReplyAsync(
        string itemId,
        string text,
        CancellationToken cancellationToken = default) =>
        ReplyAsync(itemId, new ReplyInboxItemOptions { Text = text }, cancellationToken);

    /// <summary>
    /// Send a reply that may carry media and quick replies on a DM. Either of those needs the
    /// <c>publish</c> scope.
    /// </summary>
    public async Task<InboxReplyResult> ReplyAsync(
        string itemId,
        ReplyInboxItemOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Text is not null)
        {
            body["text"] = options.Text;
        }
        if (options.MediaIds is not null)
        {
            body["media_ids"] = options.MediaIds;
        }
        if (options.QuickReplies is not null)
        {
            body["quick_replies"] = options.QuickReplies;
        }

        var response = await _http.PostAsync($"{ItemPath(itemId)}/reply", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxReplyResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Hide the comment on the platform.</summary>
    public Task<InboxItem> HideAsync(string itemId, CancellationToken cancellationToken = default) =>
        Act(itemId, "hide", cancellationToken);

    public Task<InboxItem> UnhideAsync(string itemId, CancellationToken cancellationToken = default) =>
        Act(itemId, "unhide", cancellationToken);

    /// <summary>Like the item on the platform. Needs the <c>publish</c> scope.</summary>
    public Task<InboxItem> LikeAsync(string itemId, CancellationToken cancellationToken = default) =>
        Act(itemId, "like", cancellationToken);

    /// <summary>Remove our like. Needs the <c>publish</c> scope.</summary>
    public Task<InboxItem> UnlikeAsync(string itemId, CancellationToken cancellationToken = default) =>
        Act(itemId, "unlike", cancellationToken);

    /// <summary>Pin our own comment. Needs the <c>publish</c> scope.</summary>
    public Task<InboxItem> PinAsync(string itemId, CancellationToken cancellationToken = default) =>
        Act(itemId, "pin", cancellationToken);

    /// <summary>Unpin our own comment. Needs the <c>publish</c> scope.</summary>
    public Task<InboxItem> UnpinAsync(string itemId, CancellationToken cancellationToken = default) =>
        Act(itemId, "unpin", cancellationToken);

    /// <summary>
    /// React to a DM with an emoji, or pass <c>null</c> to remove ours. Needs the <c>publish</c> scope.
    /// </summary>
    public async Task<InboxItem> ReactAsync(
        string itemId,
        string? reaction,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["reaction"] = reaction };
        var response = await _http.PostAsync($"{ItemPath(itemId)}/react", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxItem>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Open a DM, by handle from an account or as a private reply to a comment. Needs the
    /// <c>publish</c> scope.
    /// </summary>
    public async Task<InboxConversationStart> StartConversationAsync(
        StartInboxConversationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["text"] = options.Text };
        if (options.AccountId is not null)
        {
            body["account_id"] = options.AccountId;
        }
        if (options.Handle is not null)
        {
            body["handle"] = options.Handle;
        }
        if (options.CommentId is not null)
        {
            body["comment_id"] = options.CommentId;
        }
        if (options.MediaIds is not null)
        {
            body["media_ids"] = options.MediaIds;
        }

        var response = await _http.PostAsync("/v1/inbox/conversations", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxConversationStart>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Show or clear the typing indicator in a DM thread. Needs the <c>publish</c> scope.
    /// </summary>
    public async Task<bool> SetTypingAsync(
        string conversationId,
        string accountId,
        bool on = true,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["account_id"] = accountId, ["on"] = on };
        var response = await _http
            .PostAsync(
                $"/v1/inbox/conversations/{Uri.EscapeDataString(conversationId)}/typing",
                body,
                cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response)?["typing"]?.GetValue<bool>() ?? false;
    }

    /// <summary>
    /// Pass a Messenger thread to another Meta app, or take it back when <paramref name="appId"/>
    /// is null. Also needs the <c>publish</c> scope.
    /// </summary>
    public async Task<InboxHandover> HandoverAsync(
        string conversationId,
        string accountId,
        string? appId = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["account_id"] = accountId };
        if (appId is not null)
        {
            body["app_id"] = appId;
        }
        if (metadata is not null)
        {
            body["metadata"] = metadata;
        }

        var response = await _http
            .PostAsync(
                $"/v1/inbox/conversations/{Uri.EscapeDataString(conversationId)}/handover",
                body,
                cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxHandover>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Delete the comment on the platform, or our own reply. Deleting our own reply needs the
    /// <c>publish</c> scope.
    /// </summary>
    public async Task<bool> DeleteAsync(string itemId, CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(ItemPath(itemId), null, cancellationToken).ConfigureAwait(false);
        var node = FoPostHttpClient.Unwrap(response);
        return node?["deleted"]?.GetValue<bool>() ?? false;
    }

    /// <summary>Replies an automation or the agent drafted that a person still has to send.</summary>
    public async Task<IReadOnlyList<InboxApproval>> ApprovalsAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/inbox/approvals", query, cancellationToken).ConfigureAwait(false);
        return ToList<InboxApproval>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Send the drafted reply, or <paramref name="text"/> in its place.</summary>
    public async Task<InboxApprovalDecision> ApproveReplyAsync(
        long approvalId,
        string? text = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();
        if (text is not null)
        {
            body["text"] = text;
        }

        var response = await _http
            .PostAsync($"/v1/inbox/approvals/{approvalId}/approve", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxApprovalDecision>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Discard the drafted reply without sending it.</summary>
    public async Task<InboxApprovalDecision> RejectReplyAsync(
        long approvalId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"/v1/inbox/approvals/{approvalId}/reject", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxApprovalDecision>(FoPostHttpClient.Unwrap(response));
    }

    private async Task<InboxItem> Act(string itemId, string action, CancellationToken cancellationToken)
    {
        var response = await _http.PostAsync($"{ItemPath(itemId)}/{action}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<InboxItem>(FoPostHttpClient.Unwrap(response));
    }

    private static string ItemPath(string itemId) => $"/v1/inbox/{Uri.EscapeDataString(itemId)}";

    private static int ReadInt(JsonNode? node, string key) => node?[key]?.GetValue<int>() ?? 0;

    // The inbox spells its meta { page, perPage, total }, unlike the posts list.
    private static InboxPageMeta ReadInboxMeta(JsonNode? body)
    {
        if (body is JsonObject obj &&
            obj.TryGetPropertyValue("meta", out var meta) &&
            meta is JsonObject)
        {
            return meta.Deserialize<InboxPageMeta>(FoPostJson.Options) ?? new InboxPageMeta();
        }

        return new InboxPageMeta();
    }
}
