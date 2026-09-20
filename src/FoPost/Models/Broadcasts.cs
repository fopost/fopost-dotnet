using System.Collections;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>Where a broadcast stands.</summary>
public static class BroadcastStatuses
{
    public const string Draft = "draft";
    public const string Scheduled = "scheduled";
    public const string Sending = "sending";
    public const string Sent = "sent";
    public const string Cancelled = "cancelled";
}

/// <summary>What became of one recipient's message.</summary>
public static class RecipientStatuses
{
    public const string Pending = "pending";
    public const string Sent = "sent";
    public const string Skipped = "skipped";
    public const string Failed = "failed";
}

/// <summary>Why a recipient was skipped instead of written to.</summary>
public static class SkipReasons
{
    /// <summary>
    /// The network's messaging window had shut, so nothing was attempted. Messenger and
    /// Instagram take a business-initiated message only within 24 hours of the contact's
    /// last one.
    /// </summary>
    public const string WindowClosed = "window_closed";

    /// <summary>This contact never wrote to the sending account.</summary>
    public const string NoConversation = "no_conversation";

    /// <summary>The account's network takes no messages.</summary>
    public const string UnsupportedPlatform = "unsupported_platform";
}

/// <summary>Whether a sequence fires at all.</summary>
public static class SequenceStatuses
{
    public const string Active = "active";
    public const string Paused = "paused";
}

/// <summary>Where a contact stands on a sequence.</summary>
public static class EnrollmentStatuses
{
    public const string Active = "active";
    public const string Completed = "completed";
    public const string Stopped = "stopped";
    public const string Failed = "failed";
}

/// <summary>Operators an <see cref="AudienceField"/> clause may use.</summary>
public static class AudienceOperators
{
    public const string Is = "is";
    public const string IsNot = "is_not";
    public const string Contains = "contains";
    public const string IsSet = "is_set";
    public const string IsNotSet = "is_not_set";
}

/// <summary>One custom-field clause in an audience filter.</summary>
public sealed class AudienceField : FoPostModel
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>One of <see cref="AudienceOperators"/>. Null means <c>is</c>.</summary>
    [JsonPropertyName("op")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Op { get; set; }

    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }

    public static AudienceField Of(string key, string op, string? value = null) =>
        new() { Key = key, Op = op, Value = value };
}

/// <summary>
/// Who a broadcast or an enrollment resolves to, expressed over contacts. Every clause
/// narrows: a contact has to match all of them.
/// </summary>
public sealed class AudienceFilter : FoPostModel
{
    /// <summary>Contacts with a handle on at least one of these networks.</summary>
    [JsonPropertyName("platforms")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Platforms { get; set; }

    [JsonPropertyName("label_ids")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? LabelIds { get; set; }

    /// <summary>One of <see cref="ContactSources"/>.</summary>
    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; set; }

    [JsonPropertyName("fields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<AudienceField>? Fields { get; set; }

    /// <summary>Contacts with a handle on any of these networks.</summary>
    public static AudienceFilter OnPlatforms(params string[] platforms) =>
        new() { Platforms = platforms };
}

/// <summary>
/// What became of a broadcast's recipients, by status. <c>Skipped</c> is usually the
/// messaging window doing its job.
/// </summary>
public sealed class BroadcastCounts : FoPostModel
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("sent")]
    public int Sent { get; set; }

    [JsonPropertyName("skipped")]
    public int Skipped { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    [JsonPropertyName("pending")]
    public int Pending { get; set; }
}

/// <summary>One message, sent into conversations the workspace already has.</summary>
public sealed class Broadcast : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Internal only; never sent to anyone.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("audience")]
    public AudienceFilter? Audience { get; set; }

    /// <summary>One of <see cref="BroadcastStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = BroadcastStatuses.Draft;

    [JsonPropertyName("scheduled_at")]
    public DateTimeOffset? ScheduledAt { get; set; }

    [JsonPropertyName("sent_at")]
    public DateTimeOffset? SentAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("counts")]
    public BroadcastCounts? Counts { get; set; }

    /// <summary>Set only on a listing that spans workspaces.</summary>
    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }
}

/// <summary>One page of broadcasts: its rows plus the pagination block.</summary>
public sealed class BroadcastPage : IReadOnlyList<Broadcast>
{
    public BroadcastPage(IReadOnlyList<Broadcast> items, ContactPageMeta pagination)
    {
        Items = items;
        Pagination = pagination;
    }

    public IReadOnlyList<Broadcast> Items { get; }

    public ContactPageMeta Pagination { get; }

    public int Count => Items.Count;

    public Broadcast this[int index] => Items[index];

