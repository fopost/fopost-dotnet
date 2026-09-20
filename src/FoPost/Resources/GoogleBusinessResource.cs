using System.Text.Json.Nodes;
using FoPost.Http;

namespace FoPost.Resources;

/// <summary>
/// <c>client.GoogleBusiness</c> — manage a connected Google Business Profile
/// location: the profile itself, attributes, food menus, services, photos,
/// place action links, verification and performance.
/// </summary>
/// <remarks>
/// Google grants Business Profile API access per project. Until that grant
/// lands on a deployment every call here throws a 503 <c>configuration_error</c>.
/// Responses relay Google's own shape, field for field, so they come back as
/// <see cref="JsonNode"/> rather than models we would have to keep chasing.
/// </remarks>
public sealed class GoogleBusinessResource
{
    /// <summary>The daily metrics fetched when a caller names none.</summary>
    public static readonly IReadOnlyList<string> DefaultDailyMetrics = new[]
    {
        "BUSINESS_IMPRESSIONS_DESKTOP_MAPS",
        "BUSINESS_IMPRESSIONS_DESKTOP_SEARCH",
        "BUSINESS_IMPRESSIONS_MOBILE_MAPS",
        "BUSINESS_IMPRESSIONS_MOBILE_SEARCH",
        "CALL_CLICKS",
        "WEBSITE_CLICKS",
        "BUSINESS_DIRECTION_REQUESTS",
    };

    private readonly FoPostHttpClient _http;

    internal GoogleBusinessResource(FoPostHttpClient http) => _http = http;

