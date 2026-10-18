using System.Net;
using Xunit;

namespace FoPost.Tests;

/// <summary>Automations: CRUD, stats, toggle, versions and runs.</summary>
public class AutomationsTests
{
    private const string Automation = """
    {"id":"auto_1","workspace_id":"ws_1","name":"Cross-post","description":null,"trigger_type":"content_created","trigger_config":{"platforms":["twitter"]},"active":true,"schedule_window":{"days":[1,2,3],"start":"09:00","end":"17:00"},"max_per_day":null,"credit_ceiling":50,"approval_mode":false,"last_triggered_at":null,"run_count":3,"secret":"auto_secret","graph":{"nodes":[{"id":"n1","kind":"trigger","label":null,"config":{},"position":0,"layout":{"x":0,"y":0}}],"edges":[]},"steps":[{"position":1,"node_id":"n2","action_type":"publish","action_config":{}}],"created_at":"2026-09-01T00:00:00Z","updated_at":"2026-09-01T00:00:00Z"}
    """;

    [Fact]
    public async Task List_create_get_update_delete_and_toggle()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":[{{Automation}}]}""")
            .Json(Automation, HttpStatusCode.Created)
            .Json(Automation)
            .Json(Automation)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent))
            .Json("""{"id":"auto_1","active":false}""");
        using var test = new TestClient(handler);

        var all = await test.Client.Automations.ListAsync();
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(50, Assert.Single(all).CreditCeiling);

        var created = await test.Client.Automations.CreateAsync(new CreateAutomationOptions
        {
            WorkspaceId = "ws_1",
            Name = "Cross-post",
            TriggerType = AutomationTriggerTypes.ContentCreated,
            TriggerConfig = new Dictionary<string, object?> { ["platforms"] = new[] { "twitter" } },
            Graph = new AutomationGraphInput
            {
                Nodes = new List<AutomationNodeInput>
                {
                    new() { Id = "n1", Kind = AutomationNodeKinds.Trigger, Layout = new AutomationLayoutInput { X = 0, Y = 0 } },
                    new() { Id = "n2", Kind = AutomationNodeKinds.Publish, Config = new Dictionary<string, object?> { ["accountIds"] = new[] { "acc_1" } } },
                },
                Edges = new List<AutomationEdgeInput> { new() { Id = "e1", Source = "n1", Target = "n2" } },
            },
            Active = true,
        });
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            """{"workspaceId":"ws_1","name":"Cross-post","triggerType":"content_created","triggerConfig":{"platforms":["twitter"]},"graph":{"nodes":[{"id":"n1","kind":"trigger","layout":{"x":0,"y":0}},{"id":"n2","kind":"publish","config":{"accountIds":["acc_1"]}}],"edges":[{"id":"e1","source":"n1","target":"n2"}]},"active":true}""",
            handler.LastBody);
        Assert.Equal("auto_secret", created.Secret);
        Assert.Equal("publish", Assert.Single(created.Steps!).ActionType);
        Assert.Equal(new[] { 1, 2, 3 }, created.ScheduleWindow!.Days);

        await test.Client.Automations.GetAsync("auto_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/auto_1", handler.LastRequest.RequestUri!.ToString());

        await test.Client.Automations.UpdateAsync("auto_1", new UpdateAutomationOptions { Name = "Cross-post v2", Active = false });
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("""{"name":"Cross-post v2","active":false}""", handler.LastBody);

        await test.Client.Automations.DeleteAsync("auto_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);

        var toggled = await test.Client.Automations.ToggleAsync("auto_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/auto_1/toggle", handler.LastRequest.RequestUri!.ToString());
        Assert.False(toggled.Active);
    }

    [Fact]
    public async Task Stats_reads_the_counts_and_recent_runs()
    {
        var handler = new StubHandler().Json("""
        {"total_automations":4,"active_automations":2,"total_runs_24h":9,"runs_by_status":{"completed":8,"failed":1},"recent_runs":[{"id":77,"automation_id":"auto_1","status":"completed","started_at":"2026-09-01T00:00:00Z","completed_at":"2026-09-01T00:00:05Z"}]}
        """);
        using var test = new TestClient(handler);

        var stats = await test.Client.Automations.StatsAsync();

        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/stats", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(9, stats.TotalRuns24h);
        Assert.Equal(1, stats.RunsByStatus["failed"]);
        Assert.Equal(77, Assert.Single(stats.RecentRuns).Id);
    }

    [Fact]
    public async Task Versions_list_get_and_restore()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"version":3,"note":"Added a delay","created_by":{"name":"Ops"},"created_at":"2026-09-02T00:00:00Z","node_count":4,"nodes_added":[{"id":"n3","kind":"delay","label":"Wait"}],"nodes_removed":[]}],"meta":{"page":1,"per_page":20,"total":3,"total_pages":1}}""")
            .Json("""{"version":2,"note":null,"created_by":{"name":"Ops"},"created_at":"2026-09-01T00:00:00Z","graph":{"nodes":[{"id":"n1","kind":"trigger"}],"edges":[]}}""")
            .Json("""{"version":4,"graph":{"nodes":[{"id":"n1","kind":"trigger"}],"edges":[]},"steps":[]}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var versions = await test.Client.Automations.ListVersionsAsync("auto_1", page: 1, perPage: 20);
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/auto_1/versions?page=1&per_page=20", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(3, versions.Meta.Total);
        var latest = Assert.Single(versions);
        Assert.Equal(3, latest.Version);
        Assert.Equal("Wait", Assert.Single(latest.NodesAdded!).Label);
        Assert.Equal("Ops", latest.CreatedBy!.Name);

        var version = await test.Client.Automations.GetVersionAsync("auto_1", 2);
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/auto_1/versions/2", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("trigger", Assert.Single(version.Graph!.Nodes).Kind);

        var restored = await test.Client.Automations.RestoreVersionAsync("auto_1", 2);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/auto_1/versions/2/restore", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(4, restored.Version);
    }

    [Fact]
    public async Task Runs_list_and_get()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":77,"status":"deferred","current_step":1,"started_at":"2026-09-01T00:00:00Z","deferred_until":"2026-09-01T09:00:00Z","defer_reason":"schedule_window","defer_count":1}],"meta":{"page":1,"per_page":20,"total":1,"total_pages":1}}""")
            .Json("""{"id":77,"automation_id":"auto_1","status":"completed","started_at":"2026-09-01T00:00:00Z","completed_at":"2026-09-01T00:00:05Z","logs":[{"id":1,"step_position":1,"node_id":"n2","status":"completed","duration_ms":120,"output_snapshot":{"post_id":"post_1"}}]}""");
        using var test = new TestClient(handler);

        var runs = await test.Client.Automations.ListRunsAsync("auto_1", perPage: 20);
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/auto_1/runs?per_page=20", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("schedule_window", Assert.Single(runs).DeferReason);

        var run = await test.Client.Automations.GetRunAsync("auto_1", 77);
        Assert.Equal($"{TestClient.BaseUrl}/v1/automations/auto_1/runs/77", handler.LastRequest.RequestUri!.ToString());
        var log = Assert.Single(run.Logs!);
        Assert.Equal(120, log.DurationMs);
        Assert.Equal("post_1", log.OutputSnapshot!.Value.GetProperty("post_id").GetString());
    }
}
