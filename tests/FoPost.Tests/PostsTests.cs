using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class PostsTests
{
    [Fact]
    public async Task List_returns_the_items_and_the_pagination_meta()
    {
        var handler = new StubHandler().Json($$"""
        {
          "data": [{{Fixtures.Post}}],
          "meta": { "current_page": 1, "per_page": 30, "total": 1, "last_page": 1, "from": 1, "to": 1 }
        }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Posts.ListAsync(new ListPostsOptions
        {
            WorkspaceId = "ws_1",
            Status = PostStatuses.Draft,
        });

        Assert.Single(page);
        Assert.Equal("post_1", page[0].Id);
        Assert.Equal(1, page.Meta.Total);
        Assert.Equal(1, page.Meta.From);

        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("workspace_id=ws_1", query, StringComparison.Ordinal);
        Assert.Contains("status=draft", query, StringComparison.Ordinal);
        Assert.Contains("page=1", query, StringComparison.Ordinal);
        Assert.Contains("per_page=30", query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListAll_walks_pages_until_the_last_one()
    {
        var handler = new StubHandler()
            .Json(Paged(1))
            .Json(Paged(2));
        using var test = new TestClient(handler);

        var posts = new List<Post>();
        await foreach (var post in test.Client.Posts.ListAllAsync(new ListPostsOptions { PerPage = 1 }))
        {
            posts.Add(post);
        }

        Assert.Equal(2, posts.Count);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Contains("page=2", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ListAll_stops_on_a_short_page_when_the_api_sends_no_meta()
    {
        var handler = new StubHandler().Json($$"""{"data":[{{Fixtures.Post}}]}""");
        using var test = new TestClient(handler);

        var posts = new List<Post>();
        await foreach (var post in test.Client.Posts.ListAllAsync(new ListPostsOptions { PerPage = 30 }))
        {
            posts.Add(post);
        }

        Assert.Single(posts);
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Get_unwraps_a_bare_resource_as_readily_as_an_enveloped_one()
    {
        var handler = new StubHandler().Json(Fixtures.Post);
        using var test = new TestClient(handler);

        var post = await test.Client.Posts.GetAsync("post_1");

        Assert.Equal("post_1", post.Id);
        Assert.Equal($"{TestClient.BaseUrl}/v1/posts/post_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Create_sends_snake_case_and_a_utc_schedule()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Fixtures.Post}}}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Status = PostStatuses.Scheduled,
            ScheduleAt = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.FromHours(2)),
            Content = new List<PostContent> { new("Hello from .NET") },
            Accounts = new List<string> { "acc_1" },
            Labels = new List<string> { "label_1" },
            Title = "A title",
        });

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("ws_1", body.GetProperty("workspace_id").GetString());
        Assert.Equal("scheduled", body.GetProperty("status").GetString());
        Assert.Equal("2026-06-01T10:00:00.000Z", body.GetProperty("schedule_at").GetString());
        Assert.Equal("Hello from .NET", body.GetProperty("content")[0].GetProperty("text").GetString());
        Assert.Equal("acc_1", body.GetProperty("accounts")[0].GetString());
        Assert.Equal("label_1", body.GetProperty("labels")[0].GetString());
        Assert.Equal("A title", body.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Create_omits_the_keys_the_caller_left_alone()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Fixtures.Post}}}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync("ws_1", "Hello", new[] { "acc_1" });

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.False(body.TryGetProperty("schedule_at", out _));
        Assert.False(body.TryGetProperty("title", out _));
        Assert.False(body.TryGetProperty("labels", out _));
        Assert.Equal("draft", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Create_leaves_a_media_block_intact()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Fixtures.Post}}}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.CreateAsync(new CreatePostOptions
        {
            WorkspaceId = "ws_1",
            Accounts = new List<string> { "acc_1" },
            Content = new List<PostContent>
            {
                new("Look at this", new List<MediaItem>
                {
                    new()
                    {
                        Type = "image",
                        Name = "shot.png",
                        Url = "https://cdn.example.com/shot.png",
                        Alt = "A screenshot",
                    },
                }),
            },
        });

        var media = JsonDocument.Parse(handler.LastBody!).RootElement
            .GetProperty("content")[0].GetProperty("media")[0];
        Assert.Equal("image", media.GetProperty("type").GetString());
        Assert.Equal("https://cdn.example.com/shot.png", media.GetProperty("url").GetString());
        Assert.Equal("A screenshot", media.GetProperty("alt").GetString());
        Assert.False(media.TryGetProperty("size", out _));
    }

    [Fact]
    public async Task Update_sends_only_the_fields_that_were_set()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Fixtures.Post}}}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.UpdateAsync("post_1", new UpdatePostOptions { Title = "Renamed" });

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("Renamed", body.GetProperty("title").GetString());
        Assert.Single(body.EnumerateObject());
    }

    [Fact]
    public async Task Update_can_clear_a_field_by_setting_it_to_null()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Fixtures.Post}}}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.UpdateAsync("post_1", new UpdatePostOptions
        {
            Summary = Optional<string?>.Of(null),
            ScheduleAt = Optional<DateTimeOffset?>.Of(null),
        });

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal(JsonValueKind.Null, body.GetProperty("summary").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.GetProperty("schedule_at").ValueKind);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("cancel")]
    [InlineData("retry")]
    [InlineData("preflight")]
    public async Task The_publishing_actions_post_to_their_own_path(string action)
    {
        var handler = new StubHandler().Json("""{"data":{"queued":1}}""");
        using var test = new TestClient(handler);

        var body = action switch
        {
            "publish" => await test.Client.Posts.PublishAsync("post_1"),
            "cancel" => await test.Client.Posts.CancelAsync("post_1"),
            "retry" => await test.Client.Posts.RetryAsync("post_1"),
            _ => await test.Client.Posts.PreflightAsync("post_1"),
        };

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/posts/post_1/{action}",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(1, body!["queued"]!.GetValue<int>());
    }

    [Fact]
    public async Task Deliveries_bind_from_the_camelCase_the_endpoint_answers_with()
    {
        var handler = new StubHandler().Json("""
        {
          "data": [
            {
              "id": "del_1",
              "accountId": "acc_1",
              "status": "failed",
              "platform": "twitter",
              "accountName": "FoPost",
              "errorCode": "rate_limited",
              "attempts": 2,
              "maxAttempts": 3,
              "lastAttemptAt": "2026-08-12T10:05:00.000Z"
            }
          ]
        }
        """);
        using var test = new TestClient(handler);

        var deliveries = await test.Client.Posts.DeliveriesAsync("post_1");

        var delivery = Assert.Single(deliveries);
        Assert.Equal("acc_1", delivery.AccountId);
        Assert.Equal("rate_limited", delivery.ErrorCode);
        Assert.Equal(3, delivery.MaxAttempts);
        Assert.Equal("FoPost", delivery.AccountName);
    }

    /// <summary>One page of a two-page listing, meta and all.</summary>
    private static string Paged(int page) =>
        "{\"data\":[" + Fixtures.Post + "],\"meta\":{\"current_page\":" + page +
        ",\"per_page\":1,\"last_page\":2}}";
}
