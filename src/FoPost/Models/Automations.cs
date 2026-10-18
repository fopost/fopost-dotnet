using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>What starts an automation.</summary>
public static class AutomationTriggerTypes
{
    public const string ContentCreated = "content_created";
    public const string CrossPost = "cross_post";
    public const string RssFeed = "rss_feed";
    public const string ApiWebhook = "api_webhook";
    public const string Schedule = "schedule";
    public const string CompetitorPost = "competitor_post";
    public const string PostOutperformed = "post_outperformed";
    public const string InboxTopic = "inbox_topic";
    public const string InboxMessage = "inbox_message";
    public const string RadarMention = "radar_mention";
}

/// <summary>The kinds of node (and step) an automation runs.</summary>
public static class AutomationNodeKinds
{
    public const string Trigger = "trigger";
    public const string Generate = "generate";
    public const string Transform = "transform";
    public const string Delay = "delay";
    public const string Publish = "publish";
    public const string Condition = "condition";
    public const string SetVariable = "set_variable";
    public const string Webhook = "webhook";
    public const string AbSplit = "ab_split";
    public const string Ai = "ai";
    public const string SendMessage = "send_message";
    public const string WaitForReply = "wait_for_reply";
    public const string Handoff = "handoff";
    public const string AddLabel = "add_label";
    public const string RemoveLabel = "remove_label";
    public const string End = "end";
}

/// <summary>An automation: a trigger and the graph of steps it runs.</summary>
public sealed class Automation : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>One of <see cref="AutomationTriggerTypes"/>.</summary>
    [JsonPropertyName("trigger_type")]
    public string? TriggerType { get; set; }

    [JsonPropertyName("trigger_config")]
    public JsonElement? TriggerConfig { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("schedule_window")]
    public AutomationScheduleWindow? ScheduleWindow { get; set; }

    [JsonPropertyName("max_per_day")]
    public JsonElement? MaxPerDay { get; set; }

    [JsonPropertyName("credit_ceiling")]
    public int? CreditCeiling { get; set; }

    [JsonPropertyName("approval_mode")]
    public bool? ApprovalMode { get; set; }

    [JsonPropertyName("last_triggered_at")]
    public DateTimeOffset? LastTriggeredAt { get; set; }

    [JsonPropertyName("run_count")]
    public int? RunCount { get; set; }

    /// <summary>The webhook-trigger secret. Only returned on create.</summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    [JsonPropertyName("graph")]
    public AutomationGraph? Graph { get; set; }

    [JsonPropertyName("steps")]
    public IList<AutomationStep>? Steps { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>The hours an automation may run in.</summary>
public sealed class AutomationScheduleWindow : FoPostModel
{
    [JsonPropertyName("days")]
    public IList<int> Days { get; set; } = new List<int>();

    [JsonPropertyName("start")]
    public string? Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }
}

public sealed class AutomationGraph : FoPostModel
{
    [JsonPropertyName("nodes")]
    public IList<AutomationNode> Nodes { get; set; } = new List<AutomationNode>();

    [JsonPropertyName("edges")]
    public IList<AutomationEdge> Edges { get; set; } = new List<AutomationEdge>();
}

public sealed class AutomationNode : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>One of <see cref="AutomationNodeKinds"/>.</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("config")]
    public JsonElement? Config { get; set; }

    [JsonPropertyName("position")]
    public int? Position { get; set; }

    [JsonPropertyName("layout")]
    public AutomationLayout? Layout { get; set; }
}

/// <summary>Where a node sits on the canvas.</summary>
public sealed class AutomationLayout : FoPostModel
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }
}

public sealed class AutomationEdge : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>Which output of the source the edge leaves from, on a condition or split.</summary>
    [JsonPropertyName("source_handle")]
    public string? SourceHandle { get; set; }

    [JsonPropertyName("target")]
    public string Target { get; set; } = string.Empty;
}

/// <summary>One step, in run order, compiled from the graph.</summary>
public sealed class AutomationStep : FoPostModel
{
    [JsonPropertyName("position")]
    public int? Position { get; set; }

    [JsonPropertyName("node_id")]
    public string? NodeId { get; set; }

