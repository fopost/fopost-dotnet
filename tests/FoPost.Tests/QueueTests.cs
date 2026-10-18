using System.Net;
using System.Text.Json.Nodes;
using Xunit;

namespace FoPost.Tests;

/// <summary>The posting queue: slots, the next free time, the preview, and <c>schedule: queue</c> on a post.</summary>
public class QueueTests
{
    private const string Slot = """
    {"id":"slot_1","workspace_id":"ws_1","account_id":null,"weekday":1,"time":"09:00","timezone":"Europe/Zagreb","created_at":"2026-09-01T00:00:00Z"}
    """;

    [Fact]
    public async Task List_create_update_and_delete_slots()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":[{{Slot}}]}""")
            .Json(Slot, HttpStatusCode.Created)
            .Json("""{"id":"slot_1","workspace_id":"ws_1","account_id":"acc_1","weekday":2,"time":"10:30","timezone":"Europe/Zagreb","moved":[{"id":"post_1","from":"2026-09-07T07:00:00Z","to":"2026-09-08T08:30:00Z"}]}""")
            .Json("""{"deleted":true,"moved":[]}""");
        using var test = new TestClient(handler);

        var slots = await test.Client.Queue.ListSlotsAsync("ws_1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/queue/slots?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("09:00", Assert.Single(slots).Time);

        var created = await test.Client.Queue.CreateSlotAsync(new CreateQueueSlotOptions
        {
            WorkspaceId = "ws_1",
            Weekday = 1,
            Time = "09:00",
            Timezone = "Europe/Zagreb",
        });
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/queue/slots", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"workspace_id":"ws_1","weekday":1,"time":"09:00","timezone":"Europe/Zagreb"}""", handler.LastBody);
        Assert.Equal("slot_1", created.Id);
        Assert.Null(created.AccountId);

        var updated = await test.Client.Queue.UpdateSlotAsync("slot_1", new UpdateQueueSlotOptions
        {
            AccountId = "acc_1",
            Time = "10:30",
        });
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/queue/slots/slot_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"account_id":"acc_1","time":"10:30"}""", handler.LastBody);
        Assert.Equal("post_1", Assert.Single(updated.Moved!).Id);
        Assert.Equal(new DateTimeOffset(2026, 9, 8, 8, 30, 0, TimeSpan.Zero), updated.Moved![0].To);

        var deleted = await test.Client.Queue.DeleteSlotAsync("slot_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.True(deleted.Deleted);
        Assert.Empty(deleted.Moved);
    }

    [Fact]
    public async Task Next_and_preview_read_the_slot_times()
    {
        var handler = new StubHandler()
            .Json("""{"next":"2026-09-07T07:00:00Z"}""")
            .Json("""{"data":["2026-09-07T07:00:00Z","2026-09-08T08:30:00Z"]}""");
        using var test = new TestClient(handler);

        var next = await test.Client.Queue.NextAsync("ws_1", "acc_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/queue/next?workspace_id=ws_1&account_id=acc_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(new DateTimeOffset(2026, 9, 7, 7, 0, 0, TimeSpan.Zero), next.Next);

        var preview = await test.Client.Queue.PreviewAsync("ws_1", new PreviewQueueOptions { Count = 2 });
        Assert.Equal($"{TestClient.BaseUrl}/v1/queue/preview?workspace_id=ws_1&count=2", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(2, preview.Count);
        Assert.Equal(new DateTimeOffset(2026, 9, 8, 8, 30, 0, TimeSpan.Zero), preview[1]);
    }

    [Fact]
    public async Task A_post_can_be_created_and_updated_onto_the_queue()
    {
        var handler = new StubHandler().Json(Fixtures.Post, HttpStatusCode.Created).Json(Fixtures.Post);
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Accounts = new List<string> { "acc_1" },
            Content = new List<PostContent> { new("Hi") },
            Status = PostStatuses.Scheduled,
            Schedule = PostSchedules.Queue,
        });
        var body = JsonNode.Parse(handler.LastBody!)!.AsObject();
        Assert.Equal("queue", body["schedule"]!.GetValue<string>());
        Assert.False(body.ContainsKey("schedule_at"));

        await test.Client.Posts.UpdateAsync("post_1", new UpdatePostOptions { Schedule = PostSchedules.Queue });
        Assert.Equal("""{"schedule":"queue"}""", handler.LastBody);
    }
}
