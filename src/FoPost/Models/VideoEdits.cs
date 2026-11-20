using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>The operations a video edit can run, in the order they are listed.</summary>
public static class VideoEditOps
{
    public const string Trim = "trim";
    public const string Cut = "cut";
    public const string Speed = "speed";
    public const string Merge = "merge";
    public const string Volume = "volume";
    public const string RemoveSilence = "remove_silence";
    public const string Denoise = "denoise";
    public const string Subtitles = "subtitles";
    public const string TranslateSubtitles = "translate_subtitles";
    public const string Clips = "clips";
    public const string Music = "music";
    public const string Voiceover = "voiceover";
    public const string Watermark = "watermark";
    public const string Cover = "cover";
    public const string Reframe = "reframe";
    public const string Audiogram = "audiogram";
}

/// <summary>Where a video edit is.</summary>
public static class VideoEditStatuses
{
    public const string Queued = "queued";
    public const string Running = "running";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
}

/// <summary>One operation and whether this deployment can run it.</summary>
public sealed class VideoEditOpInfo : FoPostModel
{
    /// <summary>One of <see cref="VideoEditOps"/>.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public bool Available { get; set; }

    /// <summary>True when the operation spends AI balance; an API key then also needs the <c>ai</c> scope.</summary>
    [JsonPropertyName("uses_ai")]
    public bool UsesAi { get; set; }
}

/// <summary>One step of a video edit: an operation and its parameters.</summary>
public sealed class VideoEditOperation : FoPostModel
{
    public VideoEditOperation()
    {
    }

    public VideoEditOperation(string op, IDictionary<string, object?>? parameters = null)
    {
        Op = op;
        Params = parameters;
    }

    /// <summary>One of <see cref="VideoEditOps"/>.</summary>
    [JsonPropertyName("op")]
    public string Op { get; set; } = string.Empty;

    /// <summary>The operation's parameters, sent as given.</summary>
    [JsonPropertyName("params")]
    public IDictionary<string, object?>? Params { get; set; }
}

/// <summary>A video edit job: the source video, the operations, and the result once it is done.</summary>
public sealed class VideoEdit : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>One of <see cref="VideoEditStatuses"/>.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("progress")]
    public double? Progress { get; set; }

    [JsonPropertyName("source_media_id")]
    public string? SourceMediaId { get; set; }

    [JsonPropertyName("operations")]
    public IList<VideoEditOperation> Operations { get; set; } = new List<VideoEditOperation>();

    [JsonPropertyName("result_media_id")]
    public string? ResultMediaId { get; set; }

    [JsonPropertyName("result_media")]
    public VideoEditResultMedia? ResultMedia { get; set; }

    /// <summary>Further files the edit produced, such as clips or a subtitle file.</summary>
    [JsonPropertyName("extra_outputs")]
    public IList<VideoEditOutput> ExtraOutputs { get; set; } = new List<VideoEditOutput>();

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; set; }
}

public sealed class VideoEditResultMedia : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
}

public sealed class VideoEditOutput : FoPostModel
{
    [JsonPropertyName("media_id")]
    public string MediaId { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("mime")]
    public string? Mime { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }
}
