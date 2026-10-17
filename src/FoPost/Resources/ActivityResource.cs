using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Activity</c> — what happened in a workspace, and the audit log.</summary>
public sealed class ActivityResource
{
    private readonly FoPostHttpClient _http;

    internal ActivityResource(FoPostHttpClient http) => _http = http;

    /// <summary>
    /// Activity newest first. <see cref="ActivityKinds.Security"/> is the audit log: members
    /// joining, leaving or changing role and access, and changes to two-step verification,
    /// passkeys, single sign-on and signed-in devices. Those rows are append-only and never expire.
    /// </summary>
    public async Task<ActivityPage> ListAsync(
        ListActivityOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options?.WorkspaceId,
            ["kind"] = options?.Kind,
            ["from"] = options?.From,
            ["to"] = options?.To,
            ["cursor"] = options?.Cursor,
            ["limit"] = options?.Limit,
        };
        // The response carries meta beside data, so it is read whole rather than unwrapped.
        var body = await _http.GetAsync("/v1/activity", query, cancellationToken).ConfigureAwait(false);
        var events = ToList<ActivityEvent>((body as JsonObject)?["data"]);
        var cursor = ((body as JsonObject)?["meta"] as JsonObject)?["next_cursor"];
        return new ActivityPage(events, cursor?.GetValue<string?>());
    }
}
