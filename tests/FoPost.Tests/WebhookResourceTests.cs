using System.Net;
using Xunit;

namespace FoPost.Tests;

/// <summary>The webhooks resource: endpoints, test sends, deliveries and redelivery.</summary>
public class WebhookResourceTests
{
    private const string Hook = """
    {"id":"wh_1","workspace_id":"ws_1","name":"Ops","url":"https://yourbrand.com/hooks/fopost","secret":"whsec_abc","events":["post.published","delivery.failed"],"header_names":["X-Token"],"active":true,"last_triggered_at":null,"failure_count":0,"created_at":"2026-09-01T00:00:00Z"}
    """;

    private const string Delivery = """
    {"id":"dlv_1","webhook_id":"wh_1","event":"post.published","delivery_key":"post.published:post_1","attempt":2,"status":"failed","response_status":502,"response_snippet":"Bad Gateway","duration_ms":120,"payload_hash":"abc","next_retry_at":"2026-09-01T00:05:00Z","created_at":"2026-09-01T00:00:00Z"}
    """;

    [Fact]
    public async Task List_create_update_and_delete_webhooks()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":[{{Hook}}]}""")
            .Json(Hook, HttpStatusCode.Created)
            .Json(Hook)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        var hooks = await test.Client.Webhooks.ListAsync();
        Assert.Equal($"{TestClient.BaseUrl}/v1/webhooks", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("X-Token", Assert.Single(Assert.Single(hooks).HeaderNames));

        var created = await test.Client.Webhooks.CreateAsync(new CreateWebhookOptions
        {
            WorkspaceId = "ws_1",
            Name = "Ops",
            Url = "https://yourbrand.com/hooks/fopost",
            Events = new List<string> { WebhookEvents.PostPublished, WebhookEvents.DeliveryFailed },
            Headers = new Dictionary<string, string> { ["X-Token"] = "t" },
        });
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(
            """{"workspaceId":"ws_1","url":"https://yourbrand.com/hooks/fopost","events":["post.published","delivery.failed"],"name":"Ops","headers":{"X-Token":"t"}}""",
            handler.LastBody);
        Assert.Equal("whsec_abc", created.Secret);
        Assert.True(created.Active);

        await test.Client.Webhooks.UpdateAsync("wh_1", new UpdateWebhookOptions
        {
            Active = false,
            Headers = Optional<IDictionary<string, string>?>.Of(null),
        });
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/webhooks/wh_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"active":false,"headers":null}""", handler.LastBody);

        await test.Client.Webhooks.DeleteAsync("wh_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/webhooks/wh_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Test_deliveries_get_and_redeliver_hit_the_delivery_paths()
    {
        var handler = new StubHandler()
            .Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent))
            .Json($$"""{"data":[{{Delivery}}]}""")
            .Json(Delivery.TrimEnd().TrimEnd('}') + ""","payload":{"event":"post.published","data":{"id":"post_1"}}}""")
            .Enqueue(new HttpResponseMessage(HttpStatusCode.Accepted));
        using var test = new TestClient(handler);

        await test.Client.Webhooks.TestAsync("wh_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/webhooks/wh_1/test", handler.LastRequest.RequestUri!.ToString());

        var deliveries = await test.Client.Webhooks.DeliveriesAsync("wh_1", new ListWebhookDeliveriesOptions { Status = "failed", Limit = 10 });
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/webhooks/wh_1/deliveries?status=failed&limit=10", handler.LastRequest.RequestUri!.ToString());
        var delivery = Assert.Single(deliveries);
        Assert.Equal(502, delivery.ResponseStatus);
        Assert.Equal(new DateTimeOffset(2026, 9, 1, 0, 5, 0, TimeSpan.Zero), delivery.NextRetryAt);
        Assert.Null(delivery.Payload);

        var full = await test.Client.Webhooks.GetDeliveryAsync("wh_1", "dlv_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/webhooks/wh_1/deliveries/dlv_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("post_1", full.Payload!.Value.GetProperty("data").GetProperty("id").GetString());

        await test.Client.Webhooks.RedeliverAsync("wh_1", "dlv_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/webhooks/wh_1/deliveries/dlv_1/redeliver", handler.LastRequest.RequestUri!.ToString());
    }
}