    /// <summary>The connected location, in the Business Information shape.</summary>
    public async Task<JsonNode?> GetLocationAsync(
        string accountId,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/location"), null, cancellationToken).ConfigureAwait(false));

    /// <summary>
    /// Patch the profile. Only the keys the dictionary carries change, and a
    /// null value clears that field. Keys are the API's own snake_case names:
    /// <c>title</c>, <c>description</c>, <c>website_uri</c>, <c>primary_phone</c>,
    /// <c>additional_phones</c>, <c>store_code</c> and <c>regular_hours</c>.
    /// </summary>
    public async Task<JsonNode?> UpdateLocationAsync(
        string accountId,
        IReadOnlyDictionary<string, object?> fields,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(await _http
            .RequestAsync(HttpMethod.Patch, Path(accountId, "/location"), fields, null, cancellationToken)
            .ConfigureAwait(false));

    /// <summary>
    /// The attribute values set on the location, or, with <paramref name="available"/>,
    /// the attributes Google offers for its category and region.
    /// </summary>
    public async Task<JsonNode?> GetAttributesAsync(
        string accountId,
        bool available = false,
        string? categoryName = null,
        string? regionCode = null,
        string? languageCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["available"] = available ? "true" : null,
            ["category_name"] = categoryName,
            ["region_code"] = regionCode,
            ["language_code"] = languageCode,
        };
        return FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/attributes"), query, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Only the named attributes change; every other one is left alone.</summary>
    public async Task<JsonNode?> UpdateAttributesAsync(
        string accountId,
        IEnumerable<IReadOnlyDictionary<string, object?>> attributes,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["attributes"] = attributes };
        return FoPostHttpClient.Unwrap(await _http
            .RequestAsync(HttpMethod.Patch, Path(accountId, "/attributes"), body, null, cancellationToken)
            .ConfigureAwait(false));
    }

    /// <summary>The location's food menus.</summary>
    public async Task<JsonNode?> GetMenusAsync(
        string accountId,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/menus"), null, cancellationToken).ConfigureAwait(false));

    /// <summary>Google has no per-section patch, so the whole menu set is replaced.</summary>
    public async Task<JsonNode?> ReplaceMenusAsync(
        string accountId,
        IEnumerable<object> menus,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(await _http
            .PutAsync(Path(accountId, "/menus"), new Dictionary<string, object?> { ["menus"] = menus },
                cancellationToken)
            .ConfigureAwait(false));

    /// <summary>The location's service list.</summary>
    public async Task<JsonNode?> GetServicesAsync(
        string accountId,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/services"), null, cancellationToken).ConfigureAwait(false));

    /// <summary>Replace the whole service list.</summary>
    public async Task<JsonNode?> ReplaceServicesAsync(
        string accountId,
        IEnumerable<object> serviceItems,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(await _http
            .PutAsync(Path(accountId, "/services"),
                new Dictionary<string, object?> { ["service_items"] = serviceItems }, cancellationToken)
            .ConfigureAwait(false));

    /// <summary>The photos on the location.</summary>
    public async Task<JsonNode?> ListMediaAsync(
        string accountId,
        int? pageSize = null,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["page_size"] = pageSize, ["page_token"] = pageToken };
        return FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/media"), query, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Add a photo from the media library. The asset has to be in a workspace
    /// the caller can reach, and JPEG or PNG.
    /// </summary>
    public async Task<JsonNode?> AddMediaAsync(
        string accountId,
        string mediaId,
        string category = "ADDITIONAL",
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["media_id"] = mediaId,
            ["category"] = category,
        };
        if (description is not null)
        {
            body["description"] = description;
        }

        return FoPostHttpClient.Unwrap(
            await _http.PostAsync(Path(accountId, "/media"), body, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Remove a photo by the media key Google returned.</summary>
    public async Task<JsonNode?> DeleteMediaAsync(
        string accountId,
        string mediaKey,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(await _http
            .DeleteAsync(Path(accountId, $"/media/{Uri.EscapeDataString(mediaKey)}"), null, cancellationToken)
            .ConfigureAwait(false));

    /// <summary>The Book, Order and Reserve links on the listing.</summary>
    public async Task<JsonNode?> ListPlaceActionsAsync(
        string accountId,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/place-actions"), null, cancellationToken).ConfigureAwait(false));

    /// <summary>Add an action link to the listing.</summary>
    public async Task<JsonNode?> CreatePlaceActionAsync(
        string accountId,
        string uri,
        string placeActionType,
        bool? isPreferred = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["uri"] = uri,
            ["place_action_type"] = placeActionType,
        };
        if (isPreferred is not null)
        {
            body["is_preferred"] = isPreferred;
        }

        return FoPostHttpClient.Unwrap(
            await _http.PostAsync(Path(accountId, "/place-actions"), body, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>Patch one action link; a null argument is left alone.</summary>
    public async Task<JsonNode?> UpdatePlaceActionAsync(
        string accountId,
        string linkId,
        string? uri = null,
        bool? isPreferred = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>();
        if (uri is not null)
        {
            body["uri"] = uri;
        }

        if (isPreferred is not null)
        {
            body["is_preferred"] = isPreferred;
        }

        return FoPostHttpClient.Unwrap(await _http
            .RequestAsync(HttpMethod.Patch, Path(accountId, $"/place-actions/{Uri.EscapeDataString(linkId)}"),
                body, null, cancellationToken)
            .ConfigureAwait(false));
    }

    /// <summary>Remove one action link.</summary>
    public async Task<JsonNode?> DeletePlaceActionAsync(
        string accountId,
        string linkId,
        CancellationToken cancellationToken = default) =>
        FoPostHttpClient.Unwrap(await _http
            .DeleteAsync(Path(accountId, $"/place-actions/{Uri.EscapeDataString(linkId)}"), null, cancellationToken)
            .ConfigureAwait(false));

    /// <summary>The ways Google will let this location be verified.</summary>
    public async Task<JsonNode?> GetVerificationOptionsAsync(
        string accountId,
        string? languageCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?> { ["language_code"] = languageCode };
        return FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/verification"), query, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Start a verification. <paramref name="method"/> is <c>ADDRESS</c>,
    /// <c>EMAIL</c>, <c>PHONE_CALL</c>, <c>SMS</c>, <c>AUTO</c> or
    /// <c>VETTED_PARTNER</c>; the response names the pending verification.
    /// </summary>
    public async Task<JsonNode?> StartVerificationAsync(
        string accountId,
        string method,
        string? languageCode = null,
        string? phoneNumber = null,
        string? emailAddress = null,
        string? mailerContactName = null,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["method"] = method };
        Add(body, "language_code", languageCode);
        Add(body, "phone_number", phoneNumber);
        Add(body, "email_address", emailAddress);
        Add(body, "mailer_contact_name", mailerContactName);

        return FoPostHttpClient.Unwrap(await _http
            .PostAsync(Path(accountId, "/verification/start"), body, cancellationToken)
            .ConfigureAwait(false));
    }

    /// <summary>Complete a pending verification with the PIN Google sent.</summary>
    public async Task<JsonNode?> CompleteVerificationAsync(
        string accountId,
        string verificationName,
        string pin,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?>
        {
            ["verification_name"] = verificationName,
            ["pin"] = pin,
        };
        return FoPostHttpClient.Unwrap(await _http
            .PostAsync(Path(accountId, "/verification/complete"), body, cancellationToken)
            .ConfigureAwait(false));
    }

    /// <summary>
    /// Daily impressions, calls, direction requests and clicks for the range.
    /// A null or empty <paramref name="dailyMetrics"/> leaves the API's own set.
    /// </summary>
    public async Task<JsonNode?> GetPerformanceAsync(
        string accountId,
        string startDate,
        string endDate,
        IEnumerable<string>? dailyMetrics = null,
        CancellationToken cancellationToken = default)
    {
        var metrics = dailyMetrics?.Cast<object?>().ToArray();
        var query = new Dictionary<string, object?>
        {
            ["start_date"] = startDate,
            ["end_date"] = endDate,
            ["daily_metrics"] = metrics is { Length: > 0 } ? metrics : null,
        };
        return FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/performance"), query, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>The search terms people used to find the listing, by month.</summary>
    public async Task<JsonNode?> GetSearchKeywordsAsync(
        string accountId,
        string startDate,
        string endDate,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["keywords"] = "true",
            ["start_date"] = startDate,
            ["end_date"] = endDate,
            ["page_token"] = pageToken,
        };
        return FoPostHttpClient.Unwrap(
            await _http.GetAsync(Path(accountId, "/performance"), query, cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    /// Hand the location to another workspace the caller owns. The connection
    /// and every row keyed to it move in one transaction.
    /// </summary>
    public async Task<JsonNode?> AssignAsync(
        string accountId,
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var body = new Dictionary<string, object?> { ["workspace_id"] = workspaceId };
        return FoPostHttpClient.Unwrap(
            await _http.PostAsync(Path(accountId, "/assign"), body, cancellationToken).ConfigureAwait(false));
    }

    private static string Path(string accountId, string suffix) =>
        $"/v1/accounts/{Uri.EscapeDataString(accountId)}/gbp{suffix}";

    private static void Add(IDictionary<string, object?> body, string key, string? value)
    {
        if (value is not null)
        {
            body[key] = value;
        }
    }
}
