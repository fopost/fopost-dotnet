using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Sequences</c> — a series of messages, each a delay after the one before,
/// walked per enrolled contact.
/// </summary>
/// <remarks>
/// The messaging window applies to every step. A step that comes due outside it is skipped
/// rather than sent, and the enrollment carries on — so someone can complete a sequence
/// having received only some of its messages.
/// <para>
/// Reading needs the <c>inbox</c> scope; <see cref="EnrollAsync"/> and
/// <see cref="UnenrollAsync"/> also need <c>publish</c>.
/// </para>
/// </remarks>
public sealed class SequencesResource
{
    private readonly FoPostHttpClient _http;

    internal SequencesResource(FoPostHttpClient http) => _http = http;

    /// <summary>One page of sequences.</summary>
    public async Task<SequencePage> ListAsync(
        ListSequencesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options?.WorkspaceId,
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
        };
        var body = await _http.GetAsync("/v1/sequences", query, cancellationToken).ConfigureAwait(false);
        return new SequencePage(
            ToList<Sequence>(FoPostHttpClient.Unwrap(body)),
            BroadcastsResource.PaginationOf(body));
    }

    /// <summary>One sequence.</summary>
    public async Task<Sequence> GetAsync(string sequenceId, CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync($"/v1/sequences/{sequenceId}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Sequence>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Write a sequence. Creating one enrolls nobody.</summary>
    public async Task<Sequence> CreateAsync(
        CreateSequenceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["account_id"] = options.AccountId,
            ["name"] = options.Name,
            ["steps"] = options.Steps,
            ["status"] = options.Status,
        };
        var response = await _http.PostAsync("/v1/sequences", Compact(body), cancellationToken)
            .ConfigureAwait(false);
        return Require<Sequence>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Patch a sequence. Pausing stops every enrollment from firing without ending any of
    /// them; resuming picks them up where they stood.
    /// </summary>
    public async Task<Sequence> UpdateAsync(
        string sequenceId,
        UpdateSequenceOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Name.IsSet)
        {
            body["name"] = options.Name.Value;
        }
        if (options.Steps.IsSet)
        {
            body["steps"] = options.Steps.Value;
        }
        if (options.Status.IsSet)
        {
            body["status"] = options.Status.Value;
        }

        var response = await _http
            .RequestAsync(new HttpMethod("PATCH"), $"/v1/sequences/{sequenceId}", body, null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Sequence>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Put contacts on the sequence, by id or by audience. Re-enrolling someone restarts
    /// their walk from the first step rather than running two in parallel. Needs
    /// <c>publish</c> as well as <c>inbox</c>.
    /// </summary>
    public async Task<Enrolled> EnrollAsync(
        string sequenceId,
        EnrollOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["contact_ids"] = options.ContactIds,
            ["audience"] = options.Audience,
        };
        var response = await _http
            .PostAsync($"/v1/sequences/{sequenceId}/enroll", Compact(body), cancellationToken)
            .ConfigureAwait(false);
        return Require<Enrolled>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Take contacts off the sequence. Nothing further fires for them. Needs <c>publish</c>.
    /// </summary>
    public async Task<Unenrolled> UnenrollAsync(
        string sequenceId,
        IReadOnlyList<string> contactIds,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["contact_ids"] = contactIds };
        var response = await _http
            .PostAsync($"/v1/sequences/{sequenceId}/unenroll", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<Unenrolled>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>
    /// Who is on the sequence, what step they are at, and when the next one is due.
    /// </summary>
    public async Task<EnrollmentPage> EnrollmentsAsync(
        string sequenceId,
        ListEnrollmentsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["page"] = options?.Page,
            ["per_page"] = options?.PerPage,
        };
        var body = await _http
            .GetAsync($"/v1/sequences/{sequenceId}/enrollments", query, cancellationToken)
            .ConfigureAwait(false);
        return new EnrollmentPage(
            ToList<Enrollment>(FoPostHttpClient.Unwrap(body)),
            BroadcastsResource.PaginationOf(body));
    }

    /// <summary>Drop the keys the caller left unset, so a create stays minimal.</summary>
    private static Dictionary<string, object?> Compact(Dictionary<string, object?> body)
    {
        var compacted = new Dictionary<string, object?>();
        foreach (var (key, value) in body)
        {
            if (value is not null)
            {
                compacted[key] = value;
            }
        }
        return compacted;
    }

    /// <summary>Remove a sequence and every enrollment on it.</summary>
    public async Task DeleteAsync(string sequenceId, CancellationToken cancellationToken = default) =>
        await _http.DeleteAsync($"/v1/sequences/{sequenceId}", null, cancellationToken).ConfigureAwait(false);
}
