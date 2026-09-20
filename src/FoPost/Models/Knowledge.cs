using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>Where a knowledge source's text comes from.</summary>
public static class KnowledgeSourceKinds
{
    /// <summary>Question and answer pairs, one pair per paragraph.</summary>
    public const string Faq = "faq";

    /// <summary>A free-form note.</summary>
    public const string Text = "text";

    /// <summary>A page on your own site, re-read whenever you sync it.</summary>
    public const string Url = "url";

    /// <summary>A plain-text or CSV item from the media library.</summary>
    public const string File = "file";
}

/// <summary>Where a knowledge source is in its ingest cycle.</summary>
public static class KnowledgeSourceStatuses
{
    public const string Pending = "pending";
    public const string Syncing = "syncing";

    /// <summary>The only status that is searched.</summary>
    public const string Ready = "ready";
    public const string Failed = "failed";
}

/// <summary>
/// One thing the workspace has told FoPost about itself: an FAQ, a note, a page
/// on its own site, or a plain-text/CSV file from the media library.
/// </summary>
public sealed class KnowledgeSource : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>One of <see cref="KnowledgeSourceKinds"/>.</summary>
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>One of <see cref="KnowledgeSourceStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Why the last sync failed, in plain words.</summary>
    [JsonPropertyName("statusMessage")]
    public string? StatusMessage { get; set; }

    /// <summary>Set for <c>url</c> sources.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>Set for <c>file</c> sources: the media library item read.</summary>
    [JsonPropertyName("mediaId")]
    public string? MediaId { get; set; }

    /// <summary>Null means the source serves the whole workspace.</summary>
    [JsonPropertyName("brandVoiceId")]
    public string? BrandVoiceId { get; set; }

    /// <summary>Searchable passages the last sync produced.</summary>
    [JsonPropertyName("chunkCount")]
    public int ChunkCount { get; set; }

    /// <summary>The typed text, for <c>faq</c> and <c>text</c> sources only.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("lastSyncedAt")]
    public DateTimeOffset? LastSyncedAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>One retrieved passage, with the source it came from so a reply can cite it.</summary>
public sealed class KnowledgeMatch : FoPostModel
{
    [JsonPropertyName("sourceId")]
    public string SourceId { get; set; } = string.Empty;

    [JsonPropertyName("sourceTitle")]
    public string SourceTitle { get; set; } = string.Empty;

    [JsonPropertyName("sourceKind")]
    public string SourceKind { get; set; } = string.Empty;

    [JsonPropertyName("sourceUrl")]
    public string? SourceUrl { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>Similarity to the question, 0-1.</summary>
    [JsonPropertyName("score")]
    public double Score { get; set; }
}

/// <summary>What a sync answers: the source, and that it is queued.</summary>
public sealed class KnowledgeSyncResult : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
