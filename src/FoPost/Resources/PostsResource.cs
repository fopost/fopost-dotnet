using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Posts</c> — create, schedule, publish, and inspect posts.</summary>
public sealed class PostsResource
{
    private readonly FoPostHttpClient _http;

    internal PostsResource(FoPostHttpClient http) => _http = http;

    /// <summary>One page of posts, newest first unless <c>Sort</c> says otherwise.</summary>
    public async Task<Page<Post>> ListAsync(
        ListPostsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new ListPostsOptions();

        var query = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["status"] = options.Status,
            ["search"] = options.Search,
            ["platform"] = options.Platform,
            ["label"] = options.Label,
            ["account_id"] = options.AccountId,
            ["sort"] = options.Sort,
            ["from"] = options.From is { } from ? Iso(from) : null,
            ["to"] = options.To is { } to ? Iso(to) : null,
            ["page"] = options.Page,
            ["per_page"] = options.PerPage,
        };

        var body = await _http.GetAsync("/v1/posts", query, cancellationToken).ConfigureAwait(false);
        var items = ToList<Post>(FoPostHttpClient.Unwrap(body));
        return new Page<Post>(items, ReadMeta(body));
    }

    /// <summary>
    /// Walk every matching post, fetching one page at a time. The filters are
    /// the same as <see cref="ListAsync"/>; <c>Page</c> is the page to start on.
    /// </summary>
    public async IAsyncEnumerable<Post> ListAllAsync(
        ListPostsOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new ListPostsOptions();
        var pageNumber = options.Page < 1 ? 1 : options.Page;

        while (true)
        {
            var page = await ListAsync(
                new ListPostsOptions
                {
                    WorkspaceId = options.WorkspaceId,
                    Status = options.Status,
                    Search = options.Search,
                    Platform = options.Platform,
                    Label = options.Label,
                    AccountId = options.AccountId,
                    Sort = options.Sort,
                    From = options.From,
                    To = options.To,
                    Page = pageNumber,
                    PerPage = options.PerPage,
                },
                cancellationToken).ConfigureAwait(false);

            if (page.Count == 0)
            {
                yield break;
            }

            foreach (var post in page)
            {
                yield return post;
            }

            if (page.Meta.LastPage is { } lastPage)
            {
                if (pageNumber >= lastPage)
                {
                    yield break;
                }
            }
            else if (page.Count < options.PerPage)
            {
                yield break;
            }

            pageNumber++;
        }
    }

    public async Task<Post> GetAsync(string postId, CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync($"/v1/posts/{Uri.EscapeDataString(postId)}", null, cancellationToken)
            .ConfigureAwait(false);
        return Require<Post>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>
    /// Create a draft or a scheduled post. To send one out now, create it and
    /// call <see cref="PublishAsync"/>.
    /// </summary>
    public async Task<Post> CreateAsync(
        CreatePostOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspace_id"] = options.WorkspaceId,
            ["status"] = options.Status,
            ["content"] = options.Content,
            ["accounts"] = options.Accounts,
        };

        if (options.ScheduleAt is { } scheduleAt)
        {
            body["schedule_at"] = Iso(scheduleAt);
        }
        if (options.Labels is not null)
        {
            body["labels"] = options.Labels;
        }
        if (options.Title is not null)
        {
            body["title"] = options.Title;
        }
        if (options.InternalTitle is not null)
        {
            body["internal_title"] = options.InternalTitle;
        }
        if (options.Summary is not null)
        {
            body["summary"] = options.Summary;
        }
        if (options.ContentType is not null)
        {
            body["content_type"] = options.ContentType;
        }
        if (options.Settings is not null)
        {
            body["settings"] = options.Settings;
        }

        var response = await _http.PostAsync("/v1/posts", body, cancellationToken).ConfigureAwait(false);
        return Require<Post>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Create a single-block draft — the shortest path from text to a post.</summary>
    public Task<Post> CreateAsync(
        string workspaceId,
        string text,
        IEnumerable<string> accounts,
        CancellationToken cancellationToken = default) =>
        CreateAsync(
            new CreatePostOptions
            {
                WorkspaceId = workspaceId,
                Content = new List<PostContent> { new(text) },
                Accounts = accounts.ToList(),
            },
            cancellationToken);

    /// <summary>Partial update — only the fields you set are sent.</summary>
    public async Task<Post> UpdateAsync(
        string postId,
        UpdatePostOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Content.IsSet)
        {
            body["content"] = options.Content.Value;
        }
        if (options.Accounts.IsSet)
        {
            body["accounts"] = options.Accounts.Value;
        }
        if (options.Status.IsSet)
        {
            body["status"] = options.Status.Value;
        }
        if (options.ScheduleAt.IsSet)
        {
            body["schedule_at"] = options.ScheduleAt.Value is { } at ? Iso(at) : null;
        }
        if (options.Labels.IsSet)
        {
            body["labels"] = options.Labels.Value;
        }
        if (options.Title.IsSet)
        {
            body["title"] = options.Title.Value;
        }
        if (options.InternalTitle.IsSet)
        {
            body["internal_title"] = options.InternalTitle.Value;
        }
        if (options.Summary.IsSet)
        {
            body["summary"] = options.Summary.Value;
        }
        if (options.ContentType.IsSet)
        {
            body["content_type"] = options.ContentType.Value;
        }
        if (options.Settings.IsSet)
        {
            body["settings"] = options.Settings.Value;
        }

        var response = await _http
            .PutAsync($"/v1/posts/{Uri.EscapeDataString(postId)}", body, cancellationToken)
            .ConfigureAwait(false);
        return Require<Post>(FoPostHttpClient.Unwrap(response));
    }

    public async Task DeleteAsync(string postId, CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync($"/v1/posts/{Uri.EscapeDataString(postId)}", null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Queue the post for immediate delivery to its accounts.</summary>
    public Task<JsonNode?> PublishAsync(string postId, CancellationToken cancellationToken = default) =>
        Act(postId, "publish", cancellationToken);

    /// <summary>Cancel the deliveries that have not gone out yet.</summary>
    public Task<JsonNode?> CancelAsync(string postId, CancellationToken cancellationToken = default) =>
        Act(postId, "cancel", cancellationToken);

    /// <summary>Retry the deliveries that failed, leaving the successful ones alone.</summary>
    public Task<JsonNode?> RetryAsync(string postId, CancellationToken cancellationToken = default) =>
        Act(postId, "retry", cancellationToken);

    /// <summary>Per-account blockers and advisory content signals, without publishing.</summary>
    public Task<JsonNode?> PreflightAsync(string postId, CancellationToken cancellationToken = default) =>
        Act(postId, "preflight", cancellationToken);

    /// <summary>Duplicate the post as a fresh draft.</summary>
    public async Task<Post> DuplicateAsync(string postId, CancellationToken cancellationToken = default)
    {
        var body = await Act(postId, "duplicate", cancellationToken).ConfigureAwait(false);
        return Require<Post>(body);
    }

    public async Task<IReadOnlyList<Delivery>> DeliveriesAsync(
        string postId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync($"/v1/posts/{Uri.EscapeDataString(postId)}/deliveries", null, cancellationToken)
            .ConfigureAwait(false);
        return ToList<Delivery>(FoPostHttpClient.Unwrap(body));
    }

    private async Task<JsonNode?> Act(string postId, string action, CancellationToken cancellationToken)
    {
        var body = await _http
            .PostAsync($"/v1/posts/{Uri.EscapeDataString(postId)}/{action}", null, cancellationToken)
            .ConfigureAwait(false);
        return FoPostHttpClient.Unwrap(body);
    }
}
