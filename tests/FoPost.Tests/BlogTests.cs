using System.Net;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

/// <summary>
/// The blog resource: paths, the camelCase <c>workspaceId</c> on query and body,
/// partial bodies, and that camelCase answers read back into the models.
/// </summary>
public class BlogTests
{
    private const string Settings = """
    {
      "siteUrl": "https://yourbrand.com",
      "topic": "social media scheduling",
      "audience": "marketing teams",
      "language": "en",
      "locationCode": 2840,
      "cadencePerWeek": 2,
      "reviewHoldDays": 3,
      "autoPublish": false,
      "imageTier": "standard",
      "inBodyImages": 1,
      "minWords": 900,
      "maxWords": 1600,
      "brandVoiceId": null,
      "targetKind": "website",
      "targetAccountId": null,
      "targetPlatform": null,
      "websiteUrl": "https://yourbrand.com/api/fopost",
      "hasWebsiteSecret": true,
      "keywordDataAvailable": true,
      "updatedAt": "2026-10-01T00:00:00.000Z"
    }
    """;

    private const string Keyword = """
    {
      "id": "kw_1",
      "keyword": "social media scheduling",
      "searchVolume": 5400,
      "difficulty": 42,
      "cpc": 3.1,
      "intent": "commercial",
      "source": "provider",
      "status": "planned",
      "plannedFor": "2026-10-20",
      "createdAt": "2026-10-01T00:00:00.000Z"
    }
    """;

    private const string Article = """
    {
      "id": "art_1",
      "title": "How to schedule a week of posts in an hour",
      "slug": "schedule-a-week-of-posts",
      "keyword": "social media scheduling",
      "excerpt": null,
      "metaTitle": null,
      "metaDescription": null,
      "featuredImage": { "mediaId": "media_1", "url": "https://cdn.test/featured.png", "alt": "A calendar" },
      "tags": ["scheduling"],
      "wordCount": 1200,
      "status": "draft",
      "origin": "user",
      "reviewUntil": "2026-10-05T00:00:00.000Z",
      "publishAt": null,
      "publishedAt": null,
      "publishedUrl": null,
      "error": null,
      "createdAt": "2026-10-01T00:00:00.000Z",
      "updatedAt": "2026-10-02T00:00:00.000Z"
    }
    """;

    // ─── Settings ────────────────────────────────────────────────────

