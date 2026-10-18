using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Automations</c> — triggers and the graphs of steps they run, their saved versions,
/// and the record of every run.
/// </summary>
/// <remarks>Every call needs the <c>automations</c> scope.</remarks>
public sealed class AutomationsResource
{
    private readonly FoPostHttpClient _http;

    internal AutomationsResource(FoPostHttpClient http) => _http = http;

    private static string AutomationPath(string automationId) =>
        $"/v1/automations/{Uri.EscapeDataString(automationId)}";

    public async Task<IReadOnlyList<Automation>> ListAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/automations", null, cancellationToken).ConfigureAwait(false);
        return ToList<Automation>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Create an automation. The answer carries the webhook-trigger <see cref="Automation.Secret"/>; keep it.</summary>
    public async Task<Automation> CreateAsync(
        CreateAutomationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PostAsync("/v1/automations", options, cancellationToken).ConfigureAwait(false);
        return Require<Automation>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Counts across every automation, and the most recent runs.</summary>
    public async Task<AutomationStats> StatsAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/automations/stats", null, cancellationToken).ConfigureAwait(false);
        return Require<AutomationStats>(FoPostHttpClient.Unwrap(body));
    }

    public async Task<Automation> GetAsync(
        string automationId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync(AutomationPath(automationId), null, cancellationToken).ConfigureAwait(false);
        return Require<Automation>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Partial update: only the fields you set are sent. A new graph saves a new version.</summary>
    public async Task<Automation> UpdateAsync(
        string automationId,
        UpdateAutomationOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var response = await _http.PutAsync(AutomationPath(automationId), options, cancellationToken)
            .ConfigureAwait(false);
        return Require<Automation>(FoPostHttpClient.Unwrap(response));
    }

    public async Task DeleteAsync(
        string automationId,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync(AutomationPath(automationId), null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Flip the automation on or off.</summary>
    public async Task<AutomationToggle> ToggleAsync(
        string automationId,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"{AutomationPath(automationId)}/toggle", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AutomationToggle>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Saved versions of the graph, newest first, each with what it added and removed.</summary>
    public async Task<OffsetPage<AutomationVersion>> ListVersionsAsync(
        string automationId,
        int? page = null,
        int? perPage = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["page"] = page, ["per_page"] = perPage };
        var body = await _http
            .GetAsync($"{AutomationPath(automationId)}/versions", query, cancellationToken)
            .ConfigureAwait(false);
        return new OffsetPage<AutomationVersion>(
            ToList<AutomationVersion>(FoPostHttpClient.Unwrap(body)),
            ReadOffsetMeta(body));
    }

    /// <summary>One saved version, with its full graph.</summary>
    public async Task<AutomationVersion> GetVersionAsync(
        string automationId,
        int version,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"{AutomationPath(automationId)}/versions/{version}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AutomationVersion>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Make an older graph current again. It is saved as a new version, so nothing is lost.</summary>
    public async Task<AutomationRestore> RestoreVersionAsync(
        string automationId,
        int version,
        CancellationToken cancellationToken = default)
    {
        var response = await _http
            .PostAsync($"{AutomationPath(automationId)}/versions/{version}/restore", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AutomationRestore>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Runs newest first.</summary>
    public async Task<OffsetPage<AutomationRun>> ListRunsAsync(
        string automationId,
        int? page = null,
        int? perPage = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["page"] = page, ["per_page"] = perPage };
        var body = await _http
            .GetAsync($"{AutomationPath(automationId)}/runs", query, cancellationToken)
            .ConfigureAwait(false);
        return new OffsetPage<AutomationRun>(
            ToList<AutomationRun>(FoPostHttpClient.Unwrap(body)),
            ReadOffsetMeta(body));
    }

    /// <summary>One run with its per-step logs.</summary>
    public async Task<AutomationRun> GetRunAsync(
        string automationId,
        long runId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"{AutomationPath(automationId)}/runs/{runId}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<AutomationRun>(FoPostHttpClient.Unwrap(body));
    }
}
