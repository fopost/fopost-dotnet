using Xunit;

namespace FoPost.Tests;

public class UnpublishTests
{
    [Fact]
    public async Task Unpublish_posts_to_the_action_path_and_reads_what_was_removed()
    {
        var handler = new StubHandler().Json("""
        {"post_status":"unpublished","deliveries":[{"account_id":"acc_1","platform":"twitter","removed":true},{"account_id":"acc_2","platform":"linkedin","removed":false}]}
        """);
        using var test = new TestClient(handler);

        var result = await test.Client.Posts.UnpublishAsync("post_1");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/posts/post_1/unpublish", handler.LastRequest.RequestUri!.ToString());
        Assert.Null(handler.LastBody);
        Assert.Equal(PostStatuses.Unpublished, result.PostStatus);
        Assert.Equal(2, result.Deliveries.Count);
        Assert.False(result.Deliveries[1].Removed);
    }
}
