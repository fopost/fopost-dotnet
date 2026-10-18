using System.Text.Json;
using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Logs</c> — the API requests made against a workspace.</summary>
/// <remarks>Needs the <c>workspaces</c> scope.</remarks>
public sealed class LogsResource
{
    private readonly FoPostHttpClient _http;

    internal LogsResource(FoPostHttpClient http) => _http = http;

    /// <summary>Request logs newest first, with the totals for the window.</summary>
    public async Task<RequestLogPage> ListAsync(
        ListLogsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options?.WorkspaceId,
            ["days"] = options?.Days,
            ["status"] = options?.Status,
            ["method"] = options?.Method,
            ["status_code"] = options?.StatusCode,
            ["platform"] = options?.Platform,
            ["route"] = options?.Route,
            ["q"] = options?.Q,
            ["limit"] = options?.Limit,
            ["before"] = options?.Before,
        };
        // meta and summary sit beside data, so the body is read whole rather than unwrapped.
        var body = await _http.GetAsync("/v1/logs", query, cancellationToken).ConfigureAwait(false);
        var obj = body as JsonObject;
        var meta = obj?["meta"] as JsonObject;
        var summary = obj?["summary"] is JsonObject s
            ? s.Deserialize<RequestLogSummary>(FoPostJson.Options) ?? new RequestLogSummary()
            : new RequestLogSummary();
        return new RequestLogPage(
            ToList<RequestLog>(obj?["data"]),
            meta?["next_cursor"]?.GetValue<string?>(),
            meta?["has_more"]?.GetValue<bool>() ?? false,
            summary);
    }
}
