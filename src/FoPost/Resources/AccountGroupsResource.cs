using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.AccountGroups</c> — named sets of accounts a post can target with
/// <see cref="CreatePostOptions.AccountGroupId"/>.
/// </summary>
/// <example>
/// <code>
/// var group = await client.AccountGroups.CreateAsync(workspaceId, "Brand A", new[] { accountId });
/// await client.Posts.CreateAsync(new CreatePostOptions
/// {
///     WorkspaceId = workspaceId,
///     AccountGroupId = group.Id,
///     Content = new List&lt;PostContent&gt; { new("Hello") },
/// });
/// </code>
/// </example>
public sealed class AccountGroupsResource
{
    private readonly FoPostHttpClient _http;

    internal AccountGroupsResource(FoPostHttpClient http) => _http = http;

    public async Task<IReadOnlyList<AccountGroup>> ListAsync(
        string? workspaceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        var body = await _http.GetAsync("/v1/account-groups", query, cancellationToken).ConfigureAwait(false);
        return ToList<AccountGroup>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<AccountGroup> GetAsync(
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(GroupPath(groupId), null, cancellationToken).ConfigureAwait(false);
        return Require<AccountGroup>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<AccountGroup> CreateAsync(
        string workspaceId,
        string name,
        IEnumerable<string>? accountIds = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = workspaceId,
            ["name"] = name,
        };
        if (accountIds is not null)
        {
            body["account_ids"] = accountIds.ToList();
        }

        var response = await _http.PostAsync("/v1/account-groups", body, cancellationToken).ConfigureAwait(false);
        return Require<AccountGroup>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Rename the group.</summary>
    public async Task<AccountGroup> UpdateAsync(
        string groupId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["name"] = name };
        var response = await _http
            .RequestAsync(HttpMethod.Patch, GroupPath(groupId), body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AccountGroup>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Deletes the group only; its accounts stay connected.</summary>
    public async Task DeleteAsync(
        string groupId,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync(GroupPath(groupId), null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Replace the group's members with exactly <paramref name="accountIds"/>.</summary>
    public async Task<AccountGroup> SetMembersAsync(
        string groupId,
        IEnumerable<string> accountIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accountIds);

        var body = new Dictionary<string, object?> { ["account_ids"] = accountIds.ToList() };
        var response = await _http
            .PutAsync($"{GroupPath(groupId)}/members", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<AccountGroup>(FoPostHttpClient.Unwrap(response));
    }

    private static string GroupPath(string groupId) => $"/v1/account-groups/{Uri.EscapeDataString(groupId)}";
}
