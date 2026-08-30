using System.Text.Json.Nodes;
using FoPost.Http;
using FoPost.Resources;

namespace FoPost;

/// <summary>
/// Client for the FoPost API.
/// </summary>
/// <example>
/// <code>
/// using var client = new FoPostClient(Environment.GetEnvironmentVariable("FOPOST_API_KEY")!);
///
/// var accounts = await client.Accounts.ListAsync("9b2f6c1e-…");
/// var post = await client.Posts.CreateAsync("9b2f6c1e-…", "Hello from .NET", accounts.Select(a => a.Id));
/// await client.Posts.PublishAsync(post.Id);
/// </code>
/// </example>
/// <remarks>
/// The key falls back to the <c>FOPOST_API_KEY</c> environment variable.
/// Requests that come back 429 are retried up to
/// <see cref="FoPostClientOptions.MaxRetries"/> attempts, waiting for the
/// interval the API asks for in <c>Retry-After</c>.
/// </remarks>
public sealed class FoPostClient : IDisposable
{
    private readonly FoPostHttpClient _http;

    public FoPostClient(FoPostClientOptions? options = null)
    {
        _http = new FoPostHttpClient(options ?? new FoPostClientOptions());

        Posts = new PostsResource(_http);
        Accounts = new AccountsResource(_http);
        Workspaces = new WorkspacesResource(_http);
        Labels = new LabelsResource(_http);
        Ai = new AiResource(_http);
    }

    public FoPostClient(string apiKey)
        : this(new FoPostClientOptions { ApiKey = apiKey })
    {
    }

    public PostsResource Posts { get; }

    public AccountsResource Accounts { get; }

    public WorkspacesResource Workspaces { get; }

    public LabelsResource Labels { get; }

    public AiResource Ai { get; }

    public string BaseUrl => _http.BaseUrl;

    internal FoPostHttpClient Transport => _http;

    /// <summary>
    /// Call an endpoint the SDK does not wrap yet. Returns the decoded body,
    /// envelope and all, and raises the same exceptions as everything else.
    /// </summary>
    public Task<JsonNode?> RequestAsync(
        HttpMethod method,
        string path,
        object? body = null,
        IReadOnlyDictionary<string, object?>? query = null,
        CancellationToken cancellationToken = default) =>
        _http.RequestAsync(method, path, body, query, cancellationToken);

    public void Dispose() => _http.Dispose();
}