    [Fact]
    public async Task GetSettings_sends_workspaceId_in_camel_case_and_reads_the_target()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{Settings}} }""");
        using var test = new TestClient(handler);

        var settings = await test.Client.Blog.GetSettingsAsync("ws_1");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("/v1/blog/settings", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("?workspaceId=ws_1", handler.LastRequest.RequestUri!.Query);
        Assert.Equal(BlogTargetKinds.Website, settings.TargetKind);
        Assert.True(settings.HasWebsiteSecret);
        Assert.Equal(2, settings.CadencePerWeek);
    }

    [Fact]
    public async Task UpdateSettings_puts_workspaceId_in_the_body_and_returns_the_secret_once()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{Settings}}, "websiteSecret": "whsec_new" }""");
        using var test = new TestClient(handler);

        var result = await test.Client.Blog.UpdateSettingsAsync("ws_1", new UpdateBlogSettingsOptions
        {
            WebsiteUrl = "https://yourbrand.com/api/fopost",
            TargetKind = BlogTargetKinds.Website,
            BrandVoiceId = Optional<string?>.Of(null),
        });

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("/v1/blog/settings", handler.LastRequest.RequestUri!.AbsolutePath);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("ws_1", body.GetProperty("workspaceId").GetString());
        Assert.Equal("website", body.GetProperty("targetKind").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("brandVoiceId").ValueKind);
        Assert.False(body.TryGetProperty("cadencePerWeek", out _));
        Assert.Equal("whsec_new", result.WebsiteSecret);
        Assert.Equal("en", result.Settings.Language);
    }

    [Fact]
    public async Task UpdateSettings_reads_a_null_secret_when_nothing_was_issued()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{Settings}}, "websiteSecret": null }""");
        using var test = new TestClient(handler);

        var result = await test.Client.Blog.UpdateSettingsAsync("ws_1", new UpdateBlogSettingsOptions { Topic = "x" });

        Assert.Null(result.WebsiteSecret);
    }

    [Fact]
    public async Task RotateWebsiteSecret_and_TestWebsite_post_the_workspace_as_body()
    {
        var handler = new StubHandler()
            .Json("""{ "data": { "websiteSecret": "whsec_rotated" } }""")
            .Json("""{ "data": { "ok": true, "status": 200, "error": null } }""");
        using var test = new TestClient(handler);

        var secret = await test.Client.Blog.RotateWebsiteSecretAsync("ws_1");
        var check = await test.Client.Blog.TestWebsiteAsync("ws_1");

        Assert.Equal("/v1/blog/settings/website-secret", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("""{"workspaceId":"ws_1"}""", handler.Bodies[0]);
        Assert.Equal("whsec_rotated", secret.WebsiteSecret);
        Assert.Equal("/v1/blog/settings/website-test", handler.Requests[1].RequestUri!.AbsolutePath);
        Assert.True(check.Ok);
        Assert.Equal(200, check.Status);
    }

    [Fact]
    public async Task TestWebsite_reads_a_failed_test_off_the_502_instead_of_throwing()
    {
        var handler = new StubHandler().Json(
            """{ "data": { "ok": false, "status": 500, "error": "Internal Server Error" } }""",
            HttpStatusCode.BadGateway);
        using var test = new TestClient(handler);

        var check = await test.Client.Blog.TestWebsiteAsync("ws_1");

        Assert.False(check.Ok);
        Assert.Equal(500, check.Status);
        Assert.Equal("Internal Server Error", check.Error);
    }

    // ─── Keywords ────────────────────────────────────────────────────

    [Fact]
    public async Task ListKeywords_joins_statuses_and_sends_the_limit()
    {
        var handler = new StubHandler().Json($$"""{ "data": [{{Keyword}}] }""");
        using var test = new TestClient(handler);

        var keywords = await test.Client.Blog.ListKeywordsAsync("ws_1", new ListBlogKeywordsOptions
        {
            Status = new[] { BlogKeywordStatuses.Suggested, BlogKeywordStatuses.Planned },
            Limit = 50,
        });

        Assert.Equal("/v1/blog/keywords", handler.LastRequest.RequestUri!.AbsolutePath);
        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("workspaceId=ws_1", query, StringComparison.Ordinal);
        Assert.Contains("status=suggested%2Cplanned", query, StringComparison.Ordinal);
        Assert.Contains("limit=50", query, StringComparison.Ordinal);

        var keyword = Assert.Single(keywords);
        Assert.Equal(5400, keyword.SearchVolume);
        Assert.Equal("2026-10-20", keyword.PlannedFor);
    }

    [Fact]
    public async Task PlanKeywords_posts_seeds_and_reads_queued()
    {
        var handler = new StubHandler().Json("""{ "data": { "status": "queued" } }""", HttpStatusCode.Accepted);
        using var test = new TestClient(handler);

        var result = await test.Client.Blog.PlanKeywordsAsync("ws_1", new PlanBlogKeywordsOptions
        {
            Seeds = new[] { "scheduling" },
        });

        Assert.Equal("/v1/blog/keywords/plan", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"workspaceId":"ws_1","seeds":["scheduling"]}""", handler.LastBody);
        Assert.Equal("queued", result.Status);
    }

    [Fact]
    public async Task PlanKeywords_surfaces_an_empty_balance_as_payment_required()
    {
        var handler = new StubHandler().Json(
            """{"error":"insufficient_credits","message":"Out of credits","upgrade_url":"/settings/billing"}""",
            HttpStatusCode.PaymentRequired);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostPaymentRequiredException>(
            () => test.Client.Blog.PlanKeywordsAsync("ws_1"));

        Assert.Equal("insufficient_credits", error.Code);
        Assert.Equal("/settings/billing", error.UpgradeUrl);
    }

    [Fact]
    public async Task AddKeyword_formats_the_planned_day_and_UpdateKeyword_patches_with_the_workspace_query()
    {
        var handler = new StubHandler()
            .Json($$"""{ "data": {{Keyword}} }""", HttpStatusCode.Created)
            .Json($$"""{ "data": {{Keyword}} }""");
        using var test = new TestClient(handler);

        await test.Client.Blog.AddKeywordAsync("ws_1", new CreateBlogKeywordOptions
        {
            Keyword = "social media scheduling",
            PlannedFor = new DateOnly(2026, 10, 20),
        });
        await test.Client.Blog.UpdateKeywordAsync("ws_1", "kw_1", new UpdateBlogKeywordOptions
        {
            PlannedFor = Optional<DateOnly?>.Of(null),
        });

        Assert.Equal(HttpMethod.Post, handler.Requests[0].Method);
        Assert.Equal("/v1/blog/keywords", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal(
            """{"workspaceId":"ws_1","keyword":"social media scheduling","plannedFor":"2026-10-20"}""",
            handler.Bodies[0]);

        Assert.Equal(HttpMethod.Patch, handler.Requests[1].Method);
        Assert.Equal("/v1/blog/keywords/kw_1", handler.Requests[1].RequestUri!.AbsolutePath);
        Assert.Equal("?workspaceId=ws_1", handler.Requests[1].RequestUri!.Query);
        Assert.Equal("""{"plannedFor":null}""", handler.Bodies[1]);
    }

    [Fact]
    public async Task DeleteKeyword_sends_the_workspace_on_the_query()
    {
        var handler = new StubHandler().Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        await test.Client.Blog.DeleteKeywordAsync("ws_1", "kw_1");

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/v1/blog/keywords/kw_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("?workspaceId=ws_1", handler.LastRequest.RequestUri!.Query);
        Assert.Null(handler.LastBody);
    }

    // ─── Articles ────────────────────────────────────────────────────

    [Fact]
    public async Task ListArticles_filters_and_reads_the_featured_image()
    {
        var handler = new StubHandler().Json($$"""{ "data": [{{Article}}] }""");
        using var test = new TestClient(handler);

        var articles = await test.Client.Blog.ListArticlesAsync("ws_1", new ListBlogArticlesOptions
        {
            Status = new[] { BlogArticleStatuses.Draft },
        });

        Assert.Equal("/v1/blog/articles", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("status=draft", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        var article = Assert.Single(articles);
        Assert.Equal("media_1", article.FeaturedImage!.MediaId);
        Assert.Equal(1200, article.WordCount);
        Assert.Equal(new[] { "scheduling" }, article.Tags);
    }

    [Fact]
    public async Task RequestArticle_sends_only_what_was_given_and_reads_the_queued_article()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{Article}} }""", HttpStatusCode.Accepted);
        using var test = new TestClient(handler);

        var article = await test.Client.Blog.RequestArticleAsync("ws_1", new RequestBlogArticleOptions
        {
            KeywordId = "kw_1",
            Instructions = "Keep it practical.",
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/blog/articles", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(
            """{"workspaceId":"ws_1","keywordId":"kw_1","instructions":"Keep it practical."}""",
            handler.LastBody);
        Assert.Equal("art_1", article.Id);
    }

    [Fact]
    public async Task GetArticle_reads_the_full_body_and_images()
    {
        var full = Article.TrimEnd().TrimEnd('}') + """
            ,
              "instructions": null,
              "outline": ["Why batch", "How to batch"],
              "brief": { "sources": 3 },
              "bodyMarkdown": "# Hello",
              "bodyHtml": "<h1>Hello</h1>",
              "images": [{ "mediaId": "media_2", "url": "https://cdn.test/one.png", "alt": "One", "position": 1 }]
            }
            """;
        var handler = new StubHandler().Json($$"""{ "data": {{full}} }""");
        using var test = new TestClient(handler);

        var article = await test.Client.Blog.GetArticleAsync("ws_1", "art_1");

        Assert.Equal("/v1/blog/articles/art_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("?workspaceId=ws_1", handler.LastRequest.RequestUri!.Query);
        Assert.Equal("# Hello", article.BodyMarkdown);
        Assert.Equal(2, article.Outline.Count);
        Assert.Equal(1, Assert.Single(article.Images).Position);
        Assert.Equal(3, article.Brief!.Value.GetProperty("sources").GetInt32());
    }

    [Fact]
    public async Task UpdateArticle_patches_only_set_fields_and_formats_publishAt()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{Article}} }""");
        using var test = new TestClient(handler);

        await test.Client.Blog.UpdateArticleAsync("ws_1", "art_1", new UpdateBlogArticleOptions
        {
            Title = "New title",
            Tags = Optional<IReadOnlyList<string>>.Of(new[] { "a", "b" }),
            PublishAt = Optional<DateTimeOffset?>.Of(new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.Zero)),
        });

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("/v1/blog/articles/art_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("?workspaceId=ws_1", handler.LastRequest.RequestUri!.Query);
        Assert.Equal(
            """{"title":"New title","tags":["a","b"],"publishAt":"2026-10-20T09:00:00.000Z"}""",
            handler.LastBody);
    }

    [Fact]
    public async Task Regenerate_Publish_and_Reject_post_to_the_action_path()
    {
        var handler = new StubHandler()
            .Json($$"""{ "data": {{Article}} }""", HttpStatusCode.Accepted)
            .Json($$"""{ "data": {{Article}} }""", HttpStatusCode.Accepted)
            .Json($$"""{ "data": {{Article}} }""");
        using var test = new TestClient(handler);

        await test.Client.Blog.RegenerateArticleAsync("ws_1", "art_1");
        await test.Client.Blog.PublishArticleAsync("ws_1", "art_1");
        await test.Client.Blog.RejectArticleAsync("ws_1", "art_1");

        Assert.All(handler.Requests, request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("?workspaceId=ws_1", request.RequestUri!.Query);
        });
        Assert.Equal("/v1/blog/articles/art_1/regenerate", handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Equal("/v1/blog/articles/art_1/publish", handler.Requests[1].RequestUri!.AbsolutePath);
        Assert.Equal("/v1/blog/articles/art_1/reject", handler.Requests[2].RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task Publish_without_a_target_is_a_409_with_its_code()
    {
        var handler = new StubHandler().Json(
            """{"error":"no_target","message":"Set a target first"}""",
            HttpStatusCode.Conflict);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(
            () => test.Client.Blog.PublishArticleAsync("ws_1", "art_1"));

        Assert.Equal(409, error.Status);
        Assert.Equal("no_target", error.Code);
    }

    [Fact]
    public async Task DeleteArticle_sends_the_workspace_on_the_query()
    {
        var handler = new StubHandler().Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        await test.Client.Blog.DeleteArticleAsync("ws_1", "art_1");

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal("/v1/blog/articles/art_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("?workspaceId=ws_1", handler.LastRequest.RequestUri!.Query);
    }
}
