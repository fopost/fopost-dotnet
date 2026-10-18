using Xunit;

namespace FoPost.Tests;

public class BlogsTests
{
    private const string Article = """
    {"data":{"id":"99","blog_id":"11","title":"Spring drop","body_html":"<p>Hello</p>",
             "excerpt":"A short summary","status":"published","author_name":"Store Owner",
             "tags":["news"],"image_url":"https://cdn.example/img.png",
             "url":"https://demo.myshopify.com/blogs/article/spring-drop",
             "published_at":"2026-09-01T10:00:00Z","updated_at":"2026-09-02T10:00:00Z"}}
    """;

    [Fact]
    public async Task Lists_the_blogs_on_the_site()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"11","title":"News","handle":"news","url":null}]}""");
        using var test = new TestClient(handler);

        var blogs = await test.Client.Blogs.ListBlogsAsync("acc_1");

        Assert.Equal("11", Assert.Single(blogs).Id);
        Assert.Equal("News", blogs[0].Title);
        Assert.Null(blogs[0].Url);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/blogs",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Lists_articles_with_the_filters()
    {
        var handler = new StubHandler().Json("""{"data":[]}""");
        using var test = new TestClient(handler);

        await test.Client.Blogs.ListArticlesAsync(
            "acc_1",
            "11",
            new ListArticlesOptions { Limit = 5, Status = "draft", Q = "spring" });

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/blogs/11/articles?limit=5&status=draft&q=spring",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Reads_an_article_into_its_fields()
    {
        var handler = new StubHandler().Json(Article);
        using var test = new TestClient(handler);

        var article = await test.Client.Blogs.GetArticleAsync("acc_1", "11", "99");

        Assert.Equal("99", article.Id);
        Assert.Equal("11", article.BlogId);
        Assert.Equal(new[] { "news" }, article.Tags);
        Assert.Equal("published", article.Status);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/blogs/11/articles/99",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Creates_an_article_with_only_the_fields_set()
    {
        var handler = new StubHandler().Json(Article);
        using var test = new TestClient(handler);

        await test.Client.Blogs.CreateArticleAsync(
            "acc_1",
            "11",
            new ArticleOptions { Title = "Spring drop", Body = "Hello", Status = "draft" });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/blogs/11/articles",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"title":"Spring drop","body":"Hello","status":"draft"}""", handler.LastBody);
    }

    /// <summary>The article is addressed by its own id, so an update never forks a duplicate.</summary>
    [Fact]
    public async Task Updates_the_live_article_in_place()
    {
        var handler = new StubHandler().Json(Article);
        using var test = new TestClient(handler);

        await test.Client.Blogs.UpdateArticleAsync(
            "acc_1",
            "11",
            "99",
            new ArticleOptions { Title = "Spring drop, restocked" });

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/blogs/11/articles/99",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"title":"Spring drop, restocked"}""", handler.LastBody);
    }

    [Fact]
    public async Task Deletes_an_article()
    {
        var handler = new StubHandler().Json("""{"data":null}""");
        using var test = new TestClient(handler);

        await test.Client.Blogs.DeleteArticleAsync("acc_1", "11", "99");

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/blogs/11/articles/99",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Lists_and_updates_products()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"7","title":"Mug","status":"active","price":"12.00","currency":"USD"}]}""")
            .Json("""{"data":{"id":"7","title":"Mug XL","status":"draft"}}""");
        using var test = new TestClient(handler);

        var products = await test.Client.Blogs.ListProductsAsync(
            "acc_1",
            new ListProductsOptions { Status = "active" });
        Assert.Equal("12.00", Assert.Single(products).Price);
        Assert.Equal("USD", products[0].Currency);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/acc_1/products?status=active",
            handler.LastRequest.RequestUri!.ToString());

        var updated = await test.Client.Blogs.UpdateProductAsync(
            "acc_1",
            "7",
            new ProductOptions { Title = "Mug XL", Status = "draft" });
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("draft", updated.Status);
        Assert.Equal("""{"title":"Mug XL","status":"draft"}""", handler.LastBody);
    }
}
