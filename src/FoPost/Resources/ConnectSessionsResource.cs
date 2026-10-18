using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.ConnectSessions</c> — hosted connect links: a URL your own user opens to connect a
/// network to the workspace, without a FoPost login.
/// </summary>
/// <remarks>Every call needs the <c>accounts</c> scope.</remarks>
public sealed class ConnectSessionsResource
{
    private readonly FoPostHttpClient _http;

    internal ConnectSessionsResource(FoPostHttpClient http) => _http = http;

    /// <summary>Mint a link. Send the user to <see cref="ConnectSession.Url"/> before <see cref="ConnectSession.ExpiresAt"/>.</summary>
    public async Task<ConnectSession> CreateAsync(
        CreateConnectSessionOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["externalUserId"] = options.ExternalUserId,
            ["platforms"] = options.Platforms,
        };
        if (options.WorkspaceId is not null)
        {
            body["workspaceId"] = options.WorkspaceId;
        }
        if (options.RedirectUrl is not null)
        {
            body["redirectUrl"] = options.RedirectUrl;
        }
        if (options.BrandingName is not null || options.BrandingLogoUrl is not null)
        {
            var branding = new Dictionary<string, object?>();
            if (options.BrandingName is not null)
            {
                branding["name"] = options.BrandingName;
            }
            if (options.BrandingLogoUrl is not null)
            {
                branding["logoUrl"] = options.BrandingLogoUrl;
            }
            body["branding"] = branding;
        }

        var response = await _http.PostAsync("/v1/connect-sessions", body, cancellationToken).ConfigureAwait(false);
        return Require<ConnectSession>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Poll a link to learn whether it was used and which account it connected.</summary>
    public async Task<ConnectSession> GetAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"/v1/connect-sessions/{Uri.EscapeDataString(sessionId)}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<ConnectSession>(FoPostHttpClient.Unwrap(body));
    }
}
