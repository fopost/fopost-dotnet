using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Whatsapp</c> — a WhatsApp Business connection, on a number the
/// customer already owns.
/// </summary>
/// <remarks>
/// The platform owns templates, flows, the business profile and the commerce
/// settings, so every method here is a live read or write against the customer's
/// own WhatsApp Business Account. Nothing is cached, and all of it answers 503
/// until WhatsApp is set up on the deployment. Every method needs the
/// <c>accounts</c> scope, except the sandbox, which sends a template and needs
/// <c>publish</c>.
/// </remarks>
public sealed class WhatsappResource
{
    private readonly FoPostHttpClient _http;

    internal WhatsappResource(FoPostHttpClient http) => _http = http;

    private static string Base(string accountId) => $"/v1/accounts/{accountId}/whatsapp";

    // ─── Profile ──────────────────────────────────────────────────────────

    /// <summary>The profile on the number, plus its quality rating and limit tier.</summary>
    public async Task<WhatsappProfile> GetProfileAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"{Base(accountId)}/profile", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappProfile>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>A partial update: omitted fields keep their value.</summary>
    public async Task<WhatsappProfile> UpdateProfileAsync(
        string accountId,
        UpdateWhatsappProfileOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http
            .RequestAsync(HttpMethod.Patch, $"{Base(accountId)}/profile", options, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappProfile>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// A review, not a write: the number keeps its old name until it passes.
    /// </summary>
    public Task RequestDisplayNameAsync(
        string accountId,
        string displayName,
        CancellationToken cancellationToken = default) =>
        _http.PostAsync(
            $"{Base(accountId)}/profile/display-name",
            new Dictionary<string, object?> { ["display_name"] = displayName },
            cancellationToken);

    /// <summary>Sets the public username on the number.</summary>
    public async Task<WhatsappProfile> SetUsernameAsync(
        string accountId,
        string username,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PutAsync(
                $"{Base(accountId)}/profile/username",
                new Dictionary<string, object?> { ["username"] = username },
                cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappProfile>(FoPostHttpClient.Unwrap(response));
    }

    // ─── Templates ────────────────────────────────────────────────────────

    /// <summary>Every template on the account, with its review status.</summary>
    public async Task<IReadOnlyList<WhatsappTemplate>> ListTemplatesAsync(
        string accountId,
        string? after = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["after"] = after };
        var response = await _http.GetAsync($"{Base(accountId)}/templates", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<WhatsappTemplate>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The pre-written templates the platform offers, for adapting.</summary>
    public async Task<IReadOnlyList<JsonNode?>> ListTemplateLibraryAsync(
        string accountId,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["search"] = search };
        var response = await _http
            .GetAsync($"{Base(accountId)}/templates/library", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<JsonNode?>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One template and the review status it currently has.</summary>
    public async Task<WhatsappTemplate> GetTemplateAsync(
        string accountId,
        string templateId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{Base(accountId)}/templates/{templateId}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappTemplate>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Files a template for review. The result carries the status the platform
    /// assigned, which is <c>PENDING</c> on a normal submission.
    /// </summary>
    public async Task<WhatsappTemplate> CreateTemplateAsync(
        string accountId,
        CreateWhatsappTemplateOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http
            .PostAsync($"{Base(accountId)}/templates", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappTemplate>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Creates a template from one of the platform's library entries.</summary>
    public async Task<WhatsappTemplate> ImportTemplateAsync(
        string accountId,
        ImportWhatsappTemplateOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http
            .PostAsync($"{Base(accountId)}/templates/import", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappTemplate>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Edits a template. The name cannot change.</summary>
    public async Task<WhatsappTemplate> UpdateTemplateAsync(
        string accountId,
        string templateId,
        UpdateWhatsappTemplateOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http
            .RequestAsync(
                HttpMethod.Patch,
                $"{Base(accountId)}/templates/{templateId}",
                options,
                null,
                cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappTemplate>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>The name is required: it is what the platform deletes by.</summary>
    public Task DeleteTemplateAsync(
        string accountId,
        string templateId,
        string name,
        CancellationToken cancellationToken = default) =>
        _http.RequestAsync(
            HttpMethod.Delete,
            $"{Base(accountId)}/templates/{templateId}",
            null,
            new Dictionary<string, object?> { ["name"] = name },
            cancellationToken);

    // ─── Groups ───────────────────────────────────────────────────────────

    /// <summary>The groups this number created.</summary>
    public async Task<IReadOnlyList<WhatsappGroup>> ListGroupsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"{Base(accountId)}/groups", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<WhatsappGroup>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Participation is invite-only: send the invite link, there is no add.
    /// </summary>
    public async Task<WhatsappGroup> CreateGroupAsync(
        string accountId,
        WhatsappGroupOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http.PostAsync($"{Base(accountId)}/groups", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappGroup>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One group and its participant count.</summary>
    public async Task<WhatsappGroup> GetGroupAsync(
        string accountId,
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{Base(accountId)}/groups/{groupId}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappGroup>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Changes a group's subject or description.</summary>
    public async Task<WhatsappGroup> UpdateGroupAsync(
        string accountId,
        string groupId,
        WhatsappGroupOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http
            .RequestAsync(
                HttpMethod.Patch, $"{Base(accountId)}/groups/{groupId}", options, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappGroup>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Removes the group.</summary>
    public Task DeleteGroupAsync(
        string accountId,
        string groupId,
        CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"{Base(accountId)}/groups/{groupId}", null, cancellationToken);

    /// <summary>The link someone joins the group with.</summary>
    public async Task<string?> GetGroupInviteLinkAsync(
        string accountId,
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{Base(accountId)}/groups/{groupId}/invite-link", null, cancellationToken)
            .ConfigureAwait(false);
        return InviteLinkOf(response);
    }

    /// <summary>Issues a new link and invalidates the old one.</summary>
    public async Task<string?> ResetGroupInviteLinkAsync(
        string accountId,
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"{Base(accountId)}/groups/{groupId}/invite-link", null, cancellationToken)
            .ConfigureAwait(false);
        return InviteLinkOf(response);
    }

    private static string? InviteLinkOf(JsonNode? response) =>
        FoPostHttpClient.Unwrap(response) is JsonObject obj &&
        obj.TryGetPropertyValue("inviteLink", out var link)
            ? link?.GetValue<string?>()
            : null;

    /// <summary>Removes people from the group. There is no matching add.</summary>
    public Task RemoveGroupParticipantsAsync(
        string accountId,
        string groupId,
        IEnumerable<string> users,
        CancellationToken cancellationToken = default) =>
        _http.DeleteAsync(
            $"{Base(accountId)}/groups/{groupId}/participants",
            new Dictionary<string, object?> { ["users"] = users.ToList() },
            cancellationToken);

    // ─── Blocking ─────────────────────────────────────────────────────────

    /// <summary>The numbers this account has blocked.</summary>
    public async Task<IReadOnlyList<string>> ListBlockedAsync(
        string accountId,
        string? after = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["after"] = after };
        var response = await _http.GetAsync($"{Base(accountId)}/block", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<string>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Blocks up to 100 numbers, and names the ones the platform refused.</summary>
    public async Task<WhatsappBlockResult> BlockUsersAsync(
        string accountId,
        IEnumerable<string> users,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["users"] = users.ToList() };
        var response = await _http.PostAsync($"{Base(accountId)}/block", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappBlockResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Unblocks up to 100 numbers.</summary>
    public async Task<WhatsappBlockResult> UnblockUsersAsync(
        string accountId,
        IEnumerable<string> users,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["users"] = users.ToList() };
        var response = await _http.DeleteAsync($"{Base(accountId)}/block", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappBlockResult>(FoPostHttpClient.Unwrap(response));
    }

    // ─── Commerce ─────────────────────────────────────────────────────────

    /// <summary>Whether the cart and catalog show on the number.</summary>
    public async Task<WhatsappCommerceSettings> GetCommerceSettingsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"{Base(accountId)}/commerce", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappCommerceSettings>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Turns the cart or the catalog on or off.</summary>
    public async Task<WhatsappCommerceSettings> UpdateCommerceSettingsAsync(
        string accountId,
        UpdateWhatsappCommerceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http
            .RequestAsync(HttpMethod.Patch, $"{Base(accountId)}/commerce", options, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappCommerceSettings>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Points the number at a catalog the customer already owns.</summary>
    public async Task<WhatsappCommerceSettings> LinkCatalogAsync(
        string accountId,
        string catalogId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["catalog_id"] = catalogId };
        var response = await _http
            .PostAsync($"{Base(accountId)}/commerce/catalog", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappCommerceSettings>(FoPostHttpClient.Unwrap(response));
    }

    // ─── Flows ────────────────────────────────────────────────────────────

    /// <summary>The in-chat forms on this account, with their validation errors.</summary>
    public async Task<IReadOnlyList<WhatsappFlow>> ListFlowsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"{Base(accountId)}/flows", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<WhatsappFlow>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>One flow and what the platform found wrong with it.</summary>
    public async Task<WhatsappFlow> GetFlowAsync(
        string accountId,
        string flowId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{Base(accountId)}/flows/{flowId}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappFlow>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Creates a draft flow; its screens are uploaded separately.</summary>
    public async Task<WhatsappFlow> CreateFlowAsync(
        string accountId,
        CreateWhatsappFlowOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http.PostAsync($"{Base(accountId)}/flows", options, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappFlow>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Changes a flow's name, categories or endpoint.</summary>
    public async Task<WhatsappFlow> UpdateFlowAsync(
        string accountId,
        string flowId,
        UpdateWhatsappFlowOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var response = await _http
            .RequestAsync(
                HttpMethod.Patch, $"{Base(accountId)}/flows/{flowId}", options, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappFlow>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Drafts only; a published flow is deprecated instead.</summary>
    public Task DeleteFlowAsync(
        string accountId,
        string flowId,
        CancellationToken cancellationToken = default) =>
        _http.DeleteAsync($"{Base(accountId)}/flows/{flowId}", null, cancellationToken);

    /// <summary>
    /// Replaces the flow's screens. The platform answers with its validation
    /// errors rather than refusing, so they come back as data.
    /// </summary>
    public async Task<WhatsappFlowJsonResult> UploadFlowJsonAsync(
        string accountId,
        string flowId,
        JsonNode flowJson,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["flow_json"] = flowJson };
        var response = await _http
            .PutAsync($"{Base(accountId)}/flows/{flowId}/json", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappFlowJsonResult>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Makes the flow sendable. A published flow can no longer be deleted.</summary>
    public async Task<WhatsappFlow> PublishFlowAsync(
        string accountId,
        string flowId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"{Base(accountId)}/flows/{flowId}/publish", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappFlow>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Retires a published flow.</summary>
    public async Task<WhatsappFlow> DeprecateFlowAsync(
        string accountId,
        string flowId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"{Base(accountId)}/flows/{flowId}/deprecate", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappFlow>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>What people submitted through this account's flows.</summary>
    public async Task<IReadOnlyList<WhatsappFlowResponse>> ListFlowResponsesAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{Base(accountId)}/flows/responses", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<WhatsappFlowResponse>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Whether a business public key is registered, and how it was judged.</summary>
    public async Task<WhatsappEncryptionKeyStatus> GetEncryptionKeyStatusAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .GetAsync($"{Base(accountId)}/flows/encryption-key", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappEncryptionKeyStatus>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Registers the public half of the key the platform encrypts a flow
    /// endpoint's payloads with. The private half stays with the customer.
    /// </summary>
    public async Task<WhatsappEncryptionKeyStatus> SetEncryptionKeyAsync(
        string accountId,
        string businessPublicKey,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["business_public_key"] = businessPublicKey };
        var response = await _http
            .PutAsync($"{Base(accountId)}/flows/encryption-key", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappEncryptionKeyStatus>(FoPostHttpClient.Unwrap(response));
    }

    // ─── Account state and sandbox ────────────────────────────────────────

    /// <summary>The account review state and the number's quality and limit tier.</summary>
    public async Task<JsonNode?> GetAccountEventsAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http.GetAsync($"{Base(accountId)}/events", null, cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(response);
    }

    /// <summary>Sandbox invitations for a workspace.</summary>
    public async Task<IReadOnlyList<WhatsappSandboxSession>> ListSandboxSessionsAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["workspaceId"] = workspaceId };
        var response = await _http
            .GetAsync("/v1/whatsapp/sandbox/sessions", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<WhatsappSandboxSession>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Invites one tester to the platform-owned test number. Inviting sends a
    /// template, so it needs the <c>publish</c> scope.
    /// </summary>
    public async Task<WhatsappSandboxSession> CreateSandboxSessionAsync(
        string workspaceId,
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["workspaceId"] = workspaceId,
            ["phoneNumber"] = phoneNumber,
        };
        var response = await _http
            .PostAsync("/v1/whatsapp/sandbox/sessions", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<WhatsappSandboxSession>(FoPostHttpClient.Unwrap(response));
    }
}
