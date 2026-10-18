using System.Collections;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>One API request made against the workspace.</summary>
public sealed class RequestLog : FoPostModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("workspace_id")]
    public string? WorkspaceId { get; set; }

    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }

    /// <summary><c>jwt</c> or <c>api_key</c>.</summary>
    [JsonPropertyName("auth_method")]
    public string? AuthMethod { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("route")]
    public string? Route { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("status_code")]
    public int? StatusCode { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>Request counts for one time bucket.</summary>
public sealed class RequestLogBucket : FoPostModel
{
    [JsonPropertyName("at")]
    public DateTimeOffset? At { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("errors")]
    public int? Errors { get; set; }
}

/// <summary>Totals over the window the logs were read from.</summary>
public sealed class RequestLogSummary : FoPostModel
{
    [JsonPropertyName("since")]
    public DateTimeOffset? Since { get; set; }

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("errors")]
    public int? Errors { get; set; }

    [JsonPropertyName("buckets")]
    public IList<RequestLogBucket> Buckets { get; set; } = new List<RequestLogBucket>();
}

/// <summary>
/// One page of request logs, newest first. Pass <see cref="NextCursor"/> back as
/// <see cref="ListLogsOptions.Before"/> while <see cref="HasMore"/> is true.
/// </summary>
public sealed class RequestLogPage : IReadOnlyList<RequestLog>
{
    public RequestLogPage(IReadOnlyList<RequestLog> items, string? nextCursor, bool hasMore, RequestLogSummary summary)
    {
        Items = items;
        NextCursor = nextCursor;
        HasMore = hasMore;
        Summary = summary;
    }

    public IReadOnlyList<RequestLog> Items { get; }

    public string? NextCursor { get; }

    public bool HasMore { get; }

    public RequestLogSummary Summary { get; }

    public int Count => Items.Count;

    public RequestLog this[int index] => Items[index];

    public IEnumerator<RequestLog> GetEnumerator() => Items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
