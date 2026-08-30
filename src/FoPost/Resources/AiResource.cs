using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Ai</c> — caption assist, per-platform rewriting, and blog fan-out.
/// </summary>
/// <remarks>
/// Every call spends AI credits; check the balance with
/// <see cref="CreditsAsync"/>, and a <see cref="FoPostPaymentRequiredException"/>
/// means there are none left.
/// <para>
/// Watch the credential: <see cref="CreditsAsync"/> and
/// <see cref="GenerateCaptionAsync"/> accept an API key carrying the <c>ai</c>
/// scope, while <see cref="RewriteAsync"/> and
/// <see cref="RepurposeUrlAsync"/> are dashboard-session endpoints and need
/// <see cref="FoPostClientOptions.BearerToken"/> instead. An API key on those
/// two comes back 401.
/// </para>
/// </remarks>
public sealed class AiResource
{
    private readonly FoPostHttpClient _http;

    internal AiResource(FoPostHttpClient http) => _http = http;

    /// <summary>Credits remaining, used, and total for the current billing period.</summary>
    public async Task<AiCreditBalance> CreditsAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/ai/credits", null, cancellationToken).ConfigureAwait(false);
        return Require<AiCreditBalance>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<CaptionResult> GenerateCaptionAsync(
        GenerateCaptionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = Compact(new Dictionary<string, object?>
        {
            ["current_caption"] = options.CurrentCaption,
            ["image_urls"] = options.ImageUrls,
            ["platforms"] = options.Platforms,
            ["char_limit"] = options.CharLimit,
            ["workspace_id"] = options.WorkspaceId,
            ["brand_voice_id"] = options.BrandVoiceId,
        });

        var response = await _http.PostAsync("/v1/ai/generate-caption", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<CaptionResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Rewrite one draft for each target platform. Costs 1 credit per platform.</summary>
    public async Task<RewriteResult> RewriteAsync(
        RewriteOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = Compact(new Dictionary<string, object?>
        {
            ["content"] = options.Content,
            ["platforms"] = options.Platforms,
            ["tone"] = options.Tone,
            ["workspace_id"] = options.WorkspaceId,
            ["brand_voice_id"] = options.BrandVoiceId,
        });

        var response = await _http.PostAsync("/v1/ai/rewrite", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<RewriteResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Turn an article URL into a post for each platform, in one call.</summary>
    public async Task<RepurposeResult> RepurposeUrlAsync(
        RepurposeUrlOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = Compact(new Dictionary<string, object?>
        {
            ["url"] = options.Url,
            ["platforms"] = options.Platforms,
            ["workspace_id"] = options.WorkspaceId,
            ["brand_voice_id"] = options.BrandVoiceId,
        });

        var response = await _http.PostAsync("/v1/ai/repurpose-url", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<RepurposeResult>(FoPostHttpClient.Unwrap(response));
    }

    private static Dictionary<string, object?> Compact(Dictionary<string, object?> body)
    {
        foreach (var key in body.Where(pair => pair.Value is null).Select(pair => pair.Key).ToList())
        {
            body.Remove(key);
        }

        return body;
    }
}
