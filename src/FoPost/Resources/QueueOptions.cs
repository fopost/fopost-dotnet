namespace FoPost;

/// <summary>The body of <see cref="Resources.QueueResource.CreateSlotAsync"/>.</summary>
public sealed class CreateQueueSlotOptions
{
    public required string WorkspaceId { get; set; }

    /// <summary>Limit the slot to one account; omitted, it serves the whole workspace.</summary>
    public string? AccountId { get; set; }

    /// <summary>0 (Sunday) to 6 (Saturday).</summary>
    public required int Weekday { get; set; }

    /// <summary><c>HH:mm</c>.</summary>
    public required string Time { get; set; }

    public required string Timezone { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.QueueResource.UpdateSlotAsync"/>. Only the fields you set
/// are sent.
/// </summary>
public sealed class UpdateQueueSlotOptions
{
    public Optional<string?> AccountId { get; set; }

    public Optional<int> Weekday { get; set; }

    public Optional<string> Time { get; set; }

    public Optional<string> Timezone { get; set; }
}

/// <summary>Scope for <see cref="Resources.QueueResource.PreviewAsync"/>.</summary>
public sealed class PreviewQueueOptions
{
    public string? AccountId { get; set; }

    /// <summary>How many upcoming slot times to return.</summary>
    public int? Count { get; set; }
}
