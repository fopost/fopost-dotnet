using System.Net;
using Xunit;

namespace FoPost.Tests;

/// <summary>Workspace and label writes: the bodies they send and the paths they reach.</summary>
public class WorkspaceLabelTests
{
    private const string Workspace = """
    {"id":"ws_1","name":"Acme","slug":"acme","type":"TEAM","timezone":"UTC","language":"en","require_approval":true,"ai_alt_text_enabled":false,"brand_color":"#112233","connect_redirect_domains":["yourbrand.com"],"created_at":"2026-09-01T00:00:00Z"}
    """;

    private const string Label = """
    {"id":"lbl_1","name":"Launch","color":"#2563eb","workspace":{"id":"ws_1","name":"Acme"},"created_at":"2026-09-01T00:00:00Z"}
    """;

    [Fact]
    public async Task Workspaces_create_update_and_delete()
    {
        var handler = new StubHandler()
            .Json(Workspace, HttpStatusCode.Created)
            .Json(Workspace)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        var created = await test.Client.Workspaces.CreateAsync(new CreateWorkspaceOptions { Name = "Acme", Type = "TEAM" });
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/workspaces", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"name":"Acme","type":"TEAM"}""", handler.LastBody);
        Assert.True(created.RequireApproval);
        Assert.Equal("#112233", created.BrandColor);
        Assert.Equal("yourbrand.com", Assert.Single(created.ConnectRedirectDomains!));

        await test.Client.Workspaces.UpdateAsync("ws_1", new UpdateWorkspaceOptions
        {
            Name = "Acme Inc",
            Description = Optional<string?>.Of(null),
            RequireApproval = true,
            ConnectRedirectDomains = new List<string> { "yourbrand.com" },
        });
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/workspaces/ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(
            """{"name":"Acme Inc","description":null,"requireApproval":true,"connectRedirectDomains":["yourbrand.com"]}""",
            handler.LastBody);

        await test.Client.Workspaces.DeleteAsync("ws_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/workspaces/ws_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Labels_create_get_update_and_delete()
    {
        var handler = new StubHandler()
            .Json(Label, HttpStatusCode.Created)
            .Json(Label)
            .Json(Label)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        var created = await test.Client.Labels.CreateAsync("ws_1", "Launch", "#2563eb");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/labels", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"workspace_id":"ws_1","name":"Launch","color":"#2563eb"}""", handler.LastBody);
        Assert.Equal("lbl_1", created.Id);

        await test.Client.Labels.GetAsync("lbl_1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/labels/lbl_1", handler.LastRequest.RequestUri!.ToString());

        await test.Client.Labels.UpdateAsync("lbl_1", "Launch week", "#111111");
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal("""{"name":"Launch week","color":"#111111"}""", handler.LastBody);

        await test.Client.Labels.DeleteAsync("lbl_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/labels/lbl_1", handler.LastRequest.RequestUri!.ToString());
    }
}
