using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Workspaces</c> — the workspaces the credential can reach.</summary>
/// <remarks>Every call needs the <c>workspaces</c> scope.</remarks>
public sealed class WorkspacesResource
{
    private readonly FoPostHttpClient _http;

    internal WorkspacesResource(FoPostHttpClient http) => _http = http;

    private static string WorkspacePath(string workspaceId) => $"/v1/workspaces/{Uri.EscapeDataString(workspaceId)}";

    public async Task<IReadOnlyList<Workspace>> ListAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/workspaces", null, cancellationToken).ConfigureAwait(false);
        return ToList<Workspace>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<Workspace> GetAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(WorkspacePath(workspaceId), null, cancellationToken).ConfigureAwait(false);
        return Require<Workspace>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<Workspace> CreateAsync(
        CreateWorkspaceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["name"] = options.Name };
        AddIfPresent(body, "type", options.Type);
        AddIfPresent(body, "logo", options.Logo);
        AddIfPresent(body, "website", options.Website);
        AddIfPresent(body, "timezone", options.Timezone);
        AddIfPresent(body, "country", options.Country);
        AddIfPresent(body, "description", options.Description);
        AddIfPresent(body, "language", options.Language);

        var response = await _http.PostAsync("/v1/workspaces", body, cancellationToken).ConfigureAwait(false);
        return Require<Workspace>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Partial update: only the fields you set are sent.</summary>
    public async Task<Workspace> UpdateAsync(
        string workspaceId,
        UpdateWorkspaceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        AddIfSet(body, "name", options.Name);
        AddIfSet(body, "type", options.Type);
        AddIfSet(body, "logo", options.Logo);
        AddIfSet(body, "website", options.Website);
        AddIfSet(body, "timezone", options.Timezone);
        AddIfSet(body, "country", options.Country);
        AddIfSet(body, "description", options.Description);
        AddIfSet(body, "language", options.Language);
        // The settings fields are the ones this handler reads camelCase.
        AddIfSet(body, "requireApproval", options.RequireApproval);
        AddIfSet(body, "aiAltTextEnabled", options.AiAltTextEnabled);
        AddIfSet(body, "brandColor", options.BrandColor);
        AddIfSet(body, "connectRedirectDomains", options.ConnectRedirectDomains);

        var response = await _http.PutAsync(WorkspacePath(workspaceId), body, cancellationToken).ConfigureAwait(false);
        return Require<Workspace>(FoPostHttpClient.Unwrap(response));
    }

    public async Task DeleteAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync(WorkspacePath(workspaceId), null, cancellationToken).ConfigureAwait(false);
    }

    private static void AddIfPresent(IDictionary<string, object?> target, string key, string? value)
    {
        if (value is not null)
        {
            target[key] = value;
        }
    }

    private static void AddIfSet<T>(IDictionary<string, object?> target, string key, Optional<T> value)
    {
        if (value.IsSet)
        {
            target[key] = value.Value;
        }
    }
}
