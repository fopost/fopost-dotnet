using System.Net;
using Xunit;

namespace FoPost.Tests;

public class XTests
{
    [Fact]
    public async Task Follow_and_unfollow_send_the_user_id()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"account_id":"a1","user_id":"2244994945","following":true}}""")
            .Json("""{"data":{"account_id":"a1","user_id":"2244994945","following":false}}""");
        using var test = new TestClient(handler);

        var followed = await test.Client.Accounts.FollowXUserAsync("a1", "2244994945");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/x/follow", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"user_id":"2244994945"}""", handler.LastBody);
        Assert.Equal("a1", followed.AccountId);
        Assert.Equal("2244994945", followed.UserId);
        Assert.True(followed.Following);

        var unfollowed = await test.Client.Accounts.UnfollowXUserAsync("a1", "2244994945");
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/x/unfollow", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"user_id":"2244994945"}""", handler.LastBody);
        Assert.False(unfollowed.Following);
    }

    [Fact]
    public async Task Post_lookup_names_the_account_it_reads_as()
    {
        var handler = new StubHandler().Json("""
        {"data":{"id":"1790000000000000000","text":"Shipping today","permalink":"https://x.com/yourbrand/status/1790000000000000000",
                 "author_name":"Your Brand","author_handle":"yourbrand","author_avatar_url":null,"author_followers":1200,
                 "conversation_id":"1790000000000000000","parent_external_id":null,
                 "likes":4,"replies":1,"reposts":2,"quotes":0,"platform_created_at":"2026-10-18T09:00:00.000Z"}}
        """);
        using var test = new TestClient(handler);

        var post = await test.Client.Accounts.GetXPostAsync("1790000000000000000", "a1");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/x/posts/1790000000000000000?account_id=a1",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("1790000000000000000", post.Id);
        Assert.Equal("yourbrand", post.AuthorHandle);
        Assert.Equal(1200, post.AuthorFollowers);
        Assert.Equal(2, post.Reposts);
        Assert.Null(post.ParentExternalId);
        Assert.Equal(new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.Zero), post.PlatformCreatedAt);
    }

    [Fact]
    public async Task Search_sends_the_query_and_reads_the_next_token()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"1","text":"first"},{"id":"2","text":"second"}],"meta":{"next_token":"b26v89c19zqg8o3f"}}""")
            .Json("""{"data":[{"id":"3","text":"third"}],"meta":{"next_token":null}}""");
        using var test = new TestClient(handler);

        var page = await test.Client.Accounts.SearchXPostsAsync(new XSearchOptions
        {
            AccountId = "a1",
            Query = "from:yourbrand",
            MaxResults = 50,
        });

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/x/search?account_id=a1&q=from%3Ayourbrand&max_results=50",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(2, page.Count);
        Assert.Equal("second", page[1].Text);
        Assert.Equal("b26v89c19zqg8o3f", page.NextToken);

        var last = await test.Client.Accounts.SearchXPostsAsync(new XSearchOptions
        {
            AccountId = "a1",
            Query = "from:yourbrand",
            NextToken = page.NextToken,
        });
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/x/search?account_id=a1&q=from%3Ayourbrand&next_token=b26v89c19zqg8o3f",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("3", Assert.Single(last).Id);
        Assert.Null(last.NextToken);
    }

    [Fact]
    public async Task A_stale_grant_is_a_conflict()
    {
        var handler = new StubHandler().Json(
            """{"error":"reconnect_required","message":"Reconnect this X account"}""",
            HttpStatusCode.Conflict);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(
            () => test.Client.Accounts.FollowXUserAsync("a1", "2244994945"));

        Assert.Equal(409, error.Status);
        Assert.Equal("reconnect_required", error.Code);
    }
}
