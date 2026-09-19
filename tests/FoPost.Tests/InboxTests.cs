using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class InboxTests
{
    private const string Item = """
    {
      "id": "inb_1",
      "workspaceId": "ws_1",
      "platform": "instagram",
      "type": "comment",
      "state": "unread",
      "direction": "inbound",
      "conversationId": null,
      "authorName": "Jordan Rivera",
      "authorHandle": "jordan.rivera",
      "authorAvatarUrl": null,
      "text": "Love this!",
      "attachments": [{ "kind": "image", "name": null, "width": 640, "height": 480, "link": null, "url": "https://api.test.fopost.com/v1/inbox/inb_1/attachments/0", "previewUrl": null }],
      "permalink": "https://yourbrand.com/p/1",
      "postExternalId": "ext_9",
      "parentExternalId": null,
      "platformCreatedAt": "2026-09-01T10:00:00.000Z",
      "snoozedUntil": null,
      "repliedAt": null,
      "createdAt": "2026-09-01T10:01:00.000Z",
      "canReply": true,
      "hidden": false,
      "canHide": true,
      "canDelete": false,
      "post": { "id": "post_1", "title": "Launch" },
      "postContext": { "externalId": "ext_9", "isOwn": true, "text": "We shipped", "published": { "id": "post_1", "title": "Launch" } },
      "account": { "id": "acc_1", "platform": "instagram", "username": "yourbrand", "name": "Your Brand", "avatar": null }
    }
    """;

    [Fact]
    public async Task List_sends_snake_case_filters_and_reads_the_inbox_meta()
    {
        var handler = new StubHandler().Json($$"""
        { "data": [{{Item}}], "meta": { "page": 2, "perPage": 20, "total": 41 } }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Inbox.ListAsync(new ListInboxOptions
        {
            WorkspaceId = "ws_1",
            Type = InboxItemTypes.Comment,
            State = InboxItemStates.Unread,
            PostExternalId = "ext_9",
            Sort = "unanswered",
            Page = 2,
            PerPage = 20,
        });

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox", handler.LastRequest.RequestUri!.AbsolutePath);
        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("workspace_id=ws_1", query, StringComparison.Ordinal);
        Assert.Contains("type=comment", query, StringComparison.Ordinal);
        Assert.Contains("state=unread", query, StringComparison.Ordinal);
        Assert.Contains("post_external_id=ext_9", query, StringComparison.Ordinal);
        Assert.Contains("sort=unanswered", query, StringComparison.Ordinal);
        Assert.Contains("page=2", query, StringComparison.Ordinal);
        Assert.Contains("per_page=20", query, StringComparison.Ordinal);
        Assert.DoesNotContain("account_id", query, StringComparison.Ordinal);

        var item = Assert.Single(page);
        Assert.Equal("inb_1", item.Id);
        Assert.Equal("Jordan Rivera", item.AuthorName);
        Assert.True(item.CanReply);
        Assert.Equal("image", Assert.Single(item.Attachments).Kind);
        Assert.Equal("post_1", item.Post!.Id);
        Assert.True(item.PostContext!.IsOwn);
        Assert.Equal("yourbrand", item.Account!.Username);
        Assert.Equal(2, page.Meta.Page);
        Assert.Equal(20, page.Meta.PerPage);
        Assert.Equal(41, page.Meta.Total);
    }

    [Fact]
    public async Task Threads_and_conversations_hit_their_own_paths()
    {
        var handler = new StubHandler()
            .Json("""
            { "data": [{ "workspaceId": "ws_1", "accountId": "acc_1", "postExternalId": "ext_9", "commentCount": 3, "unreadCount": 1, "lastCommentText": "Nice" }], "meta": { "page": 1, "perPage": 30, "total": 1 } }
            """)
            .Json("""
            { "data": [{ "workspaceId": "ws_1", "accountId": "acc_1", "conversationId": "conv_1", "messageCount": 4, "unreadCount": 2, "lastMessageOutbound": false, "participant": { "name": "Sam Lee", "handle": "samlee", "avatarUrl": null } }], "meta": { "page": 1, "perPage": 30, "total": 1 } }
            """);
        using var test = new TestClient(handler);

        var threads = await test.Client.Inbox.ThreadsAsync(new ListInboxThreadsOptions { WorkspaceId = "ws_1", Kind = "mentions" });
        Assert.Equal("/v1/inbox/posts", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("kind=mentions", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        Assert.Equal(3, Assert.Single(threads).CommentCount);

        var conversations = await test.Client.Inbox.ConversationsAsync(new ListInboxConversationsOptions { WorkspaceId = "ws_1" });
        Assert.Equal("/v1/inbox/conversations", handler.LastRequest.RequestUri!.AbsolutePath);
        var conversation = Assert.Single(conversations);
        Assert.Equal("conv_1", conversation.ConversationId);
        Assert.Equal("Sam Lee", conversation.Participant!.Name);
    }

    [Fact]
    public async Task Unread_count_is_read_bare_and_read_marks_a_thread()
    {
        var handler = new StubHandler()
            .Json("""{"count":7}""")
            .Json("""{"data":{"updated":3}}""");
        using var test = new TestClient(handler);

        var count = await test.Client.Inbox.UnreadCountAsync("ws_1");
        Assert.Equal(7, count);
        Assert.Equal($"{TestClient.BaseUrl}/v1/inbox/unread-count?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());

        var updated = await test.Client.Inbox.MarkThreadReadAsync(new MarkInboxThreadReadOptions
        {
            WorkspaceId = "ws_1",
            AccountId = "acc_1",
            PostExternalId = "ext_9",
        });
        Assert.Equal(3, updated);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox/read", handler.LastRequest.RequestUri!.AbsolutePath);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("ws_1", body.GetProperty("workspace_id").GetString());
        Assert.Equal("acc_1", body.GetProperty("account_id").GetString());
        Assert.Equal("ext_9", body.GetProperty("post_external_id").GetString());
        Assert.False(body.TryGetProperty("conversation_id", out _));
    }

    [Fact]
    public async Task Update_patches_a_camelCase_snooze()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Item}}}""");
        using var test = new TestClient(handler);

        var item = await test.Client.Inbox.UpdateAsync(
            "inb_1",
            new UpdateInboxItemOptions(
                InboxItemStates.Snoozed,
                new DateTimeOffset(2026, 9, 2, 9, 0, 0, TimeSpan.FromHours(2))));

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox/inb_1", handler.LastRequest.RequestUri!.AbsolutePath);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("snoozed", body.GetProperty("state").GetString());
        Assert.Equal("2026-09-02T07:00:00.000Z", body.GetProperty("snoozedUntil").GetString());
        Assert.Equal("inb_1", item.Id);
    }

    [Fact]
    public async Task Reply_hide_and_delete_act_on_one_item()
    {
        var handler = new StubHandler()
            .Json($$$$"""{"data":{"item":{{{{Item}}}},"reply":{"externalId":"r_1","externalUrl":"https://yourbrand.com/r/1"}}}""")
            .Json($$"""{"data":{{Item}}}""")
            .Json("""{"data":{"deleted":true}}""");
        using var test = new TestClient(handler);

        var reply = await test.Client.Inbox.ReplyAsync("inb_1", "Thanks!");
        Assert.Equal("/v1/inbox/inb_1/reply", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"text":"Thanks!"}""", handler.LastBody);
        Assert.Equal("r_1", reply.Reply!.ExternalId);
        Assert.Equal("inb_1", reply.Item.Id);

        await test.Client.Inbox.HideAsync("inb_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox/inb_1/hide", handler.LastRequest.RequestUri!.AbsolutePath);

        var deleted = await test.Client.Inbox.DeleteAsync("inb_1");
        Assert.True(deleted);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox/inb_1", handler.LastRequest.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Refresh_reports_what_the_poll_found()
    {
        var handler = new StubHandler().Json("""
        {"data":{"accountsPolled":4,"newItems":9,"rateLimited":1,"dmReconnect":[{"platform":"instagram","account":"yourbrand"}]}}
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Inbox.RefreshAsync("ws_1");

        Assert.Equal("""{"workspace_id":"ws_1"}""", handler.LastBody);
        Assert.Equal(4, result.AccountsPolled);
        Assert.Equal(9, result.NewItems);
        Assert.Equal("instagram", Assert.Single(result.DmReconnect).Platform);
    }

    [Fact]
    public async Task Approvals_list_approve_and_reject()
    {
        var handler = new StubHandler()
            .Json("""
            {"data":[{"id":12,"workspaceId":"ws_1","source":"agent","reply":"Glad you like it!","createdAt":"2026-09-01T12:00:00.000Z","item":{"id":"inb_1","platform":"instagram","type":"comment","state":"unread","text":"Love this!"}}]}
            """)
            .Json("""{"data":{"id":12,"outcome":"sent"}}""")
            .Json("""{"data":{"id":13,"outcome":"rejected"}}""");
        using var test = new TestClient(handler);

        var approvals = await test.Client.Inbox.ApprovalsAsync("ws_1");
        var approval = Assert.Single(approvals);
        Assert.Equal(12, approval.Id);
        Assert.Equal("Glad you like it!", approval.Reply);
        Assert.Equal("inb_1", approval.Item!.Id);

        var approved = await test.Client.Inbox.ApproveReplyAsync(12, "Glad you like it, Jordan!");
        Assert.Equal("/v1/inbox/approvals/12/approve", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"text":"Glad you like it, Jordan!"}""", handler.LastBody);
        Assert.Equal("sent", approved.Outcome);

        var rejected = await test.Client.Inbox.RejectReplyAsync(13);
        Assert.Equal("/v1/inbox/approvals/13/reject", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Null(handler.LastBody);
        Assert.Equal("rejected", rejected.Outcome);
    }

    [Fact]
    public async Task Accounts_and_platforms_read_capability_flags()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"acc_1","workspaceId":"ws_1","platform":"instagram","username":"yourbrand","name":"Your Brand","avatar":null,"inboxSupported":true,"pendingReason":null,"dmSupported":false,"dmPendingReason":"reconnect"}]}""")
            .Json("""{"data":[{"platform":"instagram","comments":"live","dms":"soon"}]}""");
        using var test = new TestClient(handler);

        var accounts = await test.Client.Inbox.AccountsAsync("ws_1");
        var account = Assert.Single(accounts);
        Assert.True(account.InboxSupported);
        Assert.False(account.DmSupported);
        Assert.Equal("reconnect", account.DmPendingReason);

        var platforms = await test.Client.Inbox.PlatformsAsync();
        Assert.Equal("/v1/inbox/platforms", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("soon", Assert.Single(platforms).Dms);
    }
    [Fact]
    public async Task Item_reads_the_action_flags()
    {
        var handler = new StubHandler().Json("""
        {"data":{"id":"inb_2","platform":"instagram","type":"dm","state":"read","liked":true,"pinned":false,"reaction":"❤️","editedAt":"2026-09-02T08:00:00.000Z","canLike":true,"canPin":false,"canEdit":true,"canReact":true,"canSendMedia":true,"canQuickReply":true,"canPrivateReply":false,"canDelete":true}}
        """);
        using var test = new TestClient(handler);

        var item = await test.Client.Inbox.LikeAsync("inb_2");

        Assert.True(item.Liked);
        Assert.False(item.Pinned);
        Assert.Equal("❤️", item.Reaction);
        Assert.Equal(new DateTimeOffset(2026, 9, 2, 8, 0, 0, TimeSpan.Zero), item.EditedAt);
        Assert.True(item.CanLike);
        Assert.False(item.CanPin);
        Assert.True(item.CanEdit);
        Assert.True(item.CanReact);
        Assert.True(item.CanSendMedia);
        Assert.True(item.CanQuickReply);
        Assert.False(item.CanPrivateReply);
    }

    [Fact]
    public async Task Like_unlike_pin_and_unpin_post_to_their_actions()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":{{Item}}}""")
            .Json($$"""{"data":{{Item}}}""")
            .Json($$"""{"data":{{Item}}}""")
            .Json($$"""{"data":{{Item}}}""");
        using var test = new TestClient(handler);

        await test.Client.Inbox.LikeAsync("inb_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox/inb_1/like", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Null(handler.LastBody);

        await test.Client.Inbox.UnlikeAsync("inb_1");
        Assert.Equal("/v1/inbox/inb_1/unlike", handler.LastRequest.RequestUri!.AbsolutePath);

        await test.Client.Inbox.PinAsync("inb_1");
        Assert.Equal("/v1/inbox/inb_1/pin", handler.LastRequest.RequestUri!.AbsolutePath);

        var item = await test.Client.Inbox.UnpinAsync("inb_1");
        Assert.Equal("/v1/inbox/inb_1/unpin", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("inb_1", item.Id);
    }

    [Fact]
    public async Task React_sends_the_emoji_or_an_explicit_null()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":{{Item}}}""")
            .Json($$"""{"data":{{Item}}}""");
        using var test = new TestClient(handler);

        await test.Client.Inbox.ReactAsync("inb_1", "❤️");
        Assert.Equal("/v1/inbox/inb_1/react", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"reaction":"❤️"}""", handler.LastBody);

        await test.Client.Inbox.ReactAsync("inb_1", null);
        Assert.Equal("""{"reaction":null}""", handler.LastBody);
    }

    [Fact]
    public async Task Edit_comment_patches_the_text()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Item}}}""");
        using var test = new TestClient(handler);

        var item = await test.Client.Inbox.EditCommentAsync("inb_1", "Fixed the typo");

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox/inb_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"text":"Fixed the typo"}""", handler.LastBody);
        Assert.Equal("inb_1", item.Id);
    }

    [Fact]
    public async Task Reply_with_options_sends_media_and_quick_replies()
    {
        var handler = new StubHandler()
            .Json($$$$"""{"data":{"item":{{{{Item}}}},"reply":{"externalId":"r_2","externalUrl":null}}}""");
        using var test = new TestClient(handler);

        var reply = await test.Client.Inbox.ReplyAsync("inb_1", new ReplyInboxItemOptions
        {
            MediaIds = new List<string> { "med_1" },
            QuickReplies = new List<string> { "Yes", "No" },
        });

        Assert.Equal("/v1/inbox/inb_1/reply", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"media_ids":["med_1"],"quick_replies":["Yes","No"]}""", handler.LastBody);
        Assert.Equal("r_2", reply.Reply!.ExternalId);
    }

    [Fact]
    public async Task Start_conversation_and_typing_hit_the_conversation_routes()
    {
        var handler = new StubHandler()
            .Json($$$"""{"data":{"conversationId":"conv_9","item":{{{Item}}}}}""")
            .Json("""{"data":{"conversationId":null,"item":null}}""")
            .Json("""{"data":{"typing":false}}""");
        using var test = new TestClient(handler);

        var started = await test.Client.Inbox.StartConversationAsync(new StartInboxConversationOptions
        {
            AccountId = "acc_1",
            Handle = "samlee",
            Text = "Hi Sam",
            MediaIds = new List<string> { "med_1" },
        });
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/inbox/conversations", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"text":"Hi Sam","account_id":"acc_1","handle":"samlee","media_ids":["med_1"]}""", handler.LastBody);
        Assert.Equal("conv_9", started.ConversationId);
        Assert.Equal("inb_1", started.Item!.Id);

        var privateReply = await test.Client.Inbox.StartConversationAsync(new StartInboxConversationOptions
        {
            CommentId = "inb_1",
            Text = "Sent you the details",
        });
        Assert.Equal("""{"text":"Sent you the details","comment_id":"inb_1"}""", handler.LastBody);
        Assert.Null(privateReply.ConversationId);
        Assert.Null(privateReply.Item);

        var typing = await test.Client.Inbox.SetTypingAsync("conv_9", "acc_1", on: false);
        Assert.Equal("/v1/inbox/conversations/conv_9/typing", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"account_id":"acc_1","on":false}""", handler.LastBody);
        Assert.False(typing);
    }

    [Fact]
    public async Task Accounts_read_whether_a_conversation_can_start()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"acc_1","platform":"x","username":"yourbrand","name":"Your Brand","inboxSupported":true,"dmSupported":true,"canStartConversation":true}]}""");
        using var test = new TestClient(handler);

        var account = Assert.Single(await test.Client.Inbox.AccountsAsync("ws_1"));
        Assert.True(account.CanStartConversation);
    }
}
