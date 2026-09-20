using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace FoPost.Http;

/// <summary>
/// Internal transport: auth headers, JSON coding, the <c>{ "data": ... }</c>
/// envelope unwrap, and the 429 retry. One per <see cref="FoPostClient"/>.
/// </summary>
internal sealed class FoPostHttpClient : IDisposable
{
    internal const string UserAgent = "fopost-dotnet";
    private static readonly TimeSpan MaxRetryWait = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan FallbackRetryWait = TimeSpan.FromSeconds(1);

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly string? _apiKey;
    private readonly string? _bearerToken;

    /// <summary>Indirected so tests can replace the wait without touching the real clock.</summary>
    internal Func<TimeSpan, CancellationToken, Task> Delay { get; set; } =
        static (wait, cancellationToken) => Task.Delay(wait, cancellationToken);

    public FoPostHttpClient(FoPostClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(options.ApiKey) && string.IsNullOrEmpty(options.BearerToken))
        {
            throw new ArgumentException(
                "fopost: a credential is required — set ApiKey (or FOPOST_API_KEY) or BearerToken",
                nameof(options));
        }

        if (options.MaxRetries < 1)
        {
            throw new ArgumentException("fopost: MaxRetries must be at least 1", nameof(options));
        }

        _apiKey = options.ApiKey;
        _bearerToken = options.BearerToken;
        BaseUrl = NormalizeBaseUrl(options.BaseUrl);
        MaxRetries = options.MaxRetries;

