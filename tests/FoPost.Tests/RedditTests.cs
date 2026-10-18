using System.Net;
using Xunit;

namespace FoPost.Tests;

public class RedditTests
{
    [Fact]
    public async Task Subreddits_rules_and_flairs_read_the_camel_case_wire()
    {
        var handler = new StubHandler()
            .Json("""
            {"data":[{"name":"webdev","title":"Web Development","subscribers":2000000,"over18":false,
                      "canPost":true,"flairEnabled":true,"iconUrl":null,"isDefault":true}]}
            """)
            .Json("""
            {"data":{"subreddit":"webdev","rules":[{"name":"No self promotion",
                      "description":"Keep it useful","appliesTo":"link"}]}}
            """)
            .Json("""
            {"data":{"subreddit":"webdev","flairs":[{"id":"flair-1","text":"Showoff Saturday",
                      "editable":false}]}}
            """);
        using var test = new TestClient(handler);

        var subreddits = await test.Client.Accounts.ListRedditSubredditsAsync("a1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/reddit/subreddits",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("webdev", subreddits[0].Name);
        Assert.True(subreddits[0].IsDefault);
        Assert.True(subreddits[0].FlairEnabled);

        var rules = await test.Client.Accounts.ListRedditSubredditRulesAsync("a1", "webdev");
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/reddit/subreddits/webdev/rules",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("link", Assert.Single(rules.Rules).AppliesTo);

        var flairs = await test.Client.Accounts.ListRedditFlairsAsync("a1", "webdev");
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/reddit/flairs?subreddit=webdev",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("flair-1", Assert.Single(flairs.Flairs).Id);
    }

    [Fact]
    public async Task The_default_subreddit_sends_null_to_fall_back_to_the_profile_page()
    {
        var handler = new StubHandler().Json("""{"data":{"subreddit":null}}""");
        using var test = new TestClient(handler);

        var result = await test.Client.Accounts.SetRedditDefaultSubredditAsync("a1", null);

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("""{"subreddit":null}""", handler.LastBody);
        Assert.Null(result.Subreddit);
    }

    [Fact]
    public async Task Validating_a_subreddit_names_the_account_it_reads_as()
    {
        var handler = new StubHandler().Json("""
        {"data":{"subreddit":"webdev","exists":true,"can_post":true,"over_18":false,
                 "flair_enabled":true,"ok":true}}
        """);
        using var test = new TestClient(handler);

        var check = await test.Client.Validate.SubredditAsync("a1", "webdev");

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/validate/subreddit?account_id=a1&name=webdev",
            handler.LastRequest.RequestUri!.ToString());
        Assert.True(check.Ok);
        Assert.True(check.CanPost);
        Assert.False(check.Over18);
    }

    [Fact]
    public async Task Voting_sends_the_direction()
    {
        var handler = new StubHandler().Json("""
        {"data":{"id":"i1","platform":"reddit","type":"comment","state":"unread",
                 "vote":"down","canVote":true}}
        """);
        using var test = new TestClient(handler);

        var item = await test.Client.Inbox.VoteAsync("i1", "down");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/inbox/i1/vote", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"direction":"down"}""", handler.LastBody);
        Assert.Equal("down", item.Vote);
        Assert.True(item.CanVote);
    }

    [Fact]
    public async Task A_stale_grant_is_a_conflict()
    {
        var handler = new StubHandler().Json(
            """{"error":"reconnect_required","message":"Reconnect this Reddit account"}""",
            HttpStatusCode.Conflict);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(
            () => test.Client.Accounts.ListRedditSubredditsAsync("a1"));

        Assert.Equal(409, error.Status);
        Assert.Equal("reconnect_required", error.Code);
    }
}
