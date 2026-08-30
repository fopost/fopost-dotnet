using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Workspaces</c> — the workspaces the credential can reach.</summary>
public sealed class WorkspacesResource
{
    private readonly FoPostHttpClient _http;

    internal WorkspacesResource(FoPostHttpClient http) => _http = http;

    public async Task<IReadOnlyList<Workspace>> ListAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/workspaces", null, cancellationToken).ConfigureAwait(false);
        return ToList<Workspace>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<Workspace> GetAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"/v1/workspaces/{Uri.EscapeDataString(workspaceId)}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Workspace>(FoPostHttpClient.Unwrap(body));
    }
}
