using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Media</c> — direct uploads into the media library. Needs the
/// <c>posts</c> scope.
/// </summary>
/// <example>
/// <code>
/// var bytes = await File.ReadAllBytesAsync("launch.png");
/// var media = await client.Media.UploadDirectAsync("9b2f6c1e-…", "launch.png", "image/png", bytes);
///
/// await client.Posts.CreateAsync(new CreatePostOptions
/// {
///     WorkspaceId = "9b2f6c1e-…",
///     Content = new List&lt;PostContent&gt; { new("Shipped!", new List&lt;MediaItem&gt; { new() { Url = media.Url } }) },
///     Accounts = accountIds,
/// });
/// </code>
/// </example>
public sealed class MediaResource
{
    private readonly FoPostHttpClient _http;

    internal MediaResource(FoPostHttpClient http) => _http = http;

    /// <summary>Reserve an upload slot for a file of exactly <paramref name="size"/> bytes.</summary>
    public async Task<PresignedUpload> PresignAsync(
        string workspaceId,
        string filename,
        string mimeType,
        long size,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["filename"] = filename,
            ["mimeType"] = mimeType,
            ["size"] = size,
        };
        var response = await _http.PostAsync("/v1/media/presign", body, cancellationToken).ConfigureAwait(false);
        return Require<PresignedUpload>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Finish an upload once the bytes are in place and get the library item back.</summary>
    public async Task<MediaLibraryItem> CompleteAsync(
        string uploadId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"/v1/media/presign/{Uri.EscapeDataString(uploadId)}/complete", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<MediaLibraryItem>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Presign, PUT the bytes, and complete in one call.</summary>
    public async Task<MediaLibraryItem> UploadDirectAsync(
        string workspaceId,
        string filename,
        string mimeType,
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        var upload = await PresignAsync(workspaceId, filename, mimeType, data.Length, cancellationToken)
            .ConfigureAwait(false);
        await _http.PutBytesAsync(upload.UploadUrl, upload.Headers, data, cancellationToken).ConfigureAwait(false);
        return await CompleteAsync(upload.UploadId, cancellationToken).ConfigureAwait(false);
    }
}
