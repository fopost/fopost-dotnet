using System.Net;
using System.Text.Json.Nodes;
using Xunit;

namespace FoPost.Tests;

public class AccountGroupTests
{
    private const string Group = """
    {"data":{"id":"g1","name":"Brand A","account_ids":["a1","a2"],
             "created_at":"2026-09-01T00:00:00Z","updated_at":"2026-09-02T00:00:00Z"}}
    """;

    [Fact]
    public async Task List_filters_by_workspace()
    {
        var handler = new StubHandler().Json("""{"data":[{"id":"g1","name":"Brand A","account_ids":["a1","a2"]}]}""");
        using var test = new TestClient(handler);

        var groups = await test.Client.AccountGroups.ListAsync("w1");

        Assert.Equal(new[] { "a1", "a2" }, Assert.Single(groups).AccountIds);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/account-groups?workspace_id=w1",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Create_sends_workspace_name_and_members()
    {
        var handler = new StubHandler().Json(Group, HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var group = await test.Client.AccountGroups.CreateAsync("w1", "Brand A", new[] { "a1", "a2" });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("""{"workspace_id":"w1","name":"Brand A","account_ids":["a1","a2"]}""", handler.LastBody);
        Assert.Equal("g1", group.Id);
        Assert.Equal(new DateTimeOffset(2026, 9, 2, 0, 0, 0, TimeSpan.Zero), group.UpdatedAt);
    }

    [Fact]
    public async Task Get_update_delete_and_set_members_hit_the_group_paths()
    {
        var handler = new StubHandler()
            .Json(Group)
            .Json(Group)
            .Json("""{"message":"Account group deleted"}""")
            .Json(Group);
        using var test = new TestClient(handler);

        await test.Client.AccountGroups.GetAsync("g1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/account-groups/g1", handler.LastRequest.RequestUri!.ToString());

        await test.Client.AccountGroups.UpdateAsync("g1", "Brand B");
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("""{"name":"Brand B"}""", handler.LastBody);

        await test.Client.AccountGroups.DeleteAsync("g1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);

        await test.Client.AccountGroups.SetMembersAsync("g1", new[] { "a3" });
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/account-groups/g1/members",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"account_ids":["a3"]}""", handler.LastBody);
    }

    [Fact]
    public async Task Accounts_list_filters_by_group_and_reads_the_platform_name()
    {
        var handler = new StubHandler().Json("""{"data":[{"id":"a1","name":"Shop","platformName":"Acme"}]}""");
        using var test = new TestClient(handler);

        var accounts = await test.Client.Accounts.ListAsync("w1", "g1");

        Assert.Equal("Acme", Assert.Single(accounts).PlatformName);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts?workspaceId=w1&group_id=g1",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Rename_sends_an_explicit_null_to_restore_the_platform_name()
    {
        var handler = new StubHandler().Json("""{"data":{"id":"a1","name":"Acme","platform_name":"Acme"}}""");
        using var test = new TestClient(handler);

        var account = await test.Client.Accounts.RenameAsync("a1", null);

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"display_name":null}""", handler.LastBody);
        Assert.Equal("Acme", account.PlatformName);
    }

    [Fact]
    public async Task Move_returns_the_new_workspace_and_surfaces_blocking_tables()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"id":"a1","workspace_id":"w2"}}""")
            .Json(
                """{"error":"move_blocked","message":"Blocked","blocking_tables":["posts"]}""",
                HttpStatusCode.Conflict);
        using var test = new TestClient(handler);

        var moved = await test.Client.Accounts.MoveAsync("a1", "w2");
        Assert.Equal("w2", moved.WorkspaceId);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/move", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"workspace_id":"w2"}""", handler.LastBody);

        var error = await Assert.ThrowsAsync<FoPostException>(() => test.Client.Accounts.MoveAsync("a1", "w2"));
        Assert.Equal("move_blocked", error.Code);
        Assert.Equal("posts", error.Body!["blocking_tables"]![0]!.GetValue<string>());
    }

    [Fact]
    public async Task A_post_can_target_a_group_without_accounts()
    {
        var handler = new StubHandler().Json(Fixtures.Post, HttpStatusCode.Created);
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "w1",
            AccountGroupId = "g1",
            Content = new List<PostContent> { new("Hi") },
        });

        var body = JsonNode.Parse(handler.LastBody!)!.AsObject();
        Assert.Equal("g1", body["account_group_id"]!.GetValue<string>());
        Assert.False(body.ContainsKey("accounts"));
    }
}
