using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Accounts</c> — the social accounts connected to a workspace.</summary>
public sealed class AccountsResource
{
    private readonly FoPostHttpClient _http;

    internal AccountsResource(FoPostHttpClient http) => _http = http;

    /// <summary>Connected accounts, across every workspace unless one is named.</summary>
    public Task<IReadOnlyList<SocialAccount>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default) =>
        ListAsync(workspaceId, null, cancellationToken);

    /// <summary>
    /// Connected accounts, narrowed to one account group when <paramref name="groupId"/> is set.
    /// </summary>
    public async Task<IReadOnlyList<SocialAccount>> ListAsync(
        string? workspaceId,
        string? groupId,
        CancellationToken cancellationToken = default)
    {
        // This endpoint reads a camelCase query param; posts and labels use snake.
        var query = new Dictionary<string, object?> { ["workspaceId"] = workspaceId, ["group_id"] = groupId };
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

    /// <summary>
    /// Set the name shown instead of the platform name. <c>null</c> or an empty string restores
    /// the platform name. The result carries <c>Id</c>, <c>Name</c> and <c>PlatformName</c>.
    /// </summary>
    public async Task<SocialAccount> RenameAsync(
        string accountId,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["display_name"] = displayName };
        var response = await _http
            .RequestAsync(HttpMethod.Patch, AccountPath(accountId), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<SocialAccount>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Move the account to another workspace the caller owns. The result carries <c>Id</c> and
    /// <c>WorkspaceId</c>. A 409 with code <c>move_blocked</c> lists <c>blocking_tables</c> on
    /// <see cref="FoPostException.Body"/>; any other 409 means the target already has an account
    /// on that network.
    /// </summary>
    public async Task<SocialAccount> MoveAsync(
        string accountId,
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var response = await _http
            .PostAsync($"{AccountPath(accountId)}/move", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<SocialAccount>(FoPostHttpClient.Unwrap(response));
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

    private static string AccountPath(string accountId) => $"/v1/accounts/{Uri.EscapeDataString(accountId)}";
}
