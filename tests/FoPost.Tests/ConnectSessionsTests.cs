using System.Net;
using Xunit;

namespace FoPost.Tests;

public class ConnectSessionsTests
{
    private const string Session = """
    {"id":"cs_1","status":"pending","external_user_id":"user-42","platforms":["twitter","linkedin"],"redirect_url":"https://yourbrand.com/done","branding":{"name":"Your Brand","logo_url":"https://yourbrand.com/logo.png"},"account_id":null,"url":"https://app.fopost.com/connect/cs_1","expires_at":"2026-09-01T01:00:00Z","used_at":null,"created_at":"2026-09-01T00:00:00Z"}
    """;

    [Fact]
    public async Task Create_sends_the_camelCase_body_and_reads_the_link()
    {
        var handler = new StubHandler().Json(Session, HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var session = await test.Client.ConnectSessions.CreateAsync(new CreateConnectSessionOptions
        {
            WorkspaceId = "ws_1",
            ExternalUserId = "user-42",
            Platforms = new List<string> { Platforms.Twitter, Platforms.LinkedIn },
            RedirectUrl = "https://yourbrand.com/done",
            BrandingName = "Your Brand",
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/connect-sessions", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(
            """{"externalUserId":"user-42","platforms":["twitter","linkedin"],"workspaceId":"ws_1","redirectUrl":"https://yourbrand.com/done","branding":{"name":"Your Brand"}}""",
            handler.LastBody);
        Assert.Equal(ConnectSessionStatuses.Pending, session.Status);
        Assert.Equal("https://app.fopost.com/connect/cs_1", session.Url);
        Assert.Equal("https://yourbrand.com/logo.png", session.Branding!.LogoUrl);
        Assert.Null(session.UsedAt);
    }

    [Fact]
    public async Task Get_reads_the_session_by_id()
    {
        var handler = new StubHandler().Json(Session.Replace("\"status\":\"pending\"", "\"status\":\"used\"").Replace("\"account_id\":null", "\"account_id\":\"acc_1\""));
        using var test = new TestClient(handler);

        var session = await test.Client.ConnectSessions.GetAsync("cs_1");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/connect-sessions/cs_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(ConnectSessionStatuses.Used, session.Status);
        Assert.Equal("acc_1", session.AccountId);
    }
}
