using System.Text.Json.Serialization;

namespace FoPost;

// The automation request bodies are camelCase and are serialised straight from these objects;
// a null member is left out, so an update stays partial.

/// <summary>A node in the graph being written.</summary>
public sealed class AutomationNodeInput
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>One of <see cref="AutomationNodeKinds"/>.</summary>
    [JsonPropertyName("kind")]
    public required string Kind { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("config")]
    public IDictionary<string, object?>? Config { get; set; }

    [JsonPropertyName("layout")]
    public AutomationLayoutInput? Layout { get; set; }
}

public sealed class AutomationLayoutInput
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }
}

public sealed class AutomationEdgeInput
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("source")]
    public required string Source { get; set; }

    [JsonPropertyName("sourceHandle")]
    public string? SourceHandle { get; set; }

    [JsonPropertyName("target")]
    public required string Target { get; set; }
}

public sealed class AutomationGraphInput
{
    [JsonPropertyName("nodes")]
    public IList<AutomationNodeInput> Nodes { get; set; } = new List<AutomationNodeInput>();

    [JsonPropertyName("edges")]
    public IList<AutomationEdgeInput> Edges { get; set; } = new List<AutomationEdgeInput>();
}

/// <summary>A linear step, for an automation written without a graph.</summary>
public sealed class AutomationStepInput
{
    /// <summary>One of <see cref="AutomationNodeKinds"/> other than <c>trigger</c>.</summary>
    [JsonPropertyName("actionType")]
    public required string ActionType { get; set; }

    [JsonPropertyName("actionConfig")]
    public IDictionary<string, object?>? ActionConfig { get; set; }
}

/// <summary>The body of <see cref="Resources.AutomationsResource.CreateAsync"/>.</summary>
public sealed class CreateAutomationOptions
{
    [JsonPropertyName("workspaceId")]
    public required string WorkspaceId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>One of <see cref="AutomationTriggerTypes"/>.</summary>
    [JsonPropertyName("triggerType")]
    public required string TriggerType { get; set; }

    [JsonPropertyName("triggerConfig")]
    public IDictionary<string, object?>? TriggerConfig { get; set; }

    /// <summary>Linear steps; use <see cref="Graph"/> for branches.</summary>
    [JsonPropertyName("steps")]
    public IList<AutomationStepInput>? Steps { get; set; }

    [JsonPropertyName("graph")]
    public AutomationGraphInput? Graph { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }
}

/// <summary>The body of <see cref="Resources.AutomationsResource.UpdateAsync"/>. Only the fields you set are sent.</summary>
public sealed class UpdateAutomationOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>One of <see cref="AutomationTriggerTypes"/>.</summary>
    [JsonPropertyName("triggerType")]
    public string? TriggerType { get; set; }

    [JsonPropertyName("triggerConfig")]
    public IDictionary<string, object?>? TriggerConfig { get; set; }

    [JsonPropertyName("steps")]
    public IList<AutomationStepInput>? Steps { get; set; }

    [JsonPropertyName("graph")]
    public AutomationGraphInput? Graph { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }
}
