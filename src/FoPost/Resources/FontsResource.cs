using System.Net.Http.Headers;
using System.Text;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Fonts</c> — the bundled font library and the fonts a workspace uploads for its
/// designs. Needs the <c>posts</c> scope.
/// </summary>
/// <example>
/// <code>
/// var font = await client.Fonts.UploadAsync("9b2f6c1e-…", "fonts/Brand-Bold.woff2",
///     new UploadFontOptions { Family = "Brand", Weight = 700 });
/// </code>
/// </example>
public sealed class FontsResource
{
    private readonly FoPostHttpClient _http;

    internal FontsResource(FoPostHttpClient http) => _http = http;

    /// <summary>The library, plus the workspace's uploads when <paramref name="workspaceId"/> is given.</summary>
    public async Task<FontList> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/fonts", query, cancellationToken).ConfigureAwait(false);
        return Require<FontList>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Upload a font file from disk (TTF, OTF, WOFF or WOFF2, up to 5 MB).</summary>
    public async Task<UploadedFont> UploadAsync(
        string workspaceId,
        string path,
        UploadFontOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await UploadAsync(workspaceId, Path.GetFileName(path), stream, options, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<UploadedFont> UploadAsync(
        string workspaceId,
        string fileName,
        byte[] data,
        UploadFontOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        using var stream = new MemoryStream(data, writable: false);
        return await UploadAsync(workspaceId, fileName, stream, options, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Upload a font from a stream. A seekable stream is rewound if a 429 retry resends it.</summary>
    public async Task<UploadedFont> UploadAsync(
        string workspaceId,
        string fileName,
        Stream data,
        UploadFontOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        var start = data.CanSeek ? data.Position : 0;
        var response = await _http.RequestAsync(
            HttpMethod.Post,
            "/v1/fonts",
            () =>
            {
                if (data.CanSeek)
                {
                    data.Position = start;
                }

                // Names are quoted explicitly: .NET sends them bare, which some multipart parsers reject.
                var form = new MultipartFormDataContent { { Text(workspaceId), "\"workspace_id\"" } };
                if (options?.Family is { } family)
                {
                    form.Add(Text(family), "\"family\"");
                }
                if (options?.Weight is { } weight)
                {
                    form.Add(Text(weight.ToString(System.Globalization.CultureInfo.InvariantCulture)), "\"weight\"");
                }
                if (options?.Style is { } style)
                {
                    form.Add(Text(style), "\"style\"");
                }

                var part = new LeaveOpenStreamContent(data);
                part.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeOf(fileName));
                form.Add(part, "\"file\"", $"\"{fileName}\"");
                return form;
            },
            null,
            cancellationToken).ConfigureAwait(false);
        return Require<UploadedFont>(FoPostHttpClient.Unwrap(response));
    }

    public async Task DeleteAsync(
        string fontId,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync($"/v1/fonts/{Uri.EscapeDataString(fontId)}", null, cancellationToken)
            .ConfigureAwait(false);
    }

    private static StringContent Text(string value) => new(value, Encoding.UTF8);

    private static string MimeTypeOf(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".ttf" => "font/ttf",
            ".otf" => "font/otf",
            ".woff" => "font/woff",
            ".woff2" => "font/woff2",
            _ => "application/octet-stream",
        };

    // A 429 retry rebuilds the form, so the caller's stream must outlive one attempt.
    private sealed class LeaveOpenStreamContent : StreamContent
    {
        public LeaveOpenStreamContent(Stream content) : base(content)
        {
        }

        protected override void Dispose(bool disposing) => base.Dispose(false);
    }
}
