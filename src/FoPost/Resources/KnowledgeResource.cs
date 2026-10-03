using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Knowledge</c> — what the workspace has told FoPost about itself.
/// </summary>
/// <remarks>
/// A source is an FAQ, a note, a page on your own site, or a plain-text/CSV
/// item from the media library. Retrieval over these is what grounds a drafted
/// inbox reply in your own answers instead of an invented one. Needs the
/// <c>inbox</c> scope.
/// </remarks>
public sealed class KnowledgeResource
{
    private readonly FoPostHttpClient _http;

    internal KnowledgeResource(FoPostHttpClient http) => _http = http;

    private static string SourcePath(string sourceId) => $"/v1/knowledge/sources/{sourceId}";

    /// <summary>Every source in the workspace. Only a <c>ready</c> one is searched.</summary>
    public async Task<IReadOnlyList<KnowledgeSource>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/knowledge/sources", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<KnowledgeSource>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Adds a source and queues it for indexing, so it comes back <c>pending</c>.
    /// </summary>
    public async Task<KnowledgeSource> CreateAsync(
        CreateKnowledgeSourceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["kind"] = options.Kind,
            ["title"] = options.Title,
        };
        AddIfPresent(body, "content", options.Content);
        AddIfPresent(body, "url", options.Url);
        AddIfPresent(body, "media_id", options.MediaId);
        AddIfPresent(body, "brand_voice_id", options.BrandVoiceId);
        AddIfPresent(body, "workspace_id", options.WorkspaceId);

        var response = await _http.PostAsync("/v1/knowledge/sources", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<KnowledgeSource>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Partial update: only the fields you set are sent.</summary>
    public async Task<KnowledgeSource> UpdateAsync(
        string sourceId,
        UpdateKnowledgeSourceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Title.IsSet)
        {
            body["title"] = options.Title.Value;
        }

        if (options.Content.IsSet)
        {
            body["content"] = options.Content.Value;
        }

        if (options.Url.IsSet)
        {
            body["url"] = options.Url.Value;
        }

        if (options.BrandVoiceId.IsSet)
        {
            body["brand_voice_id"] = options.BrandVoiceId.Value;
        }

        var response = await _http
            .RequestAsync(HttpMethod.Patch, SourcePath(sourceId), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<KnowledgeSource>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Removes the source and every passage indexed from it.</summary>
    public async Task DeleteAsync(
        string sourceId,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync(SourcePath(sourceId), null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the source again — a <c>url</c> source is re-fetched. Returns once
    /// the re-index is queued, not once it has finished.
    /// </summary>
    public async Task<KnowledgeSyncResult> SyncAsync(
        string sourceId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"{SourcePath(sourceId)}/sync", new Dictionary<string, object?>(), cancellationToken)
            .ConfigureAwait(false);
        return Require<KnowledgeSyncResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// The passages closest to a question, best first. An empty list is the
    /// honest answer when nothing stored answers it.
    /// </summary>
    public async Task<IReadOnlyList<KnowledgeMatch>> SearchAsync(
        string q,
        SearchKnowledgeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["q"] = q,
            ["top_k"] = options?.TopK,
            ["brand_voice_id"] = options?.BrandVoiceId,
            ["workspace_id"] = options?.WorkspaceId,
        };
        var body = await _http.GetAsync("/v1/knowledge/search", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<KnowledgeMatch>(FoPostHttpClient.Unwrap(body));
    }

    private static void AddIfPresent(IDictionary<string, object?> target, string key, string? value)
    {
        if (value is not null)
        {
            target[key] = value;
        }
    }
}
