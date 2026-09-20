namespace FoPost;

/// <summary>Filters for <see cref="Resources.BroadcastsResource.ListAsync"/>.</summary>
public sealed class ListBroadcastsOptions
{
    /// <summary>Omit to span every workspace the key can reach.</summary>
    public string? WorkspaceId { get; set; }

    /// <summary>One of <see cref="BroadcastStatuses"/>.</summary>
    public string? Status { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.BroadcastsResource.CreateAsync"/>. Creating never
/// sends: set <see cref="ScheduledAt"/> to have it go out on its own, or call
/// <see cref="Resources.BroadcastsResource.SendAsync"/>.
/// </summary>
public sealed class CreateBroadcastOptions
{
    public required string WorkspaceId { get; set; }

    /// <summary>The connected account the messages go out from.</summary>
    public required string AccountId { get; set; }

    /// <summary>Internal only; never sent to anyone.</summary>
    public required string Name { get; set; }

    public required string Text { get; set; }

    public string? MediaId { get; set; }

    /// <summary>Omit to reach every contact in the workspace.</summary>
    public AudienceFilter? Audience { get; set; }

    public DateTimeOffset? ScheduledAt { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.BroadcastsResource.UpdateAsync"/>. Only what is set is
/// sent, and only a draft or scheduled broadcast can be edited.
/// </summary>
public sealed class UpdateBroadcastOptions
{
    public Optional<string> Name { get; set; }

    public Optional<string> Text { get; set; }

    public Optional<string?> MediaId { get; set; }

    public Optional<AudienceFilter> Audience { get; set; }

    public Optional<DateTimeOffset?> ScheduledAt { get; set; }
}

/// <summary>Filters for <see cref="Resources.BroadcastsResource.RecipientsAsync"/>.</summary>
public sealed class ListRecipientsOptions
{
    /// <summary>One of <see cref="RecipientStatuses"/>.</summary>
    public string? Status { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}

/// <summary>Filters for <see cref="Resources.SequencesResource.ListAsync"/>.</summary>
public sealed class ListSequencesOptions
{
    public string? WorkspaceId { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.SequencesResource.CreateAsync"/>. Creating one enrolls
/// nobody.
/// </summary>
public sealed class CreateSequenceOptions
{
    public required string WorkspaceId { get; set; }

    public required string AccountId { get; set; }

    public required string Name { get; set; }

    /// <summary>In order; each step's delay is counted from the one before.</summary>
    public required IReadOnlyList<SequenceStep> Steps { get; set; }

    /// <summary>One of <see cref="SequenceStatuses"/>; the default is active.</summary>
    public string? Status { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.SequencesResource.UpdateAsync"/>. Pausing stops every
/// enrollment from firing without ending any of them.
/// </summary>
public sealed class UpdateSequenceOptions
{
    public Optional<string> Name { get; set; }

    public Optional<IReadOnlyList<SequenceStep>> Steps { get; set; }

    public Optional<string> Status { get; set; }
}

/// <summary>
/// Who to enroll: named contacts, or the audience they are drawn from. One of the two.
/// </summary>
public sealed class EnrollOptions
{
    public IReadOnlyList<string>? ContactIds { get; set; }

    public AudienceFilter? Audience { get; set; }
}

/// <summary>Pagination for <see cref="Resources.SequencesResource.EnrollmentsAsync"/>.</summary>
public sealed class ListEnrollmentsOptions
{
    public int? Page { get; set; }

    public int? PerPage { get; set; }
}
