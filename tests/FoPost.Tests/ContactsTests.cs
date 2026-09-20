using System.Net.Http;
using Xunit;

namespace FoPost.Tests;

public class ContactsTests
{
    private const string ContactJson = """
    {
      "id": "con_1",
      "display_name": "Ada Okafor",
      "channels": [
        { "platform": "instagram", "handle": "adaokafor", "externalId": "178414" },
        { "platform": "x", "handle": "ada_writes", "externalId": null }
      ],
      "source": "inbox",
      "note": null,
      "first_seen_at": "2026-04-02T09:14:00.000Z",
      "last_seen_at": "2026-09-18T14:30:00.000Z",
      "fields": { "plan_tier": "Pro" },
      "labels": [{ "id": "lbl_1", "name": "VIP", "color": "#0070f3" }]
    }
    """;

    [Fact]
    public async Task List_reads_the_pagination_block_rather_than_meta()
    {
        var handler = new StubHandler().Json($$"""
        { "data": [{{ContactJson}}], "pagination": { "page": 2, "per_page": 10, "total": 11 } }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Contacts.ListAsync(new ListContactsOptions
        {
            WorkspaceId = "ws_1",
            Search = "ada",
            Page = 2,
            PerPage = 10,
        });

        Assert.Equal("/v1/contacts", handler.LastRequest.RequestUri!.AbsolutePath);
        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("workspace_id=ws_1", query, StringComparison.Ordinal);
        Assert.Contains("search=ada", query, StringComparison.Ordinal);
        Assert.Contains("per_page=10", query, StringComparison.Ordinal);

        var contact = Assert.Single(page);
        Assert.Equal("Ada Okafor", contact.DisplayName);
        Assert.Equal("178414", contact.Channels[0].ExternalId);
        Assert.Equal("Pro", contact.Fields["plan_tier"]);
        Assert.Equal("VIP", contact.Labels[0].Name);
        Assert.Equal(11, page.Pagination.Total);
        Assert.Equal(2, page.Pagination.Page);
    }

    [Fact]
    public async Task Create_sends_the_snake_case_wire_names_and_omits_an_absent_platform_id()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{ContactJson}} }""");
        using var test = new TestClient(handler);

        await test.Client.Contacts.CreateAsync(new CreateContactOptions
        {
            WorkspaceId = "ws_1",
            Channels = new[] { ContactChannel.Of("x", "ada_writes") },
            DisplayName = "Ada Okafor",
            Fields = new Dictionary<string, string> { ["plan_tier"] = "Pro" },
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        var body = handler.LastBody!;
        Assert.Contains("\"workspace_id\":\"ws_1\"", body, StringComparison.Ordinal);
        Assert.Contains("\"display_name\":\"Ada Okafor\"", body, StringComparison.Ordinal);
        // An absent platform id must not travel as null: it would claim we know one.
        Assert.DoesNotContain("externalId", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Update_clears_a_field_with_null_and_sends_nothing_else()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{ContactJson}} }""");
        using var test = new TestClient(handler);

        await test.Client.Contacts.UpdateAsync("con_1", new UpdateContactOptions
        {
            Fields = new Dictionary<string, string?> { ["region"] = null },
        });

        Assert.Equal("PATCH", handler.LastRequest.Method.Method);
        Assert.Equal("/v1/contacts/con_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"fields":{"region":null}}""", handler.LastBody);
    }

    [Fact]
    public async Task Conversations_reads_the_threads_a_contact_appears_in()
    {
        var handler = new StubHandler().Json("""
        { "data": [{ "key": "t_182736", "account_id": "acc_1", "account_username": "yourbrand",
                     "platform": "instagram", "messages": 14, "received": 9, "sent": 5,
                     "last_message_at": "2026-09-18T14:30:00.000Z", "last_item_id": "inb_1" }] }
        """);
        using var test = new TestClient(handler);

        var rows = await test.Client.Contacts.ConversationsAsync("con_1", 10);

        Assert.Equal("/v1/contacts/con_1/conversations", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("limit=10", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        var row = Assert.Single(rows);
        Assert.Equal("t_182736", row.Key);
        Assert.Equal(9, row.Received);
    }

    [Fact]
    public async Task Import_reports_what_merged_and_what_was_skipped()
    {
        var handler = new StubHandler().Json("""
        { "data": { "created": 1, "merged": 2,
                    "skipped": [{ "row": 4, "reason": "platform and handle are both required" }],
                    "unknownColumns": ["lifetime_value"] } }
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Contacts.ImportAsync("ws_1", "platform,handle\nx,ada_writes");

        Assert.Equal(1, result.Created);
        Assert.Equal(2, result.Merged);
        Assert.Equal(4, Assert.Single(result.Skipped).Row);
        Assert.Equal("lifetime_value", Assert.Single(result.UnknownColumns));
    }

    [Fact]
    public async Task CreateField_puts_the_workspace_on_the_query()
    {
        var handler = new StubHandler().Json("""
        { "data": { "id": "fld_1", "key": "plan_tier", "name": "Plan Tier",
                    "type": "select", "options": ["Free", "Pro"], "position": 0 } }
        """);
        using var test = new TestClient(handler);

        var field = await test.Client.Contacts.CreateFieldAsync("ws_1", new CreateContactFieldOptions
        {
            Key = "plan_tier",
            Name = "Plan Tier",
            Type = ContactFieldTypes.Select,
            Options = new[] { "Free", "Pro" },
        });

        Assert.Equal("/v1/contacts/fields", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("workspace_id=ws_1", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        Assert.Equal("plan_tier", field.Key);
        Assert.Equal(2, field.Options.Count);
    }

    [Fact]
    public async Task ConversationAnalytics_reads_the_analytics_route()
    {
        var handler = new StubHandler().Json("""
        { "data": { "conversations": [{ "key": "t_1", "accountId": "acc_1", "platform": "instagram",
                      "received": 9, "sent": 5, "answered": 5, "open": 1,
                      "medianResponseMinutes": 47, "firstMessageAt": null, "lastMessageAt": null }],
                    "total": 128, "page": 1, "perPage": 25 } }
        """);
        using var test = new TestClient(handler);

        var report = await test.Client.Contacts.ConversationAnalyticsAsync(new ConversationAnalyticsOptions
        {
            Days = 30,
            Sort = ConversationSorts.Slowest,
        });

        Assert.Equal("/v1/analytics/inbox/conversations", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("days=30", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        Assert.Contains("sort=slowest", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        Assert.Equal(128, report.Total);
        Assert.Equal(47, Assert.Single(report.Conversations).MedianResponseMinutes);
    }
}
