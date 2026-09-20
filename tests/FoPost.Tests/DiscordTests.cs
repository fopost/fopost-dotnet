using System.Net;
using Xunit;

namespace FoPost.Tests;

public class DiscordTests
{
    [Fact]
    public async Task List_channels_and_switch_the_current_one()
    {
        var handler = new StubHandler()
            .Json("""
            {"data":[{"id":"c2","name":"launches","type":0,"parent_id":null,"nsfw":false,"can_post":true,"is_current":true}]}
            """)
            .Json("""{"data":{"id":"c2","name":"launches","is_current":true}}""");
        using var test = new TestClient(handler);

        var channels = await test.Client.Accounts.ListDiscordChannelsAsync("a1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/discord/channels", handler.LastRequest.RequestUri!.ToString());
        Assert.True(channels[0].IsCurrent);

        await test.Client.Accounts.SwitchDiscordChannelAsync("a1", "c2");
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/discord/channels/current",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"channel_id":"c2"}""", handler.LastBody);
    }

    [Fact]
    public async Task Identity_patch_sends_only_the_fields_set()
    {
        var handler = new StubHandler().Json("""{"data":{"username":"Release Bot","avatar_url":null}}""");
        using var test = new TestClient(handler);

        var updated = await test.Client.Accounts.UpdateDiscordIdentityAsync("a1", new UpdateDiscordIdentityOptions
        {
            Username = "Release Bot",
        });

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/discord/identity", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"username":"Release Bot"}""", handler.LastBody);
        Assert.Equal("Release Bot", updated.Username);
    }

    [Fact]
    public async Task A_scheduled_event_round_trips()
    {
        const string Event = """
        {"id":"e1","name":"Launch stream","description":null,"channel_id":null,
         "location":"https://example.com/live","start_time":"2026-10-01T18:00:00.000Z",
         "end_time":"2026-10-01T19:00:00.000Z","status":"scheduled","user_count":0}
        """;
        var handler = new StubHandler()
            .Json($$"""{"data":{{Event}}}""", HttpStatusCode.Created)
            .Json($$"""{"data":[{{Event}}]}""")
            .Json("""{"data":{"id":"e1","name":"Launch stream","start_time":"x","status":"canceled"}}""")
            .Json("""{"data":{"deleted":true}}""");
        using var test = new TestClient(handler);

        var created = await test.Client.Accounts.CreateDiscordEventAsync("a1", new DiscordEventOptions
        {
            Name = "Launch stream",
            StartTime = "2026-10-01T18:00:00.000Z",
            EndTime = "2026-10-01T19:00:00.000Z",
            Location = "https://example.com/live",
        });
        Assert.Equal("e1", created.Id);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/a1/discord/events", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(
            """{"name":"Launch stream","start_time":"2026-10-01T18:00:00.000Z","end_time":"2026-10-01T19:00:00.000Z","location":"https://example.com/live"}""",
            handler.LastBody);

        Assert.Single(await test.Client.Accounts.ListDiscordEventsAsync("a1"));

        var updated = await test.Client.Accounts.UpdateDiscordEventAsync(
            "a1", "e1", new DiscordEventOptions { Status = "canceled" });
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("""{"status":"canceled"}""", handler.LastBody);
        Assert.Equal("canceled", updated.Status);

        var deleted = await test.Client.Accounts.DeleteDiscordEventAsync("a1", "e1");
        Assert.True(deleted.Deleted);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/discord/events/e1",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Members_roles_and_direct_messages()
    {
        var handler = new StubHandler()
            .Json("""
            {"data":[{"id":"u7","username":"ada","display_name":null,"nick":null,"avatar":null,
                      "is_bot":false,"roles":["r1"],"joined_at":null}]}
            """)
            .Json("""{"data":{"assigned":true}}""")
            .Json("""{"data":{"id":"m1","channel_id":"dm1"}}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var members = await test.Client.Accounts.ListDiscordMembersAsync("a1", "ada");
        Assert.Contains("q=ada", handler.LastRequest.RequestUri!.Query);
        Assert.Equal(new[] { "r1" }, members[0].Roles);

        var assigned = await test.Client.Accounts.AddDiscordMemberRoleAsync("a1", "r1", "u7");
        Assert.True(assigned.Assigned);
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/discord/roles/r1/members/u7",
            handler.LastRequest.RequestUri!.ToString());

        var sent = await test.Client.Accounts.SendDiscordDmAsync("a1", "u7", "hi");
        Assert.Equal("dm1", sent.ChannelId);
        Assert.Equal("""{"member_id":"u7","content":"hi"}""", handler.LastBody);
    }

    [Fact]
    public async Task A_webhook_connection_is_a_conflict()
    {
        var handler = new StubHandler().Json(
            """{"error":"webhook_connection","message":"Upgrade it to the bot first"}""",
            HttpStatusCode.Conflict);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(
            () => test.Client.Accounts.ListDiscordChannelsAsync("a1"));

        Assert.Equal(409, error.Status);
        Assert.Equal("webhook_connection", error.Code);
    }
}
