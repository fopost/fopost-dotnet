using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Queue</c> — the posting queue: weekly slots, and the times they produce. A post
/// created or updated with <see cref="CreatePostOptions.Schedule"/> set to
/// <see cref="PostSchedules.Queue"/> takes the next free one.
/// </summary>
/// <remarks>Every call needs the <c>posts</c> scope.</remarks>
public sealed class QueueResource
{
    private readonly FoPostHttpClient _http;

    internal QueueResource(FoPostHttpClient http) => _http = http;

    private static string SlotPath(string slotId) => $"/v1/queue/slots/{Uri.EscapeDataString(slotId)}";

    /// <summary>Every slot in the workspace.</summary>
    public async Task<IReadOnlyList<QueueSlot>> ListSlotsAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(workspaceId);

        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/queue/slots", query, cancellationToken).ConfigureAwait(false);
        return ToList<QueueSlot>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Add a weekly slot.</summary>
    public async Task<QueueSlot> CreateSlotAsync(
        CreateQueueSlotOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["weekday"] = options.Weekday,
            ["time"] = options.Time,
            ["timezone"] = options.Timezone,
        };
        if (options.AccountId is not null)
        {
            body["account_id"] = options.AccountId;
        }

        var response = await _http.PostAsync("/v1/queue/slots", body, cancellationToken).ConfigureAwait(false);
        return Require<QueueSlot>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Change a slot. Posts queued on it move with it, listed in <see cref="QueueSlot.Moved"/>.</summary>
    public async Task<QueueSlot> UpdateSlotAsync(
        string slotId,
        UpdateQueueSlotOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.AccountId.IsSet)
        {
            body["account_id"] = options.AccountId.Value;
        }
        if (options.Weekday.IsSet)
        {
            body["weekday"] = options.Weekday.Value;
        }
        if (options.Time.IsSet)
        {
            body["time"] = options.Time.Value;
        }
        if (options.Timezone.IsSet)
        {
            body["timezone"] = options.Timezone.Value;
        }

        var response = await _http.PutAsync(SlotPath(slotId), body, cancellationToken).ConfigureAwait(false);
        return Require<QueueSlot>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Remove a slot. Posts queued on it move to the next free one.</summary>
    public async Task<QueueSlotDeletion> DeleteSlotAsync(
        string slotId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(SlotPath(slotId), null, cancellationToken).ConfigureAwait(false);
        return Require<QueueSlotDeletion>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The next free slot time.</summary>
    public async Task<QueueNext> NextAsync(
        string workspaceId,
        string? accountId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(workspaceId);

        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["account_id"] = accountId,
        };
        var body = await _http.GetAsync("/v1/queue/next", query, cancellationToken).ConfigureAwait(false);
        return Require<QueueNext>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>The upcoming slot times, soonest first.</summary>
    public async Task<IReadOnlyList<DateTimeOffset>> PreviewAsync(
        string workspaceId,
        PreviewQueueOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(workspaceId);

        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["account_id"] = options?.AccountId,
            ["count"] = options?.Count,
        };
        var body = await _http.GetAsync("/v1/queue/preview", query, cancellationToken).ConfigureAwait(false);
        return ToList<DateTimeOffset>(FoPostHttpClient.Unwrap(body));
    }
}
