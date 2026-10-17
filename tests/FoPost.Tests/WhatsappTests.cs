using System.Text.Json.Nodes;
using Xunit;

namespace FoPost.Tests;

public class WhatsappTests
{
    [Fact]
    public void Whatsapp_is_on_the_platform_list()
    {
        Assert.Contains(Platforms.WhatsApp, Platforms.All);
        Assert.Equal("whatsapp", Platforms.WhatsApp);
    }

    [Fact]
    public async Task Create_template_returns_the_review_status_the_platform_gave_it()
    {
        var handler = new StubHandler().Json("""
        {"data":{"id":"tpl-1","name":"order_shipped","language":"en_US","category":"UTILITY",
                 "status":"PENDING","rejectedReason":null,"components":[],"qualityScore":null}}
        """);
        using var test = new TestClient(handler);

        var template = await test.Client.Whatsapp.CreateTemplateAsync("a1", new CreateWhatsappTemplateOptions
        {
            Name = "order_shipped",
            Language = "en_US",
            Category = "UTILITY",
            Components = new List<JsonNode?> { JsonNode.Parse("""{"type":"BODY","text":"On its way."}""") },
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/accounts/a1/whatsapp/templates",
            handler.LastRequest.RequestUri!.ToString());
        // Nothing marks a template approved but the platform.
        Assert.Equal("PENDING", template.Status);
        Assert.Equal("order_shipped", template.Name);
    }

    [Fact]
    public async Task Delete_template_names_it_in_the_query()
    {
        var handler = new StubHandler().Json("""{"data":{"deleted":true}}""");
        using var test = new TestClient(handler);

        await test.Client.Whatsapp.DeleteTemplateAsync("a1", "tpl-1", "order_shipped");

        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.EndsWith(
            "/whatsapp/templates/tpl-1?name=order_shipped",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Sandbox_session_carries_only_the_last_four_digits()
    {
        var handler = new StubHandler().Json("""
        {"data":{"id":"ses-1","status":"invited","phoneNumberLast4":"4567",
                 "invitedAt":"2026-09-20T10:00:00Z","activatedAt":null,"expiresAt":"2026-09-21T10:00:00Z"}}
        """);
        using var test = new TestClient(handler);

        var session = await test.Client.Whatsapp.CreateSandboxSessionAsync("ws", "+15551234567");

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/whatsapp/sandbox/sessions",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("4567", session.PhoneNumberLast4);
        Assert.Equal("invited", session.Status);
    }
}