    public IEnumerator<Broadcast> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>One contact on one broadcast, and what became of their message.</summary>
public sealed class BroadcastRecipient : FoPostModel
{
    [JsonPropertyName("contact_id")]
    public string ContactId { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>One of <see cref="RecipientStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = RecipientStatuses.Pending;

    /// <summary>
    /// One of <see cref="SkipReasons"/>, set when the status is skipped.
    /// <c>window_closed</c> means nothing was attempted.
    /// </summary>
    [JsonPropertyName("skip_reason")]
    public string? SkipReason { get; set; }

    [JsonPropertyName("sent_at")]
    public DateTimeOffset? SentAt { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>One page of a broadcast's recipients.</summary>
public sealed class RecipientPage : IReadOnlyList<BroadcastRecipient>
{
    public RecipientPage(IReadOnlyList<BroadcastRecipient> items, ContactPageMeta pagination)
    {
        Items = items;
        Pagination = pagination;
    }

    public IReadOnlyList<BroadcastRecipient> Items { get; }

    public ContactPageMeta Pagination { get; }

    public int Count => Items.Count;

    public BroadcastRecipient this[int index] => Items[index];

    public IEnumerator<BroadcastRecipient> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// What a send started. <c>Recipients</c> is how many contacts matched, not how many will
/// be messaged — the messaging window decides that.
/// </summary>
public sealed class BroadcastSent : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("recipients")]
    public int Recipients { get; set; }
}

/// <summary>
/// One message and how long after the previous step it goes out. <c>DelayHours</c> on the
/// first step is measured from the enrollment, so 0 means straight away.
/// </summary>
public sealed class SequenceStep : FoPostModel
{
    [JsonPropertyName("delay_hours")]
    public double DelayHours { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("media_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? MediaId { get; set; }

    public static SequenceStep Of(double delayHours, string text) =>
        new() { DelayHours = delayHours, Text = text };
}

/// <summary>Where a sequence's enrollments stand, by status.</summary>
public sealed class EnrollmentCounts : FoPostModel
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("active")]
    public int Active { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("stopped")]
    public int Stopped { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }
}

/// <summary>A series of messages, each a delay after the one before.</summary>
public sealed class Sequence : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string? AccountId { get; set; }

    [JsonPropertyName("steps")]
    public IReadOnlyList<SequenceStep> Steps { get; set; } = Array.Empty<SequenceStep>();

    /// <summary>
    /// One of <see cref="SequenceStatuses"/>. A paused sequence fires nothing.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = SequenceStatuses.Active;

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("enrollments")]
    public EnrollmentCounts? Enrollments { get; set; }

    /// <summary>Set only on a listing that spans workspaces.</summary>
    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }
}

/// <summary>One page of sequences.</summary>
public sealed class SequencePage : IReadOnlyList<Sequence>
{
    public SequencePage(IReadOnlyList<Sequence> items, ContactPageMeta pagination)
    {
        Items = items;
        Pagination = pagination;
    }

    public IReadOnlyList<Sequence> Items { get; }

    public ContactPageMeta Pagination { get; }

    public int Count => Items.Count;

    public Sequence this[int index] => Items[index];

    public IEnumerator<Sequence> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>One contact walking one sequence.</summary>
public sealed class Enrollment : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("contact_id")]
    public string ContactId { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    /// <summary>Steps already sent, so also the index of the next one.</summary>
    [JsonPropertyName("step")]
    public int Step { get; set; }

    [JsonPropertyName("next_at")]
    public DateTimeOffset? NextAt { get; set; }

    /// <summary>One of <see cref="EnrollmentStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = EnrollmentStatuses.Active;

    [JsonPropertyName("last_sent_at")]
    public DateTimeOffset? LastSentAt { get; set; }

    /// <summary>On a skipped step, the reason it was skipped.</summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>One page of a sequence's enrollments.</summary>
public sealed class EnrollmentPage : IReadOnlyList<Enrollment>
{
    public EnrollmentPage(IReadOnlyList<Enrollment> items, ContactPageMeta pagination)
    {
        Items = items;
        Pagination = pagination;
    }

    public IReadOnlyList<Enrollment> Items { get; }

    public ContactPageMeta Pagination { get; }

    public int Count => Items.Count;

    public Enrollment this[int index] => Items[index];

    public IEnumerator<Enrollment> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>How many contacts a call put on the sequence.</summary>
public sealed class Enrolled : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("enrolled")]
    public int Count { get; set; }
}

/// <summary>How many enrollments a call stopped.</summary>
public sealed class Unenrolled : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("stopped")]
    public int Stopped { get; set; }
}
