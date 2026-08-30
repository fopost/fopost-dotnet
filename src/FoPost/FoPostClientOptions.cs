namespace FoPost;

/// <summary>How a <see cref="FoPostClient"/> talks to the API.</summary>
public sealed class FoPostClientOptions
{
    /// <summary>The production API. Override it for staging or a self-hosted deployment.</summary>
    public const string DefaultBaseUrl = "https://api.fopost.com";

    public const int DefaultMaxRetries = 3;

    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// API key from the dashboard, under Settings → API Keys. Sent as
    /// <c>X-API-Key</c>, and falls back to the <c>FOPOST_API_KEY</c>
    /// environment variable.
    /// </summary>
    public string? ApiKey { get; set; } = Environment.GetEnvironmentVariable("FOPOST_API_KEY");

    /// <summary>
    /// A dashboard session token, sent as <c>Authorization: Bearer</c>. Only
    /// needed for the handful of endpoints that do not accept an API key —
    /// see <see cref="Resources.AiResource"/>. Set it and it wins over
    /// <see cref="ApiKey"/>, which may have come from the environment
    /// unintentionally.
    /// </summary>
    public string? BearerToken { get; set; }

    public string BaseUrl { get; set; } = DefaultBaseUrl;

    /// <summary>Ignored when <see cref="HttpClient"/> is supplied — that client's timeout wins.</summary>
    public TimeSpan Timeout { get; set; } = DefaultTimeout;

    /// <summary>
    /// Total attempts for a request the API answers with 429, including the
    /// first. The wait comes from the <c>Retry-After</c> header.
    /// </summary>
    public int MaxRetries { get; set; } = DefaultMaxRetries;

    /// <summary>
    /// Supply your own client — an <c>IHttpClientFactory</c> one, say. The SDK
    /// then neither configures nor disposes it.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>A handler to build the internal client on. Ignored when <see cref="HttpClient"/> is set.</summary>
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}
