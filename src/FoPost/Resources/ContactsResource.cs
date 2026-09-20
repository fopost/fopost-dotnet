using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Contacts</c> — the people behind the inbox, and the fields a workspace keeps
/// about them.
/// </summary>
/// <remarks>
/// A contact is one person however many handles they write from. An inbound inbox item
/// files its author, a reply files whoever you answered, and both fold into whatever is
/// already on file, so the same person never becomes two rows.
/// <para>
/// Every method needs the <c>inbox</c> scope, except
/// <see cref="ConversationAnalyticsAsync"/>, which needs <c>analytics</c>.
/// </para>
/// </remarks>
public sealed class ContactsResource
{
    private readonly FoPostHttpClient _http;

    internal ContactsResource(FoPostHttpClient http) => _http = http;

    /// <summary>
    /// One page of contacts, most recently active first. Omit the workspace to span every
    /// workspace the key can reach; each contact then carries <c>WorkspaceId</c>.
    /// </summary>
    public async Task<ContactPage> ListAsync(
        ListContactsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options?.WorkspaceId,
            ["search"] = options?.Search,
            ["platform"] = options?.Platform,
            ["source"] = options?.Source,
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
        };
        var body = await _http.GetAsync("/v1/contacts", query, cancellationToken).ConfigureAwait(false);
        var items = ToList<Contact>(FoPostHttpClient.Unwrap(body));
        var pagination = body is JsonObject obj &&
            obj.TryGetPropertyValue("pagination", out var meta) &&
            meta is JsonObject
                ? Require<ContactPageMeta>(meta)
                : new ContactPageMeta();
        return new ContactPage(items, pagination);
    }

    /// <summary>
    /// One contact. A contact in a workspace the key cannot reach answers 404, exactly as
    /// an id that never existed does.
    /// </summary>
    public async Task<Contact> GetAsync(string contactId, CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync($"/v1/contacts/{contactId}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Contact>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// File a contact. It folds into the contact that already holds the first channel, so
    /// this cannot duplicate someone the inbox has already met.
    /// </summary>
    public async Task<Contact> CreateAsync(
        CreateContactOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["channels"] = options.Channels,
            ["display_name"] = options.DisplayName,
            ["note"] = options.Note,
            ["fields"] = options.Fields,
        };
        var response = await _http.PostAsync("/v1/contacts", Compact(body), cancellationToken)
            .ConfigureAwait(false);
        return Require<Contact>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Patch a contact. Only what the options set is sent.</summary>
    public async Task<Contact> UpdateAsync(
        string contactId,
        UpdateContactOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.DisplayName.IsSet)
        {
            body["display_name"] = options.DisplayName.Value;
        }
        if (options.Channels.IsSet)
        {
            body["channels"] = options.Channels.Value;
        }
        if (options.Note.IsSet)
        {
            body["note"] = options.Note.Value;
        }
        if (options.Fields is not null)
        {
            body["fields"] = options.Fields;
        }

        var response = await _http
            .RequestAsync(new HttpMethod("PATCH"), $"/v1/contacts/{contactId}", body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Contact>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Remove a contact and its field values. The messages they sent stay in the inbox, so
    /// a later message files them again.
    /// </summary>
    public async Task DeleteAsync(string contactId, CancellationToken cancellationToken = default) =>
        await _http.DeleteAsync($"/v1/contacts/{contactId}", null, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// The threads one contact appears in, newest first. Matched on their channels, so a
    /// contact merged from two handles brings both threads with it.
    /// </summary>
    public async Task<IReadOnlyList<ContactConversation>> ConversationsAsync(
        string contactId,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["limit"] = limit };
        var body = await _http
            .GetAsync($"/v1/contacts/{contactId}/conversations", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<ContactConversation>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Import contacts from CSV text. <c>platform</c> and <c>handle</c> are required
    /// columns; any other column is read as a custom field key, and one matching no field
    /// comes back in <c>UnknownColumns</c> rather than being stored.
    /// </summary>
    public async Task<ContactImportResult> ImportAsync(
        string workspaceId,
        string csv,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["workspace_id"] = workspaceId, ["csv"] = csv };
        var response = await _http.PostAsync("/v1/contacts/import", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<ContactImportResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The columns this workspace keeps about its contacts, in display order.</summary>
    public async Task<IReadOnlyList<ContactField>> ListFieldsAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/contacts/fields", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<ContactField>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Add a custom field. A duplicate key answers 409.</summary>
    public async Task<ContactField> CreateFieldAsync(
        string workspaceId,
        CreateContactFieldOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = new Dictionary<string, object?>
        {
            ["key"] = options.Key,
            ["name"] = options.Name,
            ["type"] = options.Type,
            ["options"] = options.Options ?? Array.Empty<string>(),
        };
        var response = await _http
            .RequestAsync(HttpMethod.Post, "/v1/contacts/fields", body, query, cancellationToken)
            .ConfigureAwait(false);
        return Require<ContactField>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Rename a field, or change its options or position.</summary>
    public async Task<ContactField> UpdateFieldAsync(
        string fieldId,
        UpdateContactFieldOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Name.IsSet)
        {
            body["name"] = options.Name.Value;
        }
        if (options.Options.IsSet)
        {
            body["options"] = options.Options.Value;
        }
        if (options.Position.IsSet)
        {
            body["position"] = options.Position.Value;
        }

        var response = await _http
            .RequestAsync(new HttpMethod("PATCH"), $"/v1/contacts/fields/{fieldId}", body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<ContactField>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Remove the field and every answer to it.</summary>
    public async Task DeleteFieldAsync(string fieldId, CancellationToken cancellationToken = default) =>
        await _http.DeleteAsync($"/v1/contacts/fields/{fieldId}", null, cancellationToken)
            .ConfigureAwait(false);

    /// <summary>
    /// Volume and median reply time per thread. Counts and timings only: no message text
    /// and no author. Needs the <c>analytics</c> scope rather than <c>inbox</c>.
    /// </summary>
    public async Task<ConversationAnalytics> ConversationAnalyticsAsync(
        ConversationAnalyticsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options?.WorkspaceId,
            ["accountId"] = options?.AccountId,
            ["days"] = options?.Days,
            ["sort"] = options?.Sort,
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
        };
        var body = await _http
            .GetAsync("/v1/analytics/inbox/conversations", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<ConversationAnalytics>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Drop the keys the caller left unset, so a create stays minimal.</summary>
    private static Dictionary<string, object?> Compact(Dictionary<string, object?> body)
    {
        var out_ = new Dictionary<string, object?>();
        foreach (var (key, value) in body)
        {
            if (value is not null)
            {
                out_[key] = value;
            }
        }

        return out_;
    }
}
