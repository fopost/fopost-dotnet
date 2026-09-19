using System.Net;
using Xunit;

namespace FoPost.Tests;

public class TelegramTests
{
    private const string Commands = """
    {"data":{"commands":[{"command":"start","description":"Start the bot"}]}}
    """;

    [Fact]
    public async Task Create_connect_code_sends_the_workspace_and_reads_the_links()
    {
        var handler = new StubHandler()
            .Json("""
            {"data":{"code":"ABC123","command":"/connect ABC123","bot_username":"fopost_bot",
                     "deep_link":"https://t.me/fopost_bot?start=ABC123","group_link":null,
                     "expires_at":"2026-09-19T12:15:00Z"}}
            """, HttpStatusCode.Created)
            .Json("""{"data":{"code":"X","command":"/connect X","expires_at":"2026-09-19T12:15:00Z"}}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var code = await test.Client.Accounts.CreateTelegramConnectCodeAsync("w1");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/telegram/connect-code",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"workspaceId":"w1"}""", handler.LastBody);
        Assert.Equal("fopost_bot", code.BotUsername);
        Assert.Equal("https://t.me/fopost_bot?start=ABC123", code.DeepLink);
        Assert.Null(code.GroupLink);
        Assert.Equal(new DateTimeOffset(2026, 9, 19, 12, 15, 0, TimeSpan.Zero), code.ExpiresAt);

        await test.Client.Accounts.CreateTelegramConnectCodeAsync();
        Assert.Equal("{}", handler.LastBody);
    }

    [Fact]
    public async Task Connect_status_queries_the_code()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"status":"failed","account_id":null,"reason":"card_required"}}""");
        using var test = new TestClient(handler);

        var status = await test.Client.Accounts.GetTelegramConnectStatusAsync("ABC123");

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/telegram/connect-code/status?code=ABC123",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("failed", status.Status);
        Assert.Null(status.AccountId);
        Assert.Equal("card_required", status.Reason);
    }

    [Fact]
    public async Task Bot_commands_get_set_and_delete_hit_the_account_path()
    {
        var handler = new StubHandler()
            .Json(Commands)
            .Json(Commands)
            .Json("""{"data":{"commands":[]}}""");
        using var test = new TestClient(handler);
        var path = $"{TestClient.BaseUrl}/v1/accounts/a1/telegram/commands";

        var got = await test.Client.Accounts.GetTelegramBotCommandsAsync("a1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(path, handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("start", Assert.Single(got.Commands).Command);

        await test.Client.Accounts.SetTelegramBotCommandsAsync(
            "a1",
            new[] { new TelegramBotCommand { Command = "start", Description = "Start the bot" } });
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal(path, handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"commands":[{"command":"start","description":"Start the bot"}]}""", handler.LastBody);

        var cleared = await test.Client.Accounts.DeleteTelegramBotCommandsAsync("a1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal(path, handler.LastRequest.RequestUri!.ToString());
        Assert.Empty(cleared.Commands);
    }
}
