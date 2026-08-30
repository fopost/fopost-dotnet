using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Accounts</c> — the social accounts connected to a workspace.</summary>
public sealed class AccountsResource
{
    private readonly FoPostHttpClient _http;

    internal AccountsResource(FoPostHttpClient http) => _http = http;

    /// <summary>Connected accounts, across every workspace unless one is named.</summary>
    public async Task<IReadOnlyList<SocialAccount>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        // This endpoint reads a camelCase query param; posts and labels use snake.
        var query = new Dictionary<string, object?> { ["workspaceId"] = workspaceId };
        var body = await _http.GetAsync("/v1/accounts", query, cancellationToken).ConfigureAwait(false);
        return ToList<SocialAccount>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<SocialAccount> GetAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"/v1/accounts/{Uri.EscapeDataString(accountId)}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<SocialAccount>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Token validity and last-check detail for one account.</summary>
    public async Task<AccountHealth> HealthAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"/v1/accounts/{Uri.EscapeDataString(accountId)}/health", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AccountHealth>(FoPostHttpClient.Unwrap(body));
    }
}
