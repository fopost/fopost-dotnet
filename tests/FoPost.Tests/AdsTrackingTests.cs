using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FoPost.Tests;

public class AdsTrackingTests
{
    [Fact]
    public async Task Pixel_conversions_send_the_idempotency_key_header()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"id":"batch_1","accepted":1,"eventCount":1,"replayed":false}}""")
            .Json("""{"data":{"id":"batch_1","accepted":1,"eventCount":1,"replayed":true}}""");
        using var test = new TestClient(handler);

        var options = new UploadConversionsOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "act_1",
            PixelId = "px_1",
            Events = new List<ConversionEvent>
            {
                new()
                {
                    EventName = "Purchase",
                    OccurredAt = "2026-10-18T10:04:00Z",
                    EventId = "order-77",
                    ActionSource = ConversionActionSources.Website,
                    Email = "buyer@yourbrand.com",
                    ValueMinor = 4999,
                    Currency = "USD",
                    ContentIds = new List<string> { "sku_1" },
                },
            },
        };

        var first = await test.Client.Ads.SendPixelConversionsAsync(options, "key-1");
        Assert.False(first.Replayed);
        Assert.EndsWith("/v1/ads/conversions/events", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("key-1", handler.LastRequest.Headers.GetValues("Idempotency-Key").Single());
        Assert.Contains("\"eventId\":\"order-77\"", handler.LastBody);
        Assert.Contains("\"actionSource\":\"website\"", handler.LastBody);
        Assert.Contains("\"contentIds\":[\"sku_1\"]", handler.LastBody);

        var replay = await test.Client.Ads.SendPixelConversionsAsync(options, "key-1");
        Assert.True(replay.Replayed);
        Assert.Equal("batch_1", replay.Id);
    }

    [Fact]
    public async Task Pixel_read_carries_the_account_on_the_query()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"px_1","name":"Site"}]}""")
            .Json("""{"data":{"id":"px_1","name":"Site","code":"<script></script>","createdAt":"2026-01-01T00:00:00Z","lastFiredAt":null,"isUnavailable":false,"dataUseSetting":"advertising_and_analytics"}}""")
            .Json("""{"data":[{"eventName":"Purchase","score":7.5,"matchKeys":[{"identifier":"email","coveragePercent":80}]}]}""");
        using var test = new TestClient(handler);

        var pixels = await test.Client.Ads.PixelsAsync("conn_1", "act_1", "ws_1");
        Assert.Equal("Site", Assert.Single(pixels).Name);
        Assert.Contains("ad_account_id=act_1", handler.LastRequest.RequestUri!.ToString());

        var pixel = await test.Client.Ads.PixelAsync("px_1", "conn_1", "act_1", "ws_1");
        Assert.EndsWith("/v1/ads/pixels/px_1?workspace_id=ws_1&connection_id=conn_1&ad_account_id=act_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("<script></script>", pixel.Code);
        Assert.Equal(PixelDataUseSettings.AdvertisingAndAnalytics, pixel.DataUseSetting);
        Assert.Null(pixel.LastFiredAt);

        var quality = await test.Client.Ads.EventMatchQualityAsync("px_1", "conn_1", "act_1", "ws_1");
        Assert.EndsWith("/v1/ads/pixels/px_1/match-quality?workspace_id=ws_1&connection_id=conn_1&ad_account_id=act_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(7.5, Assert.Single(quality).Score);
        Assert.Equal(80, quality[0].MatchKeys[0].CoveragePercent);
    }

    [Fact]
    public async Task Custom_conversion_create_sends_the_rule_and_reads_it_back()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"id":"cc_1","name":"Checkout","pixelId":"px_1","eventType":"PURCHASE","rule":{"eventName":"PageView","urlContains":"/thanks"},"description":null,"defaultValueMinor":1000,"isArchived":false,"isUnavailable":false,"createdAt":null,"lastFiredAt":null}}""")
            .Enqueue(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        var created = await test.Client.Ads.CreateCustomConversionAsync(new CreateCustomConversionOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "act_1",
            PixelId = "px_1",
            Name = "Checkout",
            EventType = CustomConversionEventTypes.Purchase,
            Rule = new CustomConversionRule { UrlContains = "/thanks" },
            DefaultValueMinor = 1000,
        });
        Assert.EndsWith("/v1/ads/custom-conversions", handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("\"eventType\":\"PURCHASE\"", handler.LastBody);
        Assert.Contains("\"rule\":{\"urlContains\":\"/thanks\"}", handler.LastBody);
        Assert.Equal("cc_1", created.Id);
        Assert.Equal("/thanks", created.Rule!.UrlContains);
        Assert.Equal(1000, created.DefaultValueMinor);

        await test.Client.Ads.DeleteCustomConversionAsync("cc_1", "ws_1", "conn_1", "act_1");
        Assert.Equal(System.Net.Http.HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Contains("\"adAccountId\":\"act_1\"", handler.LastBody);
    }
}
