using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Media</c> — upload limits and direct uploads into the media library.
/// Needs the <c>posts</c> scope.
/// </summary>
/// <example>
/// <code>
/// await using var file = File.OpenRead("launch.mp4");
/// var media = await client.Media.UploadDirectAsync("9b2f6c1e-…", "launch.mp4", "video/mp4", file);
///
/// await client.Posts.CreateAsync(new CreatePostOptions
/// {
///     WorkspaceId = "9b2f6c1e-…",
///     Content = new List&lt;PostContent&gt; { new("Shipped!", new List&lt;MediaItem&gt; { new() { Type = "video", Url = media.Url } }) },
///     Accounts = accountIds,
/// });
/// </code>
/// </example>
public sealed class MediaResource
{
    private readonly FoPostHttpClient _http;

    internal MediaResource(FoPostHttpClient http) => _http = http;

    /// <summary>What the account may upload: the per-file ceiling, the multipart ceiling, and the allowed types.</summary>
    public async Task<UploadLimits> LimitsAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspaceId"] = workspaceId };
        var response = await _http.GetAsync("/v1/media/limits", query, cancellationToken).ConfigureAwait(false);
        return Require<UploadLimits>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Reserve an upload slot for a file of exactly <paramref name="size"/> bytes.</summary>
    public Task<PresignedUpload> PresignAsync(
        string workspaceId,
        string filename,
        string mimeType,
        long size,
        CancellationToken cancellationToken = default) =>
        PresignAsync(
            new PresignOptions { WorkspaceId = workspaceId, Filename = filename, MimeType = mimeType, Size = size },
            cancellationToken);

    /// <summary>Reserve an upload slot, optionally filing the upload or tagging it as a brand asset.</summary>
    public async Task<PresignedUpload> PresignAsync(
        PresignOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspaceId"] = options.WorkspaceId,
            ["filename"] = options.Filename,
            ["mimeType"] = options.MimeType,
            ["size"] = options.Size,
        };
        if (options.FolderId is not null)
        {
            body["folderId"] = options.FolderId;
        }
        if (options.Origin is not null)
        {
            body["origin"] = options.Origin;
        }
        if (options.BrandRole is not null)
        {
            body["brandRole"] = options.BrandRole;
        }
        if (options.BrandVoiceId is not null)
        {
            body["brandVoiceId"] = options.BrandVoiceId;
        }
        if (options.PersonConsent is { } consent)
        {
            body["personConsent"] = new Dictionary<string, object?>
            {
                ["subject"] = consent.Subject,
                ["statement"] = consent.Statement,
            };
        }

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

        using var stream = new MemoryStream(data, writable: false);
        return await UploadDirectAsync(workspaceId, filename, mimeType, stream, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Presign, stream the file to storage, and complete in one call. The stream must report its length.</summary>
    public Task<MediaLibraryItem> UploadDirectAsync(
        string workspaceId,
        string filename,
        string mimeType,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        return UploadDirectAsync(
            new PresignOptions { WorkspaceId = workspaceId, Filename = filename, MimeType = mimeType },
            data,
            cancellationToken);
    }

    /// <summary>
    /// Presign with the full option set, stream the file to storage, and complete.
    /// <see cref="PresignOptions.Size"/> is taken from the stream when left at zero.
    /// </summary>
    public async Task<MediaLibraryItem> UploadDirectAsync(
        PresignOptions options,
        Stream data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(data);

        if (options.Size <= 0)
        {
            if (!data.CanSeek)
            {
                throw new ArgumentException("fopost: set Size for a stream that cannot report its length", nameof(options));
            }

            options.Size = data.Length - data.Position;
        }

        var upload = await PresignAsync(options, cancellationToken).ConfigureAwait(false);
        await _http.PutStreamAsync(upload.UploadUrl, upload.Headers, data, options.Size, cancellationToken)
            .ConfigureAwait(false);
        return await CompleteAsync(upload.UploadId, cancellationToken).ConfigureAwait(false);
    }
}