    [JsonPropertyName("action_type")]
    public string? ActionType { get; set; }

    [JsonPropertyName("action_config")]
    public JsonElement? ActionConfig { get; set; }
}

/// <summary>Counts across every automation the credential can see.</summary>
public sealed class AutomationStats : FoPostModel
{
    [JsonPropertyName("total_automations")]
    public int TotalAutomations { get; set; }

    [JsonPropertyName("active_automations")]
    public int ActiveAutomations { get; set; }

    [JsonPropertyName("total_runs_24h")]
    public int TotalRuns24h { get; set; }

    [JsonPropertyName("runs_by_status")]
    public IDictionary<string, int> RunsByStatus { get; set; } = new Dictionary<string, int>();

    [JsonPropertyName("recent_runs")]
    public IList<AutomationRunSummary> RecentRuns { get; set; } = new List<AutomationRunSummary>();
}

public sealed class AutomationRunSummary : FoPostModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("automation_id")]
    public string? AutomationId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("started_at")]
    public DateTimeOffset? StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }
}

/// <summary>The answer to a toggle.</summary>
public sealed class AutomationToggle : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("active")]
    public bool Active { get; set; }
}

/// <summary>A saved version of an automation's graph. The listing carries the diff, the detail the graph.</summary>
public sealed class AutomationVersion : FoPostModel
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("created_by")]
    public ActivityActor? CreatedBy { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("node_count")]
    public int? NodeCount { get; set; }

    [JsonPropertyName("nodes_added")]
    public IList<AutomationNodeRef>? NodesAdded { get; set; }

    [JsonPropertyName("nodes_removed")]
    public IList<AutomationNodeRef>? NodesRemoved { get; set; }

    [JsonPropertyName("graph")]
    public AutomationGraph? Graph { get; set; }
}

public sealed class AutomationNodeRef : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}

/// <summary>What a restore put back: the new version number, its graph and the compiled steps.</summary>
public sealed class AutomationRestore : FoPostModel
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("graph")]
    public AutomationGraph Graph { get; set; } = new();

    [JsonPropertyName("steps")]
    public IList<AutomationStep> Steps { get; set; } = new List<AutomationStep>();
}

/// <summary>One execution of an automation.</summary>
public sealed class AutomationRun : FoPostModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("automation_id")]
    public string? AutomationId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("current_step")]
    public int? CurrentStep { get; set; }

    [JsonPropertyName("current_node_id")]
    public string? CurrentNodeId { get; set; }

    [JsonPropertyName("waiting_until")]
    public DateTimeOffset? WaitingUntil { get; set; }

    [JsonPropertyName("trigger_event")]
    public JsonElement? TriggerEvent { get; set; }

    [JsonPropertyName("context")]
    public JsonElement? Context { get; set; }

    [JsonPropertyName("initial_context")]
    public JsonElement? InitialContext { get; set; }

    [JsonPropertyName("replay_of_run_id")]
    public long? ReplayOfRunId { get; set; }

    [JsonPropertyName("deferred_until")]
    public DateTimeOffset? DeferredUntil { get; set; }

    [JsonPropertyName("defer_reason")]
    public string? DeferReason { get; set; }

    [JsonPropertyName("defer_detail")]
    public string? DeferDetail { get; set; }

    [JsonPropertyName("defer_count")]
    public int? DeferCount { get; set; }

    [JsonPropertyName("started_at")]
    public DateTimeOffset? StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    /// <summary>Per-step records. Only on <see cref="Resources.AutomationsResource.GetRunAsync"/>.</summary>
    [JsonPropertyName("logs")]
    public IList<AutomationRunLog>? Logs { get; set; }
}

public sealed class AutomationRunLog : FoPostModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("step_position")]
    public int? StepPosition { get; set; }

    [JsonPropertyName("node_id")]
    public string? NodeId { get; set; }

    [JsonPropertyName("handle")]
    public string? Handle { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("input_snapshot")]
    public JsonElement? InputSnapshot { get; set; }

    [JsonPropertyName("output_snapshot")]
    public JsonElement? OutputSnapshot { get; set; }

    [JsonPropertyName("started_at")]
    public DateTimeOffset? StartedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }
}
