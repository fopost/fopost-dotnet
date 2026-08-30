using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Labels</c> — workspace labels you can attach to posts.</summary>
public sealed class LabelsResource
{
    private readonly FoPostHttpClient _http;

    internal LabelsResource(FoPostHttpClient http) => _http = http;

    public async Task<IReadOnlyList<Label>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/labels", query, cancellationToken).ConfigureAwait(false);
        return ToList<Label>(FoPostHttpClient.Unwrap(body));
    }
}
