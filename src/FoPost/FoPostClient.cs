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

        Accounts = new AccountsResource(_http);
        ConnectSessions = new ConnectSessionsResource(_http);
        AccountGroups = new AccountGroupsResource(_http);
        Workspaces = new WorkspacesResource(_http);
        Labels = new LabelsResource(_http);
        Activity = new ActivityResource(_http);
        Logs = new LogsResource(_http);
        Webhooks = new WebhooksResource(_http);
        Ai = new AiResource(_http);
        Inbox = new InboxResource(_http);
        Contacts = new ContactsResource(_http);
        Broadcasts = new BroadcastsResource(_http);
        Sequences = new SequencesResource(_http);
        Knowledge = new KnowledgeResource(_http);
        Ads = new AdsResource(_http);
        Validate = new ValidateResource(_http);
        Media = new MediaResource(_http);
        Posts = new PostsResource(_http, Media);
        Queue = new QueueResource(_http);
        Analytics = new AnalyticsResource(_http);
        GoogleBusiness = new GoogleBusinessResource(_http);
        Blog = new BlogResource(_http);
        Whatsapp = new WhatsappResource(_http);
        Blogs = new BlogsResource(_http);
        Automations = new AutomationsResource(_http);
    }

    public FoPostClient(string apiKey)
        : this(new FoPostClientOptions { ApiKey = apiKey })
    {
    }

    public PostsResource Posts { get; }

    /// <summary>The posting queue: weekly slots and the times they produce.</summary>
    public QueueResource Queue { get; }

    public AccountsResource Accounts { get; }

    /// <summary>Hosted connect links for your own users.</summary>
    public ConnectSessionsResource ConnectSessions { get; }

    public AccountGroupsResource AccountGroups { get; }

    public WorkspacesResource Workspaces { get; }

    public LabelsResource Labels { get; }

    /// <summary>What happened in a workspace, including the security audit log.</summary>
    public ActivityResource Activity { get; }

    /// <summary>The API requests made against a workspace.</summary>
    public LogsResource Logs { get; }

    /// <summary>Webhook endpoints and their deliveries.</summary>
    public WebhooksResource Webhooks { get; }

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

    /// <summary>Deeper posting analytics: decay, cadence, per-post timelines and the changes feed.</summary>
    public AnalyticsResource Analytics { get; }
    /// <summary>Manage a connected Google Business Profile location.</summary>
    public GoogleBusinessResource GoogleBusiness { get; }

    /// <summary>Articles for a website: settings, the keyword plan and the articles themselves.</summary>
    public BlogResource Blog { get; }
    /// <summary>WhatsApp Business: templates, flows, groups, blocking and commerce.</summary>
    public WhatsappResource Whatsapp { get; }
    /// <summary>Articles and products on a connected site, by the platform's own ids.</summary>
    public BlogsResource Blogs { get; }

    /// <summary>Triggers, the graphs they run, their versions and their runs.</summary>
    public AutomationsResource Automations { get; }

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
