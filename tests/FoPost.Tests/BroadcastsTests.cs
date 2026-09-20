using Xunit;

namespace FoPost.Tests;

public class BroadcastsTests
{
    private const string BroadcastJson = """
    {
      "id": "bc_1",
      "name": "September check-in",
      "text": "New colours just landed.",
      "account_id": "acc_1",
      "audience": { "platforms": ["instagram"] },
      "status": "sent",
      "scheduled_at": null,
      "sent_at": "2026-09-19T10:04:00.000Z",
      "created_at": "2026-09-19T09:58:00.000Z",
      "counts": { "total": 3, "sent": 2, "skipped": 1, "failed": 0, "pending": 0 }
    }
    """;

    [Fact]
    public async Task List_reads_the_pagination_block_rather_than_meta()
    {
        var handler = new StubHandler().Json($$"""
        { "data": [{{BroadcastJson}}], "pagination": { "page": 2, "per_page": 10, "total": 11 } }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Broadcasts.ListAsync(new ListBroadcastsOptions
        {
            WorkspaceId = "ws_1",
            Status = BroadcastStatuses.Sent,
            Page = 2,
            PerPage = 10,
        });

        Assert.Equal("/v1/broadcasts", handler.LastRequest.RequestUri!.AbsolutePath);
        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("workspace_id=ws_1", query, StringComparison.Ordinal);
        Assert.Contains("status=sent", query, StringComparison.Ordinal);

        var broadcast = Assert.Single(page);
        Assert.Equal("September check-in", broadcast.Name);
        Assert.Equal(2, broadcast.Counts!.Sent);
        Assert.Equal(1, broadcast.Counts!.Skipped);
        Assert.Equal(11, page.Pagination.Total);
    }

    [Fact]
    public async Task Create_sends_the_snake_case_body()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{BroadcastJson}} }""", System.Net.HttpStatusCode.Created);
        using var test = new TestClient(handler);

        await test.Client.Broadcasts.CreateAsync(new CreateBroadcastOptions
        {
            WorkspaceId = "ws_1",
            AccountId = "acc_1",
            Name = "September check-in",
            Text = "New colours just landed.",
            Audience = AudienceFilter.OnPlatforms("instagram"),
        });

        var body = handler.LastBody!;
        Assert.Contains("\"workspace_id\":\"ws_1\"", body, StringComparison.Ordinal);
        Assert.Contains("\"account_id\":\"acc_1\"", body, StringComparison.Ordinal);
        Assert.Contains("\"platforms\":[\"instagram\"]", body, StringComparison.Ordinal);
    }

    /// <summary>A closed messaging window has to be readable, or a non-send is a mystery.</summary>
    [Fact]
    public async Task A_skipped_recipient_keeps_its_reason()
    {
        var handler = new StubHandler().Json("""
        {
          "data": [{
            "contact_id": "con_1",
            "display_name": "Sam Rivera",
            "status": "skipped",
            "skip_reason": "window_closed",
            "sent_at": null,
            "error": null
          }],
          "pagination": { "page": 1, "per_page": 50, "total": 1 }
        }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Broadcasts.RecipientsAsync(
            "bc_1",
            new ListRecipientsOptions { Status = RecipientStatuses.Skipped });

        Assert.Equal("/v1/broadcasts/bc_1/recipients", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("status=skipped", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);

        var recipient = Assert.Single(page);
        Assert.Equal(RecipientStatuses.Skipped, recipient.Status);
        Assert.Equal(SkipReasons.WindowClosed, recipient.SkipReason);
    }

    [Fact]
    public async Task Send_reports_how_many_matched()
    {
        var handler = new StubHandler().Json("""
        { "data": { "id": "bc_1", "status": "sending", "recipients": 3 } }
        """);
        using var test = new TestClient(handler);

        var sent = await test.Client.Broadcasts.SendAsync("bc_1");

        Assert.Equal("/v1/broadcasts/bc_1/send", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(3, sent.Recipients);
        Assert.Equal("sending", sent.Status);
    }

    [Fact]
    public async Task Sequence_steps_travel_as_given()
    {
        var handler = new StubHandler().Json("""
        {
          "data": {
            "id": "seq_1",
            "name": "Welcome",
            "account_id": "acc_1",
            "steps": [
              { "delay_hours": 0, "text": "Hi" },
              { "delay_hours": 48, "text": "Still here?" }
            ],
            "status": "active",
            "created_at": "2026-09-12T08:00:00.000Z"
          }
        }
        """, System.Net.HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var sequence = await test.Client.Sequences.CreateAsync(new CreateSequenceOptions
        {
            WorkspaceId = "ws_1",
            AccountId = "acc_1",
            Name = "Welcome",
            Steps = new[] { SequenceStep.Of(0, "Hi") },
        });

        Assert.Equal(48, sequence.Steps[1].DelayHours);
        Assert.Contains("\"delay_hours\":0", handler.LastBody!, StringComparison.Ordinal);
        Assert.Contains("\"text\":\"Hi\"", handler.LastBody!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Enroll_takes_ids_or_an_audience()
    {
        var handler = new StubHandler()
            .Json("""{ "data": { "id": "seq_1", "enrolled": 2 } }""")
            .Json("""{ "data": { "id": "seq_1", "enrolled": 5 } }""");
        using var test = new TestClient(handler);

        var byId = await test.Client.Sequences.EnrollAsync(
            "seq_1",
            new EnrollOptions { ContactIds = new[] { "con_1", "con_2" } });

        Assert.Equal(2, byId.Count);
        Assert.Contains("\"contact_ids\":[\"con_1\",\"con_2\"]", handler.LastBody!, StringComparison.Ordinal);

        await test.Client.Sequences.EnrollAsync(
            "seq_1",
            new EnrollOptions { Audience = AudienceFilter.OnPlatforms("telegram") });

        Assert.Contains("\"platforms\":[\"telegram\"]", handler.LastBody!, StringComparison.Ordinal);
        Assert.DoesNotContain("contact_ids", handler.LastBody!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Unenroll_names_the_contacts_it_stops()
    {
        var handler = new StubHandler().Json("""{ "data": { "id": "seq_1", "stopped": 1 } }""");
        using var test = new TestClient(handler);

        var stopped = await test.Client.Sequences.UnenrollAsync("seq_1", new[] { "con_1" });

        Assert.Equal("/v1/sequences/seq_1/unenroll", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("\"contact_ids\":[\"con_1\"]", handler.LastBody!, StringComparison.Ordinal);
        Assert.Equal(1, stopped.Stopped);
    }
}
