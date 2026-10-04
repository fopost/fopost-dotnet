using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Broadcasts</c> — one message into every conversation the workspace already
/// has with a segment of its contacts.
/// </summary>
/// <remarks>
/// A broadcast is not a post and not a cold DM: every message lands in a direct-message
/// thread the contact already started.
/// <para>
/// Nothing is sent into a closed messaging window. Messenger and Instagram take a
/// business-initiated message only within 24 hours of the contact's last one, so recipients
/// outside it come back skipped with <c>window_closed</c> rather than attempted, which is
/// why the number sent is often lower than the audience. Telegram, Slack, Bluesky and
/// Reddit have no window.
/// </para>
/// <para>
/// Reading needs the <c>inbox</c> scope; <see cref="SendAsync"/> and
/// <see cref="CancelAsync"/> also need <c>publish</c>.
/// </para>
/// </remarks>
public sealed class BroadcastsResource
{
    private readonly FoPostHttpClient _http;

    internal BroadcastsResource(FoPostHttpClient http) => _http = http;

    /// <summary>
    /// One page of broadcasts, newest first. Omit the workspace to span every workspace the
    /// key can reach; each broadcast then carries <c>WorkspaceId</c>.
    /// </summary>
    public async Task<BroadcastPage> ListAsync(
        ListBroadcastsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options?.WorkspaceId,
            ["status"] = options?.Status,
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
        };
        var body = await _http.GetAsync("/v1/broadcasts", query, cancellationToken).ConfigureAwait(false);
        return new BroadcastPage(ToList<Broadcast>(FoPostHttpClient.Unwrap(body)), PaginationOf(body));
    }

    /// <summary>
    /// One broadcast. A broadcast in a workspace the key cannot reach answers 404, exactly
    /// as an id that never existed does.
    /// </summary>
    public async Task<Broadcast> GetAsync(string broadcastId, CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync($"/v1/broadcasts/{broadcastId}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Broadcast>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Write a broadcast without sending it.</summary>
    public async Task<Broadcast> CreateAsync(
        CreateBroadcastOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["account_id"] = options.AccountId,
            ["name"] = options.Name,
            ["text"] = options.Text,
            ["media_id"] = options.MediaId,
            ["audience"] = options.Audience,
            ["scheduled_at"] = options.ScheduledAt,
        };
        var response = await _http.PostAsync("/v1/broadcasts", Compact(body), cancellationToken)
            .ConfigureAwait(false);
        return Require<Broadcast>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Patch a broadcast. Only a draft or scheduled broadcast can be edited.</summary>
    public async Task<Broadcast> UpdateAsync(
        string broadcastId,
        UpdateBroadcastOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Name.IsSet)
        {
            body["name"] = options.Name.Value;
        }
        if (options.Text.IsSet)
        {
            body["text"] = options.Text.Value;
        }
        if (options.MediaId.IsSet)
        {
            body["media_id"] = options.MediaId.Value;
        }
        if (options.Audience.IsSet)
        {
            body["audience"] = options.Audience.Value;
        }
        if (options.ScheduledAt.IsSet)
        {
            body["scheduled_at"] = options.ScheduledAt.Value;
        }

        var response = await _http
            .RequestAsync(new HttpMethod("PATCH"), $"/v1/broadcasts/{broadcastId}", body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Broadcast>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Freeze the audience into a recipient list and start sending. The returned
    /// <c>Recipients</c> is how many contacts matched, not how many will be messaged — the
    /// messaging window decides that. Needs <c>publish</c> as well as <c>inbox</c>.
    /// </summary>
    public async Task<BroadcastSent> SendAsync(
        string broadcastId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"/v1/broadcasts/{broadcastId}/send", new Dictionary<string, object?>(), cancellationToken)
            .ConfigureAwait(false);
        return Require<BroadcastSent>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Stop a broadcast where it stands. Anyone not yet written to stays unsent; messages
    /// already delivered are not recalled. Needs <c>publish</c>.
    /// </summary>
    public async Task<Broadcast> CancelAsync(
        string broadcastId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"/v1/broadcasts/{broadcastId}/cancel", new Dictionary<string, object?>(), cancellationToken)
            .ConfigureAwait(false);
        return Require<Broadcast>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// One row per contact, with what became of their message. A skipped row carries its
    /// <c>SkipReason</c>.
    /// </summary>
    public async Task<RecipientPage> RecipientsAsync(
        string broadcastId,
        ListRecipientsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["status"] = options?.Status,
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
        };
        var body = await _http
            .GetAsync($"/v1/broadcasts/{broadcastId}/recipients", query, cancellationToken)
            .ConfigureAwait(false);
        return new RecipientPage(
            ToList<BroadcastRecipient>(FoPostHttpClient.Unwrap(body)),
            PaginationOf(body));
    }

    /// <summary>
    /// Remove a broadcast and its recipient records. Messages already sent stay in the
    /// conversations they went to.
    /// </summary>
    public async Task DeleteAsync(string broadcastId, CancellationToken cancellationToken = default) =>
        await _http.DeleteAsync($"/v1/broadcasts/{broadcastId}", null, cancellationToken).ConfigureAwait(false);

    /// <summary>Drop the keys the caller left unset, so a create stays minimal.</summary>
    private static Dictionary<string, object?> Compact(Dictionary<string, object?> body)
    {
        var compacted = new Dictionary<string, object?>();
        foreach (var (key, value) in body)
        {
            if (value is not null)
            {
                compacted[key] = value;
            }
        }
        return compacted;
    }

    /// <summary>These lists answer <c>{data, pagination}</c>, not <c>{data, meta}</c>.</summary>
    internal static ContactPageMeta PaginationOf(JsonNode? body) =>
        body is JsonObject obj &&
        obj.TryGetPropertyValue("pagination", out var meta) &&
        meta is JsonObject
            ? Require<ContactPageMeta>(meta)
            : new ContactPageMeta();
}
