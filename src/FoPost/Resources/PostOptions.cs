using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>One block of a post being written. Several blocks make a thread.</summary>
public sealed class PostContent
{
    public PostContent()
    {
    }

    public PostContent(string? text, IList<MediaItem>? media = null)
    {
        Text = text;
        Media = media;
    }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("media")]
    public IList<MediaItem>? Media { get; set; }
}

/// <summary>Filters for <see cref="Resources.PostsResource.ListAsync"/>.</summary>
public sealed class ListPostsOptions
{
    public string? WorkspaceId { get; set; }

    /// <summary>One of <see cref="PostStatuses"/>.</summary>
    public string? Status { get; set; }

    public string? Search { get; set; }

    /// <summary>One of <see cref="Platforms"/>.</summary>
    public string? Platform { get; set; }

    public string? Label { get; set; }

    public string? AccountId { get; set; }

    public string? Sort { get; set; }

    public DateTimeOffset? From { get; set; }

    public DateTimeOffset? To { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 30;
}

/// <summary>The body of <see cref="Resources.PostsResource.CreateAsync(CreatePostOptions, CancellationToken)"/>.</summary>
public sealed class CreatePostOptions
{
    public string WorkspaceId { get; set; } = string.Empty;

    public IList<PostContent> Content { get; set; } = new List<PostContent>();

    /// <summary>Ids of the connected accounts to publish to.</summary>
    public IList<string> Accounts { get; set; } = new List<string>();

    /// <summary><c>draft</c> or <c>scheduled</c>; a scheduled post needs <see cref="ScheduleAt"/>.</summary>
    public string Status { get; set; } = PostStatuses.Draft;

    public DateTimeOffset? ScheduleAt { get; set; }

    public IList<string>? Labels { get; set; }

    public string? Title { get; set; }

    public string? InternalTitle { get; set; }

    public string? Summary { get; set; }

    /// <summary><c>post</c>, <c>thread</c>, or <c>reel</c>.</summary>
    public string? ContentType { get; set; }

    /// <summary>Per-platform overrides, keyed by platform name.</summary>
    public IDictionary<string, object?>? Settings { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.PostsResource.UpdateAsync"/>. Every field is
/// an <see cref="Optional{T}"/>: leave one alone and it is not sent at all, so
/// an update stays partial.
/// </summary>
public sealed class UpdatePostOptions
{
    public Optional<IList<PostContent>> Content { get; set; }

    public Optional<IList<string>> Accounts { get; set; }

    public Optional<string> Status { get; set; }

    public Optional<DateTimeOffset?> ScheduleAt { get; set; }

    public Optional<IList<string>> Labels { get; set; }

    public Optional<string?> Title { get; set; }

    public Optional<string?> InternalTitle { get; set; }

    public Optional<string?> Summary { get; set; }

    public Optional<string> ContentType { get; set; }

    public Optional<IDictionary<string, object?>> Settings { get; set; }
}