        if (options.HttpClient is not null)
        {
            _http = options.HttpClient;
            _ownsHttpClient = false;
        }
        else if (options.HttpMessageHandler is not null)
        {
            _http = new HttpClient(options.HttpMessageHandler, disposeHandler: false)
            {
                Timeout = options.Timeout,
            };
            _ownsHttpClient = true;
        }
        else
        {
            _http = new HttpClient { Timeout = options.Timeout };
            _ownsHttpClient = true;
        }
    }

    public string BaseUrl { get; }

    public int MaxRetries { get; }

    internal static string NormalizeBaseUrl(string? baseUrl)
    {
        var trimmed = baseUrl?.Trim().TrimEnd('/');
        return string.IsNullOrEmpty(trimmed) ? FoPostClientOptions.DefaultBaseUrl : trimmed;
    }

    internal Uri BuildUri(string path, IReadOnlyDictionary<string, object?>? query)
    {
        var url = path.Contains("://", StringComparison.Ordinal)
            ? path
            : $"{BaseUrl}/{path.TrimStart('/')}";

        if (query is null || query.Count == 0)
        {
            return new Uri(url);
        }

        var builder = new StringBuilder();
        foreach (var (key, value) in query)
        {
            // A sequence repeats the bare parameter, which is how the API reads
            // a multi-valued filter like daily_metrics.
            var values = value is string || value is not IEnumerable<object?> items
                ? new[] { value }
                : items;

            foreach (var item in values)
            {
                var encoded = FormatQueryValue(item);
                if (encoded is null)
                {
                    continue;
                }

                builder.Append(builder.Length == 0 ? string.Empty : "&")
                    .Append(Uri.EscapeDataString(key))
                    .Append('=')
                    .Append(Uri.EscapeDataString(encoded));
            }
        }

        if (builder.Length == 0)
        {
            return new Uri(url);
        }

        var separator = url.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        return new Uri(url + separator + builder);
    }

    private static string? FormatQueryValue(object? value) => value switch
    {
        null => null,
        bool flag => flag ? "true" : "false",
        string text => text,
        DateTimeOffset moment => moment.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

    /// <summary>Send a request, retrying on 429, and return the decoded body.</summary>
    public async Task<JsonNode?> RequestAsync(
        HttpMethod method,
        string path,
        object? body = null,
        IReadOnlyDictionary<string, object?>? query = null,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(path, query);
        var payload = body is null ? null : JsonSerializer.Serialize(body, FoPostJson.Options);

        var attempt = 0;
        while (true)
        {
            attempt++;

            using var request = new HttpRequestMessage(method, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.UserAgent.ParseAdd(UserAgent);
            // A bearer token wins: the API key can arrive from the environment
            // without the caller meaning to send it alongside a session token.
            if (!string.IsNullOrEmpty(_bearerToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
            }
            else if (!string.IsNullOrEmpty(_apiKey))
            {
                request.Headers.TryAddWithoutValidation("X-API-Key", _apiKey);
            }
            if (payload is not null)
            {
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            }

            using var response = await _http
                .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < MaxRetries)
            {
                var wait = RetryAfter(response) ?? FallbackRetryWait;
                await Delay(wait > MaxRetryWait ? MaxRetryWait : wait, cancellationToken)
                    .ConfigureAwait(false);
                continue;
            }

            return await DecodeAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>PUT raw bytes to a presigned URL: no credential, only the headers the API returned.</summary>
    public async Task PutBytesAsync(
        string url,
        IEnumerable<KeyValuePair<string, string>> headers,
        byte[] data,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, new Uri(url))
        {
            Content = new ByteArrayContent(data),
        };
        foreach (var (name, value) in headers)
        {
            if (!request.Content.Headers.TryAddWithoutValidation(name, value))
            {
                request.Headers.TryAddWithoutValidation(name, value);
            }
        }

        using var response = await _http
            .SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw ErrorFor(response, raw);
        }
    }

    private static async Task<JsonNode?> DecodeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var status = (int)response.StatusCode;
        var raw = response.StatusCode == HttpStatusCode.NoContent
            ? string.Empty
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        JsonNode? body = null;
        var isJson = true;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                body = JsonNode.Parse(raw);
            }
            catch (JsonException)
            {
                isJson = false;
            }
        }

        if (response.IsSuccessStatusCode)
        {
            if (!isJson)
            {
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "no content type";
                throw new FoPostException($"Expected a JSON response, got {contentType}", status);
            }

            return body;
        }

        throw ErrorFor(response, raw);
    }

    private static FoPostException ErrorFor(HttpResponseMessage response, string raw)
    {
        var status = (int)response.StatusCode;
        JsonNode? body = null;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                body = JsonNode.Parse(raw);
            }
            catch (JsonException)
            {
                return new FoPostException(raw.Trim(), status);
            }
        }

        return ErrorFactory.FromResponse(status, body, RetryAfter(response));
    }

    private static TimeSpan? RetryAfter(HttpResponseMessage response)
    {
        var header = response.Headers.RetryAfter;
        if (header is null)
        {
            return null;
        }

        if (header.Delta is { } delta)
        {
            return delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
        }

        if (header.Date is { } date)
        {
            var remaining = date - DateTimeOffset.UtcNow;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }

        return null;
    }

    public Task<JsonNode?> GetAsync(
        string path,
        IReadOnlyDictionary<string, object?>? query = null,
        CancellationToken cancellationToken = default) =>
        RequestAsync(HttpMethod.Get, path, null, query, cancellationToken);

    public Task<JsonNode?> PostAsync(
        string path,
        object? body = null,
        CancellationToken cancellationToken = default) =>
        RequestAsync(HttpMethod.Post, path, body, null, cancellationToken);

    public Task<JsonNode?> PutAsync(
        string path,
        object? body = null,
        CancellationToken cancellationToken = default) =>
        RequestAsync(HttpMethod.Put, path, body, null, cancellationToken);

    public Task<JsonNode?> DeleteAsync(
        string path,
        object? body = null,
        CancellationToken cancellationToken = default) =>
        RequestAsync(HttpMethod.Delete, path, body, null, cancellationToken);

    /// <summary>
    /// Peel the <c>{ "data": ... }</c> envelope the API wraps most responses in.
    /// Some endpoints return the resource bare, so it is peeled only when present.
    /// </summary>
    public static JsonNode? Unwrap(JsonNode? body)
    {
        if (body is JsonObject obj && obj.TryGetPropertyValue("data", out var data))
        {
            return data;
        }

        return body;
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _http.Dispose();
        }
    }
}
