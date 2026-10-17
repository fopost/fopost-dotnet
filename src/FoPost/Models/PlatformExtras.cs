using System.Text.Json.Serialization;

namespace FoPost;

// Per-network extras under /accounts/{id}/<platform>/…, all on the accounts scope.

/// <summary>A Pinterest board a Pin can land on; pass <see cref="Id"/> as the <c>board_id</c> platform setting.</summary>
public sealed class PinterestBoard : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>The board cover image.</summary>
    [JsonPropertyName("image")]
    public string? Image { get; set; }
}

/// <summary>A playlist on the channel; <see cref="IsDefault"/> marks the one a new video joins when none is picked.</summary>
public sealed class YouTubePlaylist : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("privacy")]
    public string? Privacy { get; set; }

    [JsonPropertyName("item_count")]
    public int? ItemCount { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("is_default")]
    public bool IsDefault { get; set; }
}

/// <summary>A caption track on one of the channel's videos.</summary>
public sealed class YouTubeCaptionTrack : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>A BCP-47 tag.</summary>
    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("track_kind")]
    public string? TrackKind { get; set; }

    [JsonPropertyName("is_draft")]
    public bool IsDraft { get; set; }

    [JsonPropertyName("is_auto_synced")]
    public bool IsAutoSynced { get; set; }

    [JsonPropertyName("last_updated")]
    public string? LastUpdated { get; set; }
}

/// <summary>One caption track read back as text, in SRT.</summary>
public sealed class YouTubeTranscript : FoPostModel
{
    [JsonPropertyName("caption_id")]
    public string CaptionId { get; set; } = string.Empty;

    [JsonPropertyName("transcript")]
    public string Transcript { get; set; } = string.Empty;
}

/// <summary>The default post languages for a Bluesky connection: up to three BCP-47 tags.</summary>
public sealed class BlueskyLanguages : FoPostModel
{
    [JsonPropertyName("languages")]
    public IList<string> Languages { get; set; } = new List<string>();
}

/// <summary>
/// A track from TikTok's Commercial Music Library. Pass <see cref="Id"/> as the
/// <c>music_id</c> platform setting to attach it to a post.
/// </summary>
public sealed class TikTokMusic : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("duration_sec")]
    public int? DurationSec { get; set; }

    [JsonPropertyName("cover_url")]
    public string? CoverUrl { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }
}

/// <summary>
/// A place a post can be tagged with. Pass <see cref="Id"/> as the <c>location_id</c>
/// platform setting.
/// </summary>
public sealed class TikTokPlace : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }
}

/// <summary>
/// One of the account's own videos, resolved from a share link. TikTok serves no raw
/// media file, so <see cref="DownloadUrl"/> is the share address, which is what a
/// repurpose run reads.
/// </summary>
public sealed class TikTokVideoSource : FoPostModel
{
    [JsonPropertyName("video_id")]
    public string VideoId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("duration_sec")]
    public int? DurationSec { get; set; }

    [JsonPropertyName("cover_image_url")]
    public string? CoverImageUrl { get; set; }

    [JsonPropertyName("share_url")]
    public string? ShareUrl { get; set; }

    [JsonPropertyName("embed_link")]
    public string? EmbedLink { get; set; }

    [JsonPropertyName("download_url")]
    public string? DownloadUrl { get; set; }
}

/// <summary>
/// The switches TikTok enforces at publish time. They are set on the TikTok account itself, not in
/// FoPost, so a disabled one cannot be turned back on here.
/// </summary>
public sealed class TikTokCreatorInfo : FoPostModel
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }

    /// <summary>The levels this creator may publish at right now.</summary>
    [JsonPropertyName("privacy_level_options")]
    public IList<string> PrivacyLevelOptions { get; set; } = new List<string>();

    [JsonPropertyName("comment_disabled")]
    public bool CommentDisabled { get; set; }

    [JsonPropertyName("duet_disabled")]
    public bool DuetDisabled { get; set; }

    [JsonPropertyName("stitch_disabled")]
    public bool StitchDisabled { get; set; }

    [JsonPropertyName("max_video_post_duration_sec")]
    public int? MaxVideoPostDurationSec { get; set; }
}

/// <summary>A track a Reel can carry; pass <see cref="Id"/> as the <c>audio_id</c> platform setting.</summary>
public sealed class InstagramAudio : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("duration_ms")]
    public int? DurationMs { get; set; }

    [JsonPropertyName("audio_type")]
    public string? AudioType { get; set; }

    [JsonPropertyName("cover_artwork_url")]
    public string? CoverArtworkUrl { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("is_ads_eligible")]
    public bool? IsAdsEligible { get; set; }
}

/// <summary>What this account has published in the rolling window, and what is left.</summary>
public sealed class InstagramPublishingLimit : FoPostModel
{
    [JsonPropertyName("quota_usage")]
    public int QuotaUsage { get; set; }

    [JsonPropertyName("quota_total")]
    public int? QuotaTotal { get; set; }

    [JsonPropertyName("quota_duration_sec")]
    public int? QuotaDurationSec { get; set; }

    [JsonPropertyName("remaining")]
    public int? Remaining { get; set; }
}

/// <summary>A story still inside its 24 hours; <see cref="Insights"/> is present only when asked for.</summary>
public sealed class InstagramStory : FoPostModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; }

    [JsonPropertyName("media_product_type")]
    public string? MediaProductType { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("media_url")]
    public string? MediaUrl { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("caption")]
    public string? Caption { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("insights")]
    public IDictionary<string, int>? Insights { get; set; }
}

/// <summary>The insight set for one story.</summary>
public sealed class InstagramStoryInsights : FoPostModel
{
    [JsonPropertyName("story_id")]
    public string StoryId { get; set; } = string.Empty;

    [JsonPropertyName("insights")]
    public IDictionary<string, int> Insights { get; set; } = new Dictionary<string, int>();
}

/// <summary>An entity a post can mention; <see cref="Annotation"/> is what the post text carries.</summary>
public sealed class LinkedInMention : FoPostModel
{
    [JsonPropertyName("urn")]
    public string Urn { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("vanity_name")]
    public string? VanityName { get; set; }

    [JsonPropertyName("logo_url")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("annotation")]
    public string Annotation { get; set; } = string.Empty;
}
