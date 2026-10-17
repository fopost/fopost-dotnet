using Xunit;

namespace FoPost.Tests;

public class MetaMessagingTests
{
    [Fact]
    public async Task Ice_breakers_round_trip()
    {
        const string body = """{"data":{"ice_breakers":[{"question":"What are your hours?","payload":"HOURS"}]}}""";
        var handler = new StubHandler()
            .Json(body)
            .Json(body)
            .Json("""{"data":{"ice_breakers":[]}}""");
        using var test = new TestClient(handler);
        var path = $"{TestClient.BaseUrl}/v1/accounts/a1/messaging/ice-breakers";

        var got = await test.Client.Accounts.GetIceBreakersAsync("a1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(path, handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("HOURS", got.IceBreakers[0].Payload);

        var set = await test.Client.Accounts.SetIceBreakersAsync(
            "a1",
            [new MetaIceBreaker { Question = "What are your hours?", Payload = "HOURS" }]);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("""{"ice_breakers":[{"question":"What are your hours?","payload":"HOURS"}]}""", handler.LastBody);
        Assert.Equal("What are your hours?", set.IceBreakers[0].Question);

        var cleared = await test.Client.Accounts.DeleteIceBreakersAsync("a1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Empty(cleared.IceBreakers);
    }

    [Fact]
    public async Task A_link_menu_item_omits_the_payload_key()
    {
        var handler = new StubHandler().Json("""
        {"data":{"persistent_menu":[{"locale":"default","call_to_actions":[
          {"type":"web_url","title":"Shop","url":"https://example.com/shop"}]}]}}
        """);
        using var test = new TestClient(handler);

        var set = await test.Client.Accounts.SetPersistentMenuAsync(
            "a1",
            [MetaPersistentMenuEntry.DefaultLocale([MetaMenuItem.Link("Shop", "https://example.com/shop")])]);

        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/messaging/persistent-menu",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(
            """{"persistent_menu":[{"locale":"default","call_to_actions":[{"type":"web_url","title":"Shop","url":"https://example.com/shop"}]}]}""",
            handler.LastBody);
        Assert.Equal("https://example.com/shop", set.PersistentMenu[0].CallToActions[0].Url);
    }

    [Fact]
    public async Task The_greeting_defaults_its_locale()
    {
        var handler = new StubHandler().Json("""{"data":{"greeting":[{"locale":"default","text":"Hi!"}]}}""");
        using var test = new TestClient(handler);

        var saved = await test.Client.Accounts.SetGreetingAsync("a1", [MetaGreetingText.Of("Hi!")]);

        Assert.Equal("""{"greeting":[{"locale":"default","text":"Hi!"}]}""", handler.LastBody);
        Assert.Equal("default", saved.Greeting[0].Locale);
    }

    [Fact]
    public async Task A_lapsed_subscription_is_reported_and_resubscribed()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"subscribed":false,"fields":["feed"],"missing_fields":["messages"]}}""")
            .Json("""{"data":{"subscribed":true,"fields":["feed","messages"],"missing_fields":[]}}""");
        using var test = new TestClient(handler);

        var lapsed = await test.Client.Accounts.GetWebhookSubscriptionAsync("a1");
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/webhook-subscription",
            handler.LastRequest.RequestUri!.ToString());
        Assert.False(lapsed.Subscribed);
        Assert.Equal(["messages"], lapsed.MissingFields);

        var fixedUp = await test.Client.Accounts.ResubscribeWebhookAsync("a1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.True(fixedUp.Subscribed);
    }

    [Fact]
    public async Task Handover_passes_and_takes_control()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"app_id":"263902037430900","control":"passed"}}""")
            .Json("""{"data":{"app_id":null,"control":"taken"}}""");
        using var test = new TestClient(handler);

        var passed = await test.Client.Inbox.HandoverAsync("t_1", "a1", "263902037430900");
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/inbox/conversations/t_1/handover",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"account_id":"a1","app_id":"263902037430900"}""", handler.LastBody);
        Assert.Equal("passed", passed.Control);

        var taken = await test.Client.Inbox.HandoverAsync("t_1", "a1");
        Assert.Equal("""{"account_id":"a1"}""", handler.LastBody);
        Assert.Null(taken.AppId);
        Assert.Equal("taken", taken.Control);
    }
}
