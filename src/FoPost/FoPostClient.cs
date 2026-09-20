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
        AccountGroups = new AccountGroupsResource(_http);
        Workspaces = new WorkspacesResource(_http);
        Labels = new LabelsResource(_http);
        Activity = new ActivityResource(_http);
        Ai = new AiResource(_http);
        Inbox = new InboxResource(_http);
        Contacts = new ContactsResource(_http);
        Broadcasts = new BroadcastsResource(_http);
        Sequences = new SequencesResource(_http);
        Knowledge = new KnowledgeResource(_http);
        Ads = new AdsResource(_http);
        Validate = new ValidateResource(_http);
        Media = new MediaResource(_http);
    }

    public FoPostClient(string apiKey)
        : this(new FoPostClientOptions { ApiKey = apiKey })
    {
    }

    public PostsResource Posts { get; }

    public AccountsResource Accounts { get; }

    public AccountGroupsResource AccountGroups { get; }

    public WorkspacesResource Workspaces { get; }

    public LabelsResource Labels { get; }

    /// <summary>What happened in a workspace, including the security audit log.</summary>
    public ActivityResource Activity { get; }

    public AiResource Ai { get; }

    public InboxResource Inbox { get; }

    /// <summary>The people behind the inbox, and the fields kept about them.</summary>
    public ContactsResource Contacts { get; }

    /// <summary>
    /// One message into every conversation the workspace already has with a segment of its
    /// contacts.
    /// </summary>
    public BroadcastsResource Broadcasts { get; }

    /// <summary>A series of messages on a delay, walked per enrolled contact.</summary>
    public SequencesResource Sequences { get; }
    /// <summary>The workspace knowledge base, which grounds drafted replies.</summary>
    public KnowledgeResource Knowledge { get; }

    public AdsResource Ads { get; }

    public ValidateResource Validate { get; }

    public MediaResource Media { get; }

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
