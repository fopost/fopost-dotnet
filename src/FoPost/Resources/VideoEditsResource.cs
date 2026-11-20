using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.VideoEdits</c> — edit a library video with a list of operations. A job runs in the
/// background; read it back with <see cref="GetAsync"/> until it has succeeded or failed.
/// </summary>
/// <remarks>
/// Needs the <c>posts</c> scope; an operation whose <see cref="VideoEditOpInfo.UsesAi"/> is true
/// spends AI balance and needs <c>ai</c> as well.
/// </remarks>
/// <example>
/// <code>
/// var job = await client.VideoEdits.CreateAsync("media_1", new[]
/// {
///     new VideoEditOperation(VideoEditOps.Trim, new Dictionary&lt;string, object?&gt; { ["startSec"] = 2, ["endSec"] = 12 }),
/// });
/// </code>
/// </example>
public sealed class VideoEditsResource
{
    private readonly FoPostHttpClient _http;

    internal VideoEditsResource(FoPostHttpClient http) => _http = http;

    /// <summary>Every operation, and whether this deployment can run it.</summary>
    public async Task<IReadOnlyList<VideoEditOpInfo>> OpsAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/video-edits/ops", null, cancellationToken).ConfigureAwait(false);
        return ToList<VideoEditOpInfo>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Recent edits in a workspace, or of one video. Pass <paramref name="workspaceId"/> or <paramref name="mediaId"/>.</summary>
    public async Task<IReadOnlyList<VideoEdit>> ListAsync(
        string? workspaceId = null,
        string? mediaId = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["media_id"] = mediaId,
            ["limit"] = limit,
        };
        var body = await _http.GetAsync("/v1/video-edits", query, cancellationToken).ConfigureAwait(false);
        return ToList<VideoEdit>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Queue an edit of <paramref name="mediaId"/>: 1 to 10 operations, run in order.</summary>
    public async Task<VideoEdit> CreateAsync(
        string mediaId,
        IEnumerable<VideoEditOperation> operations,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operations);

        var body = new Dictionary<string, object?>
        {
            ["media_id"] = mediaId,
            ["operations"] = operations
                .Select(operation =>
                {
                    var step = new Dictionary<string, object?> { ["op"] = operation.Op };
                    if (operation.Params is not null)
                    {
                        step["params"] = operation.Params;
                    }

                    return step;
                })
                .ToList(),
        };
        if (workspaceId is not null)
        {
            body["workspace_id"] = workspaceId;
        }

        var response = await _http.PostAsync("/v1/video-edits", body, cancellationToken).ConfigureAwait(false);
        return Require<VideoEdit>(FoPostHttpClient.Unwrap(response));
    }

    public async Task<VideoEdit> GetAsync(
        string videoEditId,
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http
            .GetAsync($"/v1/video-edits/{Uri.EscapeDataString(videoEditId)}", query, cancellationToken)
            .ConfigureAwait(false);
        return Require<VideoEdit>(FoPostHttpClient.Unwrap(body));
    }
}
