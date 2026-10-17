namespace FoPost;

/// <summary>Filters for <see cref="Resources.ContactsResource.ListAsync"/>.</summary>
public sealed class ListContactsOptions
{
    /// <summary>Omit to span every workspace the key can reach.</summary>
    public string? WorkspaceId { get; set; }

    /// <summary>Matches a display name or any of their handles.</summary>
    public string? Search { get; set; }

    /// <summary>One of <see cref="Platforms"/>.</summary>
    public string? Platform { get; set; }

    /// <summary>One of <see cref="ContactSources"/>.</summary>
    public string? Source { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}

/// <summary>The body of <see cref="Resources.ContactsResource.CreateAsync"/>.</summary>
public sealed class CreateContactOptions
{
    public required string WorkspaceId { get; set; }

    /// <summary>At least one. The first decides which contact this folds into.</summary>
    public required IReadOnlyList<ContactChannel> Channels { get; set; }

    public string? DisplayName { get; set; }

    public string? Note { get; set; }

    public IReadOnlyDictionary<string, string>? Fields { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.ContactsResource.UpdateAsync"/>. Only what is set is
/// sent; a field whose value is null is cleared rather than left alone.
/// </summary>
public sealed class UpdateContactOptions
{
    public Optional<string?> DisplayName { get; set; }

    public Optional<IReadOnlyList<ContactChannel>> Channels { get; set; }

    public Optional<string?> Note { get; set; }

    /// <summary>A value of null clears that field.</summary>
    public IReadOnlyDictionary<string, string?>? Fields { get; set; }
}

/// <summary>The body of <see cref="Resources.ContactsResource.CreateFieldAsync"/>.</summary>
public sealed class CreateContactFieldOptions
{
    /// <summary>Lower-case letters, digits and underscores, starting with a letter.</summary>
    public required string Key { get; set; }

    public required string Name { get; set; }

    /// <summary>One of <see cref="ContactFieldTypes"/>. Defaults to text.</summary>
    public string Type { get; set; } = ContactFieldTypes.Text;

    /// <summary>Required when <see cref="Type"/> is <c>select</c>.</summary>
    public IReadOnlyList<string>? Options { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.ContactsResource.UpdateFieldAsync"/>. The key and the
/// type are fixed once created; the name and options are not.
/// </summary>
public sealed class UpdateContactFieldOptions
{
    public Optional<string> Name { get; set; }

    public Optional<IReadOnlyList<string>> Options { get; set; }

    public Optional<int> Position { get; set; }
}

/// <summary>
/// Filters for <see cref="Resources.ContactsResource.ConversationAnalyticsAsync"/>.
/// </summary>
public sealed class ConversationAnalyticsOptions
{
    public string? WorkspaceId { get; set; }

    public string? AccountId { get; set; }

    /// <summary>The reporting period, 1 to 365. Defaults to 7.</summary>
    public int? Days { get; set; }

    /// <summary>One of <see cref="ConversationSorts"/>.</summary>
    public string? Sort { get; set; }

    public int? Page { get; set; }

    public int? PerPage { get; set; }
}
