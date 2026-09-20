using System.Collections;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>What first created a contact row.</summary>
public static class ContactSources
{
    public const string Inbox = "inbox";
    public const string Radar = "radar";
    public const string Import = "import";
}

/// <summary>What a custom field accepts.</summary>
public static class ContactFieldTypes
{
    public const string Text = "text";
    public const string Number = "number";
    public const string Date = "date";
    public const string Select = "select";
    public const string Boolean = "boolean";
}

/// <summary>How a per-conversation report is ordered.</summary>
public static class ConversationSorts
{
    public const string Volume = "volume";
    public const string Slowest = "slowest";
    public const string Recent = "recent";
}

/// <summary>
/// One handle on one network. <c>Handle</c> is lower-cased with no leading <c>@</c>.
/// <c>ExternalId</c> is the platform's own id for this person when the network gave us
/// one, and it is what a merge prefers: a handle can be changed, an id cannot.
/// </summary>
public sealed class ContactChannel : FoPostModel
{
    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("handle")]
    public string Handle { get; set; } = string.Empty;

    [JsonPropertyName("externalId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExternalId { get; set; }

    /// <summary>A channel without a platform id, which is all most writes need.</summary>
    public static ContactChannel Of(string platform, string handle) =>
        new() { Platform = platform, Handle = handle };
}

/// <summary>A workspace label put on a contact.</summary>
public sealed class ContactLabel : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string? Color { get; set; }
}

/// <summary>One person, however many handles they write from.</summary>
public sealed class Contact : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("channels")]
    public IReadOnlyList<ContactChannel> Channels { get; set; } = Array.Empty<ContactChannel>();

    /// <summary>One of <see cref="ContactSources"/> — what first created the row.</summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = ContactSources.Inbox;

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("first_seen_at")]
    public DateTimeOffset? FirstSeenAt { get; set; }

    [JsonPropertyName("last_seen_at")]
    public DateTimeOffset? LastSeenAt { get; set; }

    /// <summary>Custom field values, keyed by field key.</summary>
    [JsonPropertyName("fields")]
    public IReadOnlyDictionary<string, string> Fields { get; set; } =
        new Dictionary<string, string>();

    [JsonPropertyName("labels")]
    public IReadOnlyList<ContactLabel> Labels { get; set; } = Array.Empty<ContactLabel>();

    /// <summary>Set only on a listing that spans workspaces.</summary>
    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }
}

/// <summary>The pagination block a contacts listing returns beside its data.</summary>
public sealed class ContactPageMeta : FoPostModel
{
    [JsonPropertyName("page")]
    public int? Page { get; set; }

    [JsonPropertyName("per_page")]
    public int? PerPage { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }
}

/// <summary>One page of contacts: its rows plus the pagination block.</summary>
public sealed class ContactPage : IReadOnlyList<Contact>
{
    public ContactPage(IReadOnlyList<Contact> items, ContactPageMeta pagination)
    {
        Items = items;
        Pagination = pagination;
    }

    public IReadOnlyList<Contact> Items { get; }

    public ContactPageMeta Pagination { get; }

    public int Count => Items.Count;

    public Contact this[int index] => Items[index];

    public IEnumerator<Contact> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// One thread a contact appears in. <c>Key</c> is how the inbox groups it: the DM thread
/// id, else the post the comments hang off, else the handle.
/// </summary>
public sealed class ContactConversation : FoPostModel
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("account_username")]
    public string? AccountUsername { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public int Messages { get; set; }

    [JsonPropertyName("received")]
    public int Received { get; set; }

    [JsonPropertyName("sent")]
    public int Sent { get; set; }

    [JsonPropertyName("last_message_at")]
    public DateTimeOffset? LastMessageAt { get; set; }

    /// <summary>An inbox item id, readable through the inbox endpoints.</summary>
    [JsonPropertyName("last_item_id")]
    public string? LastItemId { get; set; }
}

/// <summary>One CSV row the import could not read.</summary>
public sealed class ContactImportSkip : FoPostModel
{
    [JsonPropertyName("row")]
    public int Row { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>What a CSV import did.</summary>
public sealed class ContactImportResult : FoPostModel
{
    [JsonPropertyName("created")]
    public int Created { get; set; }

    /// <summary>Rows that folded into a contact already on file.</summary>
    [JsonPropertyName("merged")]
    public int Merged { get; set; }

    [JsonPropertyName("skipped")]
    public IReadOnlyList<ContactImportSkip> Skipped { get; set; } =
        Array.Empty<ContactImportSkip>();

    /// <summary>
    /// Columns that matched neither a reserved field nor a custom field. They are reported,
    /// never stored.
    /// </summary>
    [JsonPropertyName("unknownColumns")]
    public IReadOnlyList<string> UnknownColumns { get; set; } = Array.Empty<string>();
}

/// <summary>
/// A column the workspace invented. <c>Key</c> is the machine name and the CSV column
/// header, fixed once created; the name and options are not.
/// </summary>
public sealed class ContactField : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>One of <see cref="ContactFieldTypes"/>.</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = ContactFieldTypes.Text;

    /// <summary>Allowed values when <see cref="Type"/> is <c>select</c>.</summary>
    [JsonPropertyName("options")]
    public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();

    [JsonPropertyName("position")]
    public int Position { get; set; }
}

/// <summary>How one thread performed over the period.</summary>
public sealed class ConversationAnalyticsRow : FoPostModel
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = string.Empty;

    [JsonPropertyName("received")]
    public int Received { get; set; }

    [JsonPropertyName("sent")]
    public int Sent { get; set; }

    [JsonPropertyName("answered")]
    public int Answered { get; set; }

    [JsonPropertyName("open")]
    public int Open { get; set; }

    /// <summary>Null when the thread was never answered.</summary>
    [JsonPropertyName("medianResponseMinutes")]
    public double? MedianResponseMinutes { get; set; }

    [JsonPropertyName("firstMessageAt")]
    public DateTimeOffset? FirstMessageAt { get; set; }

    [JsonPropertyName("lastMessageAt")]
    public DateTimeOffset? LastMessageAt { get; set; }
}

/// <summary>Inbox analytics broken out per thread.</summary>
public sealed class ConversationAnalytics : FoPostModel
{
    [JsonPropertyName("conversations")]
    public IReadOnlyList<ConversationAnalyticsRow> Conversations { get; set; } =
        Array.Empty<ConversationAnalyticsRow>();

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("perPage")]
    public int PerPage { get; set; }
}
