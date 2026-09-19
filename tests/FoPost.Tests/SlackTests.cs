using System.Net;
using Xunit;

namespace FoPost.Tests;

public class SlackTests
{
    [Fact]
    public async Task List_channels_reads_the_flags()
    {
        var handler = new StubHandler().Json("""
        {"data":[{"id":"C1","name":"general","is_private":false,"is_member":true,"is_current":true},
                 {"id":"C2","name":"ops","is_private":true,"is_member":true,"is_current":false}]}
        """);
        using var test = new TestClient(handler);

        var channels = await test.Client.Accounts.ListSlackChannelsAsync("a1");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/slack/channels", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("general", channels[0].Name);
        Assert.True(channels[0].IsCurrent);
        Assert.True(channels[1].IsPrivate);
    }

    [Fact]
    public async Task List_members_keeps_null_names()
    {
        var handler = new StubHandler().Json("""
        {"data":[{"id":"U1","name":"sam","real_name":"Sam Rivers","display_name":null,"avatar":null,"is_bot":false}]}
        """);
        using var test = new TestClient(handler);

        var member = Assert.Single(await test.Client.Accounts.ListSlackMembersAsync("a1"));

        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/slack/members", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Sam Rivers", member.RealName);
        Assert.Null(member.DisplayName);
        Assert.False(member.IsBot);
    }

    [Fact]
    public async Task Identity_get_and_patch_send_only_the_fields_set()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"username":null,"icon_url":null,"icon_emoji":":rocket:"}}""")
            .Json("""{"data":{"username":"Release Bot","icon_url":null,"icon_emoji":null}}""");
        using var test = new TestClient(handler);
        var path = $"{TestClient.BaseUrl}/v1/accounts/a1/slack/identity";

        var got = await test.Client.Accounts.GetSlackIdentityAsync("a1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(path, handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(":rocket:", got.IconEmoji);

        var updated = await test.Client.Accounts.UpdateSlackIdentityAsync("a1", new UpdateSlackIdentityOptions
        {
            Username = "Release Bot",
            IconEmoji = Optional<string?>.Of(null),
        });
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal(path, handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"username":"Release Bot","icon_emoji":null}""", handler.LastBody);
        Assert.Equal("Release Bot", updated.Username);
    }

    [Fact]
    public async Task A_webhook_connection_is_a_conflict()
    {
        var handler = new StubHandler().Json(
            """{"error":"webhook_connection","message":"Reconnect with the Slack app"}""",
            HttpStatusCode.Conflict);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(() => test.Client.Accounts.ListSlackChannelsAsync("a1"));

        Assert.Equal(409, error.Status);
        Assert.Equal("webhook_connection", error.Code);
    }
}
