using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Validate</c> — check a draft, its length, or a media file against
/// platform rules without creating anything.
/// </summary>
/// <remarks>
/// Nothing is stored server-side. Every call accepts an API key carrying the
/// <c>posts</c> scope.
/// </remarks>
/// <example>
/// <code>
/// var result = await client.Validate.PostAsync(new ValidatePostOptions
/// {
///     Content = "Hello from the SDK",
///     Platforms = new List&lt;string&gt; { Platforms.Twitter, Platforms.LinkedIn },
/// });
/// if (!result.Ready)
/// {
///     foreach (var platform in result.Platforms.Where(p => !p.Ready))
///     {
///         Console.WriteLine($"{platform.Platform}: {string.Join(", ", platform.Issues)}");
///     }
/// }
/// </code>
/// </example>
public sealed class ValidateResource
{
    private readonly FoPostHttpClient _http;

    internal ValidateResource(FoPostHttpClient http) => _http = http;

    /// <summary>Per-platform blockers and advisory signals for a draft post.</summary>
    public async Task<PostValidation> PostAsync(
        ValidatePostOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["content"] = options.Content,
            ["media"] = options.Media,
            ["platforms"] = options.Platforms,
        };

        var response = await _http.PostAsync("/v1/validate/post", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<PostValidation>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>How each platform counts the text, and whether it fits.</summary>
    public async Task<LengthValidation> LengthAsync(
        ValidateLengthOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["text"] = options.Text,
            ["platforms"] = options.Platforms,
        };

        var response = await _http.PostAsync("/v1/validate/length", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<LengthValidation>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Fetch a public file and check it can be attached. Answers 200 even when a check fails.</summary>
    public async Task<MediaValidation> MediaAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);

        var body = new Dictionary<string, object?> { ["url"] = url };

        var response = await _http.PostAsync("/v1/validate/media", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<MediaValidation>(FoPostHttpClient.Unwrap(response));
    }
}
