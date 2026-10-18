using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary><c>client.Posts</c> — create, schedule, publish, and inspect posts.</summary>
public sealed class PostsResource
{
    // What a multipart request may carry per file when the limits call is unavailable.
    private const long FallbackMultipartMaxFileBytes = 100L * 1024 * 1024;

    private readonly FoPostHttpClient _http;
    private readonly MediaResource _media;

    internal PostsResource(FoPostHttpClient http, MediaResource media)
    {
        _http = http;
        _media = media;
    }

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
        };

        if (options.Accounts.Count > 0 || options.AccountGroupId is null)
        {
            body["accounts"] = options.Accounts;
        }
        if (options.AccountGroupId is not null)
        {
            body["account_group_id"] = options.AccountGroupId;
        }

        if (options.ScheduleAt is { } scheduleAt)
        {
            body["schedule_at"] = Iso(scheduleAt);
        }
        if (options.Schedule is not null)
        {
            body["schedule"] = options.Schedule;
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

        if (options.Files is { Count: > 0 } files)
        {
            if (await FitsMultipartAsync(files, options.WorkspaceId, cancellationToken).ConfigureAwait(false))
            {
                return await SendMultipartAsync(HttpMethod.Post, "/v1/posts", body, files, cancellationToken)
                    .ConfigureAwait(false);
            }

            var content = options.Content.Count > 0 ? options.Content : new List<PostContent> { new() };
            await AttachDirectAsync(options.WorkspaceId, content, files, cancellationToken).ConfigureAwait(false);
            body["content"] = content;
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
        if (options.Schedule.IsSet)
        {
            body["schedule"] = options.Schedule.Value;
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

        var path = $"/v1/posts/{Uri.EscapeDataString(postId)}";
        if (options.Files is { Count: > 0 } files)
        {
            if (await FitsMultipartAsync(files, null, cancellationToken).ConfigureAwait(false))
            {
                return await SendMultipartAsync(HttpMethod.Put, path, body, files, cancellationToken)
                    .ConfigureAwait(false);
            }

            // Direct upload needs the workspace, and an untouched update needs the existing blocks.
            var existing = await GetAsync(postId, cancellationToken).ConfigureAwait(false);
            var content = options.Content.IsSet && options.Content.Value is { } given
                ? given
                : existing.Content.Select(block => new PostContent(block.Text, block.Media)).ToList();
            if (content.Count == 0)
            {
                content.Add(new PostContent());
            }

            await AttachDirectAsync(existing.WorkspaceId ?? string.Empty, content, files, cancellationToken)
                .ConfigureAwait(false);
            body["content"] = content;
        }

        var response = await _http.PutAsync(path, body, cancellationToken).ConfigureAwait(false);
        return Require<Post>(FoPostHttpClient.Unwrap(response));
    }

    private async Task<bool> FitsMultipartAsync(
        IList<PostFile> files,
        string? workspaceId,
        CancellationToken cancellationToken)
    {
        var ceiling = FallbackMultipartMaxFileBytes;
        try
        {
            var limits = await _media.LimitsAsync(workspaceId, cancellationToken).ConfigureAwait(false);
            if (limits.MultipartMaxFileBytes > 0)
            {
                ceiling = limits.MultipartMaxFileBytes;
            }
        }
        catch (FoPostException)
        {
            // The multipart ceiling is fixed; an unreachable limits call falls back to it.
        }

        return files.All(file => file.Length <= ceiling);
    }

    private async Task<Post> SendMultipartAsync(
        HttpMethod method,
        string path,
        Dictionary<string, object?> body,
        IList<PostFile> files,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(body, FoPostJson.Options);
        var response = await _http.RequestAsync(
            method,
            path,
            () =>
            {
                // Names are quoted explicitly: .NET sends them bare, which some multipart parsers reject.
                var form = new MultipartFormDataContent { { new StringContent(json, Encoding.UTF8), "\"post\"" } };
                foreach (var file in files)
                {
                    if (file.Content.CanSeek)
                    {
                        file.Content.Position = 0;
                    }

                    var part = new LeaveOpenStreamContent(file.Content);
                    part.Headers.ContentType = new MediaTypeHeaderValue(file.MimeType);
                    part.Headers.ContentLength = file.Length;
                    form.Add(part, "\"files\"", $"\"{file.FileName}\"");
                }

                return form;
            },
            null,
            cancellationToken).ConfigureAwait(false);
        return Require<Post>(FoPostHttpClient.Unwrap(response));
    }

    // A 429 retry rebuilds the form, so the caller's stream must outlive one attempt.
    private sealed class LeaveOpenStreamContent : StreamContent
    {
        public LeaveOpenStreamContent(Stream content) : base(content)
        {
        }

        protected override void Dispose(bool disposing) => base.Dispose(false);
    }

    private async Task AttachDirectAsync(
        string workspaceId,
        IList<PostContent> content,
        IList<PostFile> files,
        CancellationToken cancellationToken)
    {
        var first = content[0];
        first.Media ??= new List<MediaItem>();
        foreach (var file in files)
        {
            var uploaded = await _media.UploadDirectAsync(
                new PresignOptions
                {
                    WorkspaceId = workspaceId,
                    Filename = file.FileName,
                    MimeType = file.MimeType,
                    Size = file.Length,
                },
                file.Content,
                cancellationToken).ConfigureAwait(false);
            first.Media.Add(new MediaItem
            {
                Type = file.MediaType,
                Name = uploaded.Name ?? file.FileName,
                Url = uploaded.Url,
                Size = uploaded.Size ?? file.Length,
            });
        }
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
