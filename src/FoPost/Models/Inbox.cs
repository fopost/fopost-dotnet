using System.Collections;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>The kinds of inbox item the API reads.</summary>
public static class InboxItemTypes
{
    public const string Comment = "comment";
    public const string Mention = "mention";
    public const string Dm = "dm";
}

/// <summary>The states an inbox item moves through.</summary>
public static class InboxItemStates
{
    public const string Unread = "unread";
    public const string Read = "read";
    public const string Resolved = "resolved";
    public const string Snoozed = "snoozed";
}

/// <summary>The connected account an inbox item arrived on.</summary>
public sealed class InboxAccountRef : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}

/// <summary>Media or a link attached to an inbox item.</summary>
public sealed class InboxAttachment : FoPostModel
{
    /// <summary><c>image</c>, <c>video</c>, <c>audio</c>, <c>file</c>, <c>link</c> or <c>share</c>.</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>Served by the API, never a platform URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("previewUrl")]
    public string? PreviewUrl { get; set; }
}

/// <summary>The FoPost post a platform post was published from.</summary>
public sealed class InboxPostRef : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

/// <summary>The platform post an item sits under, whoever published it.</summary>
public sealed class InboxPostContext : FoPostModel
{
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    /// <summary>Published from one of our accounts.</summary>
    [JsonPropertyName("isOwn")]
    public bool IsOwn { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("authorName")]
    public string? AuthorName { get; set; }

    [JsonPropertyName("authorHandle")]
    public string? AuthorHandle { get; set; }

    [JsonPropertyName("authorAvatarUrl")]
    public string? AuthorAvatarUrl { get; set; }

    [JsonPropertyName("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTimeOffset? PublishedAt { get; set; }

    [JsonPropertyName("published")]
    public InboxPostRef? Published { get; set; }
}

/// <summary>A comment, mention, or DM read from a connected account.</summary>
public sealed class InboxItem : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    /// <summary>One of <see cref="InboxItemTypes"/>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>One of <see cref="InboxItemStates"/>.</summary>
    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    /// <summary><c>inbound</c> or <c>outbound</c>.</summary>
    [JsonPropertyName("direction")]
    public string? Direction { get; set; }

    [JsonPropertyName("conversationId")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("authorName")]
    public string? AuthorName { get; set; }

    [JsonPropertyName("authorHandle")]
    public string? AuthorHandle { get; set; }

    [JsonPropertyName("authorAvatarUrl")]
    public string? AuthorAvatarUrl { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("attachments")]
    public IList<InboxAttachment> Attachments { get; set; } = new List<InboxAttachment>();

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("postExternalId")]
    public string? PostExternalId { get; set; }

    [JsonPropertyName("parentExternalId")]
    public string? ParentExternalId { get; set; }

    [JsonPropertyName("platformCreatedAt")]
    public DateTimeOffset? PlatformCreatedAt { get; set; }

    [JsonPropertyName("snoozedUntil")]
    public DateTimeOffset? SnoozedUntil { get; set; }

    [JsonPropertyName("repliedAt")]
    public DateTimeOffset? RepliedAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>False on platforms whose API reads but cannot answer.</summary>
    [JsonPropertyName("canReply")]
    public bool CanReply { get; set; }

    [JsonPropertyName("hidden")]
    public bool Hidden { get; set; }

    [JsonPropertyName("liked")]
    public bool Liked { get; set; }

    [JsonPropertyName("pinned")]
    public bool Pinned { get; set; }

    /// <summary>Our reaction on a DM.</summary>
    [JsonPropertyName("reaction")]
    public string? Reaction { get; set; }

    [JsonPropertyName("editedAt")]
    public DateTimeOffset? EditedAt { get; set; }

    [JsonPropertyName("canHide")]
    public bool CanHide { get; set; }

    /// <summary>A comment someone left, or our own reply.</summary>
    [JsonPropertyName("canDelete")]
    public bool CanDelete { get; set; }

    [JsonPropertyName("canLike")]
    public bool CanLike { get; set; }

    /// <summary>Our own comment only.</summary>
    [JsonPropertyName("canPin")]
    public bool CanPin { get; set; }

    /// <summary>Our own comment only.</summary>
    [JsonPropertyName("canEdit")]
    public bool CanEdit { get; set; }

    [JsonPropertyName("canReact")]
    public bool CanReact { get; set; }

    [JsonPropertyName("canSendMedia")]
    public bool CanSendMedia { get; set; }

    [JsonPropertyName("canQuickReply")]
    public bool CanQuickReply { get; set; }

    /// <summary>A DM can be opened from this comment with <c>StartConversationAsync</c>.</summary>
    [JsonPropertyName("canPrivateReply")]
    public bool CanPrivateReply { get; set; }

    /// <summary>The FoPost post this item was left under, when we published it.</summary>
    [JsonPropertyName("post")]
    public InboxPostRef? Post { get; set; }

    [JsonPropertyName("postContext")]
    public InboxPostContext? PostContext { get; set; }

    [JsonPropertyName("account")]
    public InboxAccountRef? Account { get; set; }
}

/// <summary>One platform post with comments, or one post we were mentioned in.</summary>
public sealed class InboxThread : FoPostModel
{
    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("postExternalId")]
    public string? PostExternalId { get; set; }

    [JsonPropertyName("commentCount")]
    public int CommentCount { get; set; }

    [JsonPropertyName("unreadCount")]
    public int UnreadCount { get; set; }

    [JsonPropertyName("lastCommentAt")]
    public DateTimeOffset? LastCommentAt { get; set; }

    [JsonPropertyName("lastCommentText")]
    public string? LastCommentText { get; set; }

    [JsonPropertyName("lastCommentAuthor")]
    public string? LastCommentAuthor { get; set; }

    [JsonPropertyName("post")]
    public InboxPostContext? Post { get; set; }

    [JsonPropertyName("account")]
    public InboxAccountRef? Account { get; set; }
}

