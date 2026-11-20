using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Designs</c> — designs, their saved versions, exports into the media library,
/// and the tools that build one from text or a post.
/// </summary>
/// <remarks>
/// Needs the <c>posts</c> scope; <see cref="TextToSlidesAsync"/> spends AI balance and needs
/// <c>ai</c> as well.
/// </remarks>
/// <example>
/// <code>
/// var design = await client.Designs.CreateAsync(new CreateDesignOptions
/// {
///     WorkspaceId = "9b2f6c1e-…",
///     Kind = DesignKinds.Post,
///     Name = "Launch card",
/// });
/// var export = await client.Designs.ExportAsync(design.Id, new ExportDesignOptions { Format = DesignExportFormats.Png });
/// </code>
/// </example>
public sealed class DesignsResource
{
    private readonly FoPostHttpClient _http;

    internal DesignsResource(FoPostHttpClient http) => _http = http;

    private static string DesignPath(string designId) => $"/v1/designs/{Uri.EscapeDataString(designId)}";

    /// <summary>Designs in a workspace, newest first, without their documents.</summary>
    public async Task<IReadOnlyList<Design>> ListAsync(
        string workspaceId,
        ListDesignsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["kind"] = options?.Kind,
            ["limit"] = options?.Limit,
            ["offset"] = options?.Offset,
        };
        var body = await _http.GetAsync("/v1/designs", query, cancellationToken).ConfigureAwait(false);
        return ToList<Design>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<Design> CreateAsync(
        CreateDesignOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["kind"] = options.Kind,
        };
        AddIfPresent(body, "name", options.Name);
        AddIfPresent(body, "document", options.Document);
        AddIfPresent(body, "brand_voice_id", options.BrandVoiceId);

        var response = await _http.PostAsync("/v1/designs", body, cancellationToken).ConfigureAwait(false);
        return Require<Design>(FoPostHttpClient.Unwrap(response));
    }

    public async Task<Design> GetAsync(
        string designId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(DesignPath(designId), null, cancellationToken).ConfigureAwait(false);
        return Require<Design>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Partial update. With <see cref="UpdateDesignOptions.CreateVersion"/> the answer carries the saved version.</summary>
    public async Task<DesignChange> UpdateAsync(
        string designId,
        UpdateDesignOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        AddIfPresent(body, "name", options.Name);
        AddIfPresent(body, "kind", options.Kind);
        AddIfPresent(body, "document", options.Document);
        AddIfPresent(body, "create_version", options.CreateVersion);

        var response = await _http
            .RequestAsync(HttpMethod.Patch, DesignPath(designId), body, null, cancellationToken)
            .ConfigureAwait(false);
        return ReadChange(response);
    }

    public async Task<DesignDeletion> DeleteAsync(
        string designId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.DeleteAsync(DesignPath(designId), null, cancellationToken).ConfigureAwait(false);
        return Require<DesignDeletion>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Saved versions, newest first. <paramref name="includeDocument"/> adds each version's document.</summary>
    public async Task<IReadOnlyList<DesignVersion>> ListVersionsAsync(
        string designId,
        bool includeDocument = false,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["include"] = includeDocument ? "document" : null };
        var body = await _http
            .GetAsync($"{DesignPath(designId)}/versions", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<DesignVersion>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Make a saved version's document current again.</summary>
    public async Task<DesignChange> RestoreVersionAsync(
        string designId,
        string versionId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync(
                $"{DesignPath(designId)}/versions/{Uri.EscapeDataString(versionId)}/restore",
                null,
                cancellationToken)
            .ConfigureAwait(false);
        return ReadChange(response);
    }

    public async Task<Design> DuplicateAsync(
        string designId,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();
        AddIfPresent(body, "name", name);

        var response = await _http
            .PostAsync($"{DesignPath(designId)}/duplicate", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<Design>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Render pages into the media library.</summary>
    public async Task<DesignExport> ExportAsync(
        string designId,
        ExportDesignOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["format"] = options.Format };
        AddIfPresent(body, "page_indexes", options.PageIndexes);
        AddIfPresent(body, "duration_sec", options.DurationSec);

        var response = await _http
            .PostAsync($"{DesignPath(designId)}/export", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<DesignExport>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One copy of the design per row, each row's values filling the layers they name (up to 50 rows).</summary>
    public async Task<IReadOnlyList<Design>> BulkCreateAsync(
        string designId,
        IEnumerable<IDictionary<string, string>> rows,
        string? nameColumn = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var body = new Dictionary<string, object?> { ["rows"] = rows.ToList() };
        AddIfPresent(body, "name_column", nameColumn);

        var response = await _http
            .PostAsync($"{DesignPath(designId)}/bulk", body, cancellationToken)
            .ConfigureAwait(false);
        return ToList<Design>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Posts that used this design's exported media, with their metrics.</summary>
    public async Task<IReadOnlyList<DesignPost>> ListPostsAsync(
        string designId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"{DesignPath(designId)}/posts", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<DesignPost>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Split long text, or a post, into carousel slide copy. Spends AI balance; needs <c>ai</c>.</summary>
    public async Task<DesignSlides> TextToSlidesAsync(
        TextToSlidesOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["workspace_id"] = options.WorkspaceId };
        AddIfPresent(body, "text", options.Text);
        AddIfPresent(body, "post_id", options.PostId);
        AddIfPresent(body, "max_slides", options.MaxSlides);
        AddIfPresent(body, "brand_voice_id", options.BrandVoiceId);

        var response = await _http
            .PostAsync("/v1/designs/text-to-slides", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<DesignSlides>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Create a design from text or a post's text.</summary>
    public async Task<Design> FromPostAsync(
        DesignFromPostOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?> { ["workspace_id"] = options.WorkspaceId };
        AddIfPresent(body, "text", options.Text);
        AddIfPresent(body, "post_id", options.PostId);
        AddIfPresent(body, "style", options.Style);
        AddIfPresent(body, "preset", options.Preset);
        AddIfPresent(body, "account_id", options.AccountId);
        AddIfPresent(body, "carousel", options.Carousel);
        AddIfPresent(body, "name", options.Name);
        AddIfPresent(body, "brand_voice_id", options.BrandVoiceId);

        var response = await _http
            .PostAsync("/v1/designs/from-post", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<Design>(FoPostHttpClient.Unwrap(response));
    }

    private static DesignChange ReadChange(JsonNode? response)
    {
        var design = Require<Design>(FoPostHttpClient.Unwrap(response));
        var version = response is JsonObject obj && obj["version"] is JsonObject node
            ? Require<DesignVersion>(node)
            : null;
        return new DesignChange(design, version);
    }

    private static void AddIfPresent(IDictionary<string, object?> target, string key, object? value)
    {
        if (value is not null)
        {
            target[key] = value;
        }
    }
}
