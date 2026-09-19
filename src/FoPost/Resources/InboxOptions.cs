namespace FoPost;

/// <summary>Filters for <see cref="Resources.InboxResource.ListAsync"/>.</summary>
public sealed class ListInboxOptions
{
    public string? WorkspaceId { get; set; }

    /// <summary>One of <see cref="InboxItemTypes"/>.</summary>
    public string? Type { get; set; }

    /// <summary>One of <see cref="InboxItemStates"/>.</summary>
    public string? State { get; set; }

    /// <summary>One of <see cref="Platforms"/>.</summary>
    public string? Platform { get; set; }

    public string? AccountId { get; set; }

    /// <summary>Comments under one FoPost post.</summary>
    public string? PostId { get; set; }

    /// <summary>Comments under one platform post, including posts not published through FoPost.</summary>
    public string? PostExternalId { get; set; }

    /// <summary>One DM thread.</summary>
    public string? ConversationId { get; set; }

    /// <summary><c>inbound</c> or <c>outbound</c>.</summary>
    public string? Direction { get; set; }

    /// <summary>Free-text search.</summary>
    public string? Q { get; set; }

    /// <summary><c>newest</c>, <c>oldest</c>, or <c>unanswered</c>.</summary>
    public string? Sort { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}

/// <summary>Filters for <see cref="Resources.InboxResource.ThreadsAsync"/>.</summary>
public sealed class ListInboxThreadsOptions
{
    public string? WorkspaceId { get; set; }

    /// <summary><c>comments</c> (default) for threads under our posts, <c>mentions</c> for posts we were tagged in.</summary>
    public string? Kind { get; set; }

    public string? Platform { get; set; }

    public string? AccountId { get; set; }

    /// <summary>One of <see cref="InboxItemStates"/>.</summary>
    public string? State { get; set; }

    public string? Q { get; set; }

    /// <summary><c>newest</c>, <c>oldest</c>, or <c>unanswered</c>.</summary>
    public string? Sort { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}

/// <summary>Filters for <see cref="Resources.InboxResource.ConversationsAsync"/>.</summary>
public sealed class ListInboxConversationsOptions
{
    public string? WorkspaceId { get; set; }

    public string? Platform { get; set; }

    public string? AccountId { get; set; }

    /// <summary>One of <see cref="InboxItemStates"/>.</summary>
    public string? State { get; set; }

    public string? Q { get; set; }

    /// <summary><c>newest</c>, <c>oldest</c>, or <c>unanswered</c>.</summary>
    public string? Sort { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.InboxResource.MarkThreadReadAsync"/>. Name
/// either the platform post whose comments to settle or the DM thread.
/// </summary>
public sealed class MarkInboxThreadReadOptions
{
    public string WorkspaceId { get; set; } = string.Empty;

    public string AccountId { get; set; } = string.Empty;

    /// <summary>The platform post id whose thread to settle.</summary>
    public string? PostExternalId { get; set; }

    /// <summary>The DM thread to settle.</summary>
    public string? ConversationId { get; set; }
}

/// <summary>The body of <see cref="Resources.InboxResource.UpdateAsync"/>.</summary>
public sealed class UpdateInboxItemOptions
{
    public UpdateInboxItemOptions()
    {
    }

    public UpdateInboxItemOptions(string state, DateTimeOffset? snoozedUntil = null)
    {
        State = state;
        SnoozedUntil = snoozedUntil;
    }

    /// <summary>One of <see cref="InboxItemStates"/>.</summary>
    public string State { get; set; } = InboxItemStates.Read;

    /// <summary>Required when <see cref="State"/> is <c>snoozed</c>; must be in the future.</summary>
    public DateTimeOffset? SnoozedUntil { get; set; }
}

/// <summary>The body of the <see cref="Resources.InboxResource.ReplyAsync(string, ReplyInboxItemOptions, System.Threading.CancellationToken)"/> overload.</summary>
public sealed class ReplyInboxItemOptions
{
    /// <summary>Required unless <see cref="MediaIds"/> is given.</summary>
    public string? Text { get; set; }

    /// <summary>Media library ids to attach to a DM, at most 10. Only where <c>CanSendMedia</c> is true.</summary>
    public IList<string>? MediaIds { get; set; }

    /// <summary>Answer buttons under a DM, at most 13 of up to 20 characters. Only where <c>CanQuickReply</c> is true.</summary>
    public IList<string>? QuickReplies { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.InboxResource.StartConversationAsync"/>. Name either
/// <see cref="AccountId"/> and <see cref="Handle"/>, or <see cref="CommentId"/>.
/// </summary>
public sealed class StartInboxConversationOptions
{
    /// <summary>The account to send from, with <see cref="Handle"/>.</summary>
    public string? AccountId { get; set; }

    /// <summary>Who to message.</summary>
    public string? Handle { get; set; }

    /// <summary>An inbox comment to answer privately instead. Only where <c>CanPrivateReply</c> is true.</summary>
    public string? CommentId { get; set; }

    public string Text { get; set; } = string.Empty;

    /// <summary>Media library ids to attach, at most 10.</summary>
    public IList<string>? MediaIds { get; set; }
}