/// <summary>The other side of a DM thread.</summary>
public sealed class InboxParticipant : FoPostModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }
}

/// <summary>One DM thread.</summary>
public sealed class InboxConversation : FoPostModel
{
    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("conversationId")]
    public string ConversationId { get; set; } = string.Empty;

    [JsonPropertyName("messageCount")]
    public int MessageCount { get; set; }

    [JsonPropertyName("unreadCount")]
    public int UnreadCount { get; set; }

    [JsonPropertyName("lastMessageAt")]
    public DateTimeOffset? LastMessageAt { get; set; }

    [JsonPropertyName("lastMessageText")]
    public string? LastMessageText { get; set; }

    [JsonPropertyName("lastMessageOutbound")]
    public bool LastMessageOutbound { get; set; }

    [JsonPropertyName("participant")]
    public InboxParticipant? Participant { get; set; }

    [JsonPropertyName("account")]
    public InboxAccountRef? Account { get; set; }
}

/// <summary>A connected account and what its inbox can read.</summary>
public sealed class InboxAccount : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }

    /// <summary>Comments and mentions can be read for this account.</summary>
    [JsonPropertyName("inboxSupported")]
    public bool InboxSupported { get; set; }

    [JsonPropertyName("pendingReason")]
    public string? PendingReason { get; set; }

    [JsonPropertyName("dmSupported")]
    public bool DmSupported { get; set; }

    [JsonPropertyName("dmPendingReason")]
    public string? DmPendingReason { get; set; }

    /// <summary>A new DM can be opened from this account by handle.</summary>
    [JsonPropertyName("canStartConversation")]
    public bool CanStartConversation { get; set; }
}

/// <summary>What the inbox can read on one platform: <c>live</c>, <c>soon</c>, or <c>none</c>.</summary>
public sealed class InboxPlatform : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("comments")]
    public string Comments { get; set; } = string.Empty;

    [JsonPropertyName("dms")]
    public string Dms { get; set; } = string.Empty;
}

/// <summary>The item a drafted reply answers.</summary>
public sealed class InboxApprovalItem : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("authorName")]
    public string? AuthorName { get; set; }

    [JsonPropertyName("authorHandle")]
    public string? AuthorHandle { get; set; }

    [JsonPropertyName("authorAvatarUrl")]
    public string? AuthorAvatarUrl { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("platformCreatedAt")]
    public DateTimeOffset? PlatformCreatedAt { get; set; }
}

/// <summary>A reply an automation or the agent drafted that a person still has to send.</summary>
public sealed class InboxApproval : FoPostModel
{
    /// <summary>Approval id, used by the approve and reject calls.</summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }

    /// <summary>What drafted the reply: an automation or the agent.</summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>The drafted text.</summary>
    [JsonPropertyName("reply")]
    public string Reply { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("item")]
    public InboxApprovalItem? Item { get; set; }
}

/// <summary>The outcome of approving or rejecting a drafted reply.</summary>
public sealed class InboxApprovalDecision : FoPostModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = string.Empty;
}

/// <summary>Where a reply landed on the platform.</summary>
public sealed class InboxReplyRef : FoPostModel
{
    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    [JsonPropertyName("externalUrl")]
    public string? ExternalUrl { get; set; }
}

/// <summary>The item after a reply, and where the reply landed.</summary>
public sealed class InboxReplyResult : FoPostModel
{
    [JsonPropertyName("item")]
    public InboxItem Item { get; set; } = new();

    [JsonPropertyName("reply")]
    public InboxReplyRef? Reply { get; set; }
}

/// <summary>The DM a new conversation opened, and the message sent into it.</summary>
public sealed class InboxConversationStart : FoPostModel
{
    [JsonPropertyName("conversationId")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("item")]
    public InboxItem? Item { get; set; }
}

/// <summary>An account whose DM grant has to be renewed before its DMs can be read again.</summary>
public sealed class InboxDmReconnect : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("account")]
    public string Account { get; set; } = string.Empty;
}

/// <summary>What one inbox poll found.</summary>
public sealed class InboxRefreshResult : FoPostModel
{
    [JsonPropertyName("accountsPolled")]
    public int AccountsPolled { get; set; }

    [JsonPropertyName("newItems")]
    public int NewItems { get; set; }

    /// <summary>Accounts the platform rate-limited during this poll.</summary>
    [JsonPropertyName("rateLimited")]
    public int RateLimited { get; set; }

    [JsonPropertyName("dmReconnect")]
    public IList<InboxDmReconnect> DmReconnect { get; set; } = new List<InboxDmReconnect>();
}

/// <summary>Pagination detail beside an inbox list. The inbox spells it differently from posts.</summary>
public sealed class InboxPageMeta : FoPostModel
{
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("perPage")]
    public int? PerPage { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>One page of an inbox list: its items plus the pagination meta.</summary>
public sealed class InboxPage<T> : IReadOnlyList<T>
{
    public InboxPage(IReadOnlyList<T> items, InboxPageMeta meta)
    {
        Items = items;
        Meta = meta;
    }

    public IReadOnlyList<T> Items { get; }

    public InboxPageMeta Meta { get; }

    public int Count => Items.Count;

    public T this[int index] => Items[index];

    public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// The outcome of a Messenger thread hand-over. <see cref="AppId"/> is null when control
/// was taken back.
/// </summary>
public sealed class InboxHandover : FoPostModel
{
    [JsonPropertyName("app_id")]
    public string? AppId { get; set; }

    /// <summary><c>passed</c> or <c>taken</c>.</summary>
    [JsonPropertyName("control")]
    public string Control { get; set; } = string.Empty;
}
