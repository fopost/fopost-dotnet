namespace FoPost;

/// <summary>
/// Every platform the API can publish to.
/// </summary>
/// <remarks>
/// Model properties stay plain <see cref="string"/> on purpose: a platform
/// added server-side still deserializes on an older SDK. These constants are
/// here so callers do not have to spell the wire values by hand.
/// </remarks>
public static class Platforms
{
    public const string Twitter = "twitter";
    public const string Instagram = "instagram";
    public const string InstagramBusiness = "instagram-business";
    public const string Facebook = "facebook";
    public const string LinkedIn = "linkedin";
    public const string TikTok = "tiktok";
    public const string YouTube = "youtube";
    public const string Bluesky = "bluesky";
    public const string Threads = "threads";
    public const string Mastodon = "mastodon";
    public const string Lemmy = "lemmy";
    public const string Pinterest = "pinterest";
    public const string Telegram = "telegram";
    public const string Twitch = "twitch";
    public const string Discord = "discord";
    public const string Slack = "slack";
    public const string Reddit = "reddit";
    public const string Tumblr = "tumblr";
    public const string Dribbble = "dribbble";
    public const string MeWe = "mewe";
    public const string DevTo = "devto";
    public const string Hashnode = "hashnode";
    public const string Medium = "medium";
    public const string Substack = "substack";
    public const string GoogleBusiness = "google-business";
    public const string Kick = "kick";
    public const string Listmonk = "listmonk";
    public const string WordPress = "wordpress";
    public const string Nostr = "nostr";
    public const string Whop = "whop";
    public const string Skool = "skool";

    /// <summary>Every platform name, in the order the API declares them.</summary>
    public static IReadOnlyList<string> All { get; } = new[]
    {
        Twitter, Instagram, InstagramBusiness, Facebook, LinkedIn, TikTok, YouTube,
        Bluesky, Threads, Mastodon, Lemmy, Pinterest, Telegram, Twitch, Discord,
        Slack, Reddit, Tumblr, Dribbble, MeWe, DevTo, Hashnode, Medium, Substack,
        GoogleBusiness, Kick, Listmonk, WordPress, Nostr, Whop, Skool,
    };
}

/// <summary>The lifecycle states a post moves through.</summary>
public static class PostStatuses
{
    public const string Draft = "draft";
    public const string PendingApproval = "pending_approval";
    public const string Scheduled = "scheduled";
    public const string Publishing = "publishing";
    public const string Published = "published";
    public const string Failed = "failed";
    public const string Cancelled = "cancelled";

    public static IReadOnlyList<string> All { get; } = new[]
    {
        Draft, PendingApproval, Scheduled, Publishing, Published, Failed, Cancelled,
    };
}
