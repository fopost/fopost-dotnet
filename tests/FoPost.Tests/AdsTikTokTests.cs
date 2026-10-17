using System.Collections.Generic;
using Xunit;

namespace FoPost.Tests;

public class AdsTikTokTests
{
    [Fact]
    public async Task Identities_and_spark_posts_read_the_right_paths()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"bc1","name":"Brand HQ","role":"ADMIN"}]}""")
            .Json("""{"data":[{"id":"idt_1","type":"CUSTOMIZED_USER","name":"Your Brand"}]}""")
            .Json("""{"data":[{"id":"item_99","identityId":"idt_1","views":48213}]}""");
        using var test = new TestClient(handler);

        var centers = await test.Client.Ads.TikTokBusinessCentersAsync("conn_1", "ws_1");
        Assert.Equal("Brand HQ", Assert.Single(centers).Name);
        Assert.Contains("/v1/ads/tiktok/business-centers", handler.LastRequest.RequestUri!.ToString());

        var identities = await test.Client.Ads.TikTokIdentitiesAsync("conn_1", "7011", "ws_1");
        Assert.Equal("CUSTOMIZED_USER", Assert.Single(identities).Type);

        var posts = await test.Client.Ads.SparkPostsAsync("conn_1", "7011", "idt_1", "ws_1");
        Assert.Equal(48213, Assert.Single(posts).Views);
        Assert.Contains("identity_id=idt_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Spark_post_id_and_smart_plus_travel_in_the_body()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"id":"ad_1","workspaceId":"ws_1","kind":"ad","name":"Spark","goal":"traffic","status":"paused"}}""")
            .Json("""{"data":{"id":"c1","name":"Smart","status":"PAUSED"}}""");
        using var test = new TestClient(handler);

        await test.Client.Ads.CreateAsync(new CreateAdOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "7011",
            PageId = "idt_1",
            Name = "Spark",
            Goal = AdGoals.Traffic,
            Budget = new AdBudget(2000, AdBudgetTypes.Daily),
            Targeting = new AdTargeting { Countries = new List<string> { "US" }, AgeMin = 18, AgeMax = 44 },
            SparkPostId = "item_99",
        });
        Assert.Contains("\"sparkPostId\":\"item_99\"", handler.LastBody);

        await test.Client.Ads.CreateCampaignAsync(new CreateAdCampaignOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "7011",
            Name = "Smart",
            Goal = AdGoals.Traffic,
            SmartPlus = true,
        });
        Assert.Contains("\"smartPlus\":true", handler.LastBody);
    }

    [Fact]
    public async Task Conversions_report_what_the_network_accepted()
    {
        var handler = new StubHandler().Json("""{"data":{"accepted":2}}""");
        using var test = new TestClient(handler);

        var accepted = await test.Client.Ads.UploadConversionsAsync(new UploadConversionsOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "7011",
            PixelId = "px_1",
            Events = new List<ConversionEvent>
            {
                new() { EventName = "CompletePayment", OccurredAt = "2026-09-18T10:04:00Z", ValueMinor = 4999 },
            },
        });

        Assert.Equal(2, accepted);
        Assert.Contains("\"pixelId\":\"px_1\"", handler.LastBody);
    }

    [Fact]
    public async Task Comments_page_and_the_three_writes()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"comments":[{"id":"cm_1","text":"nice","likes":3,"hidden":true}],"nextCursor":"2"}}""")
            .Json("""{"data":{"replyId":"cm_2"}}""")
            .Json("""{"message":"Comment hidden"}""")
            .Json("""{"message":"Comment deleted"}""");
        using var test = new TestClient(handler);

        var page = await test.Client.Ads.CommentsAsync("conn_1", "ad_1", workspaceId: "ws_1");
        Assert.Equal("2", page.NextCursor);
        Assert.True(Assert.Single(page.Comments).Hidden);
        Assert.Equal(3, page.Comments[0].Likes);

        var scope = new AdCommentOptions { WorkspaceId = "ws_1", ConnectionId = "conn_1", AdId = "ad_1" };

        var replyId = await test.Client.Ads.ReplyToCommentAsync(
            "cm_1",
            new AdCommentOptions { WorkspaceId = "ws_1", ConnectionId = "conn_1", AdId = "ad_1", Text = "Friday!" });
        Assert.Equal("cm_2", replyId);
        Assert.EndsWith("/v1/ads/comments/cm_1/reply", handler.LastRequest.RequestUri!.ToString());

        await test.Client.Ads.SetCommentHiddenAsync(
            "cm_1",
            new AdCommentOptions { WorkspaceId = "ws_1", ConnectionId = "conn_1", AdId = "ad_1", Hidden = true });
        Assert.Contains("\"hidden\":true", handler.LastBody);

        await test.Client.Ads.DeleteCommentAsync("cm_1", scope);
        // The ad travels in the body, because the path already carries the comment.
        Assert.Equal(System.Net.Http.HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Contains("\"adId\":\"ad_1\"", handler.LastBody);
    }
}
