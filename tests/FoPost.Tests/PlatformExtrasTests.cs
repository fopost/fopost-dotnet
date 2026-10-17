using Xunit;

namespace FoPost.Tests;

public class PlatformExtrasTests
{
    [Fact]
    public async Task Create_pinterest_board_sends_only_what_was_given()
    {
        var handler = new StubHandler().Json("""
        {"data":{"id":"b1","name":"Recipes","privacy":"PUBLIC","description":null,"image":null}}
        """);
        using var test = new TestClient(handler);

        var board = await test.Client.Accounts.CreatePinterestBoardAsync(
            "a1", new CreatePinterestBoardOptions { Name = "Recipes" });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/pinterest/boards",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"name":"Recipes"}""", handler.LastBody);
        Assert.Equal("b1", board.Id);
    }

    [Fact]
    public async Task Set_default_youtube_playlist_sends_null_to_clear_it()
    {
        var handler = new StubHandler().Json("""{"data":{"playlist_id":null}}""");
        using var test = new TestClient(handler);

        var stored = await test.Client.Accounts.SetDefaultYouTubePlaylistAsync("a1", null);

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("""{"playlist_id":null}""", handler.LastBody);
        Assert.Null(stored);
    }

    [Fact]
    public async Task Playlists_mark_the_stored_default()
    {
        var handler = new StubHandler().Json("""
        {"data":[{"id":"PL1","title":"Tutorials","is_default":true}]}
        """);
        using var test = new TestClient(handler);

        var playlist = Assert.Single(await test.Client.Accounts.ListYouTubePlaylistsAsync("a1"));

        Assert.True(playlist.IsDefault);
    }

    [Fact]
    public async Task Bluesky_languages_round_trip()
    {
        var handler = new StubHandler().Json("""{"data":{"languages":["en","pt-BR"]}}""");
        using var test = new TestClient(handler);

        var result = await test.Client.Accounts.SetBlueskyLanguagesAsync("a1", ["en", "pt-BR"]);

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("""{"languages":["en","pt-BR"]}""", handler.LastBody);
        Assert.Equal(["en", "pt-BR"], result.Languages);
    }

    [Fact]
    public async Task TikTok_creator_info_reports_the_accounts_own_switches()
    {
        var handler = new StubHandler().Json("""
        {"data":{"privacy_level_options":["PUBLIC_TO_EVERYONE"],"comment_disabled":false,
                 "duet_disabled":true,"stitch_disabled":false,"max_video_post_duration_sec":600}}
        """);
        using var test = new TestClient(handler);

        var info = await test.Client.Accounts.GetTikTokCreatorInfoAsync("a1");

        Assert.True(info.DuetDisabled);
        Assert.False(info.StitchDisabled);
        Assert.Equal(600, info.MaxVideoPostDurationSec);
    }

    [Fact]
    public async Task TikTok_music_search_passes_the_query_through()
    {
        var handler = new StubHandler().Json("""{"data":[{"id":"m1","title":"Sunrise","author":"Kite"}]}""");
        using var test = new TestClient(handler);

        var tracks = await test.Client.Accounts.SearchTikTokMusicAsync(
            "a1", "sunrise", new TikTokSearchOptions { Limit = 5 });

        Assert.Equal("m1", tracks[0].Id);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("q=sunrise", url, StringComparison.Ordinal);
        Assert.Contains("limit=5", url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TikTok_video_lookup_returns_the_address_a_repurpose_run_reads()
    {
        var handler = new StubHandler().Json("""
        {"data":{"video_id":"7300000000000000000",
                 "download_url":"https://www.tiktok.com/@a/video/7300000000000000000"}}
        """);
        using var test = new TestClient(handler);

        var video = await test.Client.Accounts.LookupTikTokVideoAsync(
            "a1", "https://www.tiktok.com/@a/video/7300000000000000000");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("7300000000000000000", video.VideoId);
        Assert.NotNull(video.DownloadUrl);
    }

    [Fact]
    public async Task Instagram_stories_ask_for_insights_only_when_requested()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"s1","media_type":"IMAGE"}]}""")
            .Json("""{"data":[{"id":"s1","media_type":"IMAGE","insights":{"views":40}}]}""");
        using var test = new TestClient(handler);

        await test.Client.Accounts.ListInstagramStoriesAsync("a1");
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/instagram/stories",
            handler.LastRequest.RequestUri!.ToString());

        var story = Assert.Single(await test.Client.Accounts.ListInstagramStoriesAsync("a1", insights: true));
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/instagram/stories?insights=true",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(40, story.Insights!["views"]);
    }

    [Fact]
    public async Task LinkedIn_mentions_carry_the_annotation_to_paste()
    {
        var handler = new StubHandler().Json("""
        {"data":[{"urn":"urn:li:organization:2414183","name":"Devtestco",
                  "annotation":"@[Devtestco](urn:li:organization:2414183)"}]}
        """);
        using var test = new TestClient(handler);

        var mention = Assert.Single(await test.Client.Accounts.SearchLinkedInMentionsAsync("a1", "devtestco"));

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/linkedin/mentions?q=devtestco",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("@[Devtestco](urn:li:organization:2414183)", mention.Annotation);
    }
}
