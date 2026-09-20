using System.Net.Http;
using Xunit;

namespace FoPost.Tests;

public class ActivityTests
{
    private const string SecurityEvent = """
    {
      "id": "evt_1",
      "workspace_id": "ws_1",
      "kind": "security",
      "ref_type": "member_removed",
      "ref_id": "usr_2",
      "summary": "Removed sam@example.com",
      "actor": { "type": "user", "name": "Ada" },
      "time": "2026-09-20T10:00:00.000Z"
    }
    """;

    [Fact]
    public async Task List_reads_the_audit_log_and_keeps_the_cursor()
    {
        var handler = new StubHandler().Json($$"""
        { "data": [{{SecurityEvent}}], "meta": { "next_cursor": "42" } }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Activity.ListAsync(new ListActivityOptions
        {
            WorkspaceId = "ws_1",
            Kind = ActivityKinds.Security,
            Limit = 1,
        });

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("/v1/activity", handler.LastRequest.RequestUri!.AbsolutePath);
        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("workspace_id=ws_1", query, StringComparison.Ordinal);
        Assert.Contains("kind=security", query, StringComparison.Ordinal);

        var item = Assert.Single(page);
        Assert.Equal("member_removed", item.RefType);
        Assert.Equal("Ada", item.Actor.Name);
        Assert.Equal("42", page.NextCursor);
    }

    [Fact]
    public async Task The_end_of_the_list_is_a_null_cursor()
    {
        var handler = new StubHandler().Json("""{ "data": [], "meta": { "next_cursor": null } }""");
        using var test = new TestClient(handler);

        var page = await test.Client.Activity.ListAsync();

        Assert.Empty(page);
        Assert.Null(page.NextCursor);
    }
}
