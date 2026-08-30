using System.Net;
using System.Text;
using Xunit;

namespace FoPost.Tests;

public class HttpTests
{
    [Fact]
    public async Task Null_query_values_are_dropped_rather_than_sent_empty()
    {
        var handler = new StubHandler().Json("""{"data":[]}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.ListAsync(new ListPostsOptions { WorkspaceId = "ws_1", Status = null });

        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("workspace_id=ws_1", query, StringComparison.Ordinal);
        Assert.DoesNotContain("status=", query, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_429_is_retried_for_the_interval_the_api_asks_for()
    {
        var handler = new StubHandler()
            .TooManyRequests("2")
            .Json($$"""{"data":[{{Fixtures.Post}}]}""");
        using var test = new TestClient(handler);

        var page = await test.Client.Posts.ListAsync();

        Assert.Single(page);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(TimeSpan.FromSeconds(2), Assert.Single(test.Waits));
    }

    [Fact]
    public async Task Retrying_gives_up_after_max_retries_and_raises()
    {
        var handler = new StubHandler()
            .TooManyRequests("1")
            .TooManyRequests("1")
            .TooManyRequests("1");
        using var test = new TestClient(handler, new FoPostClientOptions { MaxRetries = 3 });

        var error = await Assert.ThrowsAsync<FoPostRateLimitException>(
            () => test.Client.Posts.ListAsync());

        Assert.Equal(3, handler.Requests.Count);
        Assert.Equal(TimeSpan.FromSeconds(1), error.RetryAfter);
        Assert.Equal("too_many_requests", error.Code);
    }

    [Fact]
    public async Task A_429_without_a_retry_after_waits_the_fallback_second()
    {
        var handler = new StubHandler()
            .TooManyRequests()
            .Json("""{"data":[]}""");
        using var test = new TestClient(handler);

        await test.Client.Posts.ListAsync();

        Assert.Equal(TimeSpan.FromSeconds(1), Assert.Single(test.Waits));
    }

    [Fact]
    public async Task A_204_decodes_as_no_body_rather_than_throwing()
    {
        var handler = new StubHandler().Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        await test.Client.Posts.DeleteAsync("post_1");

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
    }

    [Fact]
    public async Task An_html_error_page_becomes_a_FoPostException_carrying_the_text()
    {
        var handler = new StubHandler().Enqueue(new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("<html>bad gateway</html>", Encoding.UTF8, "text/html"),
        });
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(() => test.Client.Posts.ListAsync());

        Assert.Equal(502, error.Status);
        Assert.Contains("bad gateway", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_non_json_success_is_refused_instead_of_silently_returning_nothing()
    {
        var handler = new StubHandler().Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json", Encoding.UTF8, "text/plain"),
        });
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(() => test.Client.Posts.ListAsync());

        Assert.Contains("Expected a JSON response", error.Message, StringComparison.Ordinal);
    }
}
