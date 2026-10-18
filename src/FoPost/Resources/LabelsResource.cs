using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Labels</c> — workspace labels you can attach to posts.</summary>
/// <remarks>Every call needs the <c>labels</c> scope.</remarks>
public sealed class LabelsResource
{
    private readonly FoPostHttpClient _http;

    internal LabelsResource(FoPostHttpClient http) => _http = http;

    private static string LabelPath(string labelId) => $"/v1/labels/{Uri.EscapeDataString(labelId)}";

    public async Task<IReadOnlyList<Label>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/labels", query, cancellationToken).ConfigureAwait(false);
        return ToList<Label>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<Label> GetAsync(
        string labelId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(LabelPath(labelId), null, cancellationToken).ConfigureAwait(false);
        return Require<Label>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Create a label. <paramref name="color"/> is a hex colour such as <c>#2563eb</c>.</summary>
    public async Task<Label> CreateAsync(
        string workspaceId,
        string name,
        string color,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(workspaceId);
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(color);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["name"] = name,
            ["color"] = color,
        };
        var response = await _http.PostAsync("/v1/labels", body, cancellationToken).ConfigureAwait(false);
        return Require<Label>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Both fields are required: the API replaces the label's name and colour together.</summary>
    public async Task<Label> UpdateAsync(
        string labelId,
        string name,
        string color,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(color);

        var body = new Dictionary<string, object?> { ["name"] = name, ["color"] = color };
        var response = await _http.PutAsync(LabelPath(labelId), body, cancellationToken).ConfigureAwait(false);
        return Require<Label>(FoPostHttpClient.Unwrap(response));
    }

    public async Task DeleteAsync(
        string labelId,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync(LabelPath(labelId), null, cancellationToken).ConfigureAwait(false);
    }
}
