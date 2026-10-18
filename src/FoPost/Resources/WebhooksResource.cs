using FoPost.Http;
using static FoPost.Resources.ResourceHelpers;

namespace FoPost.Resources;

/// <summary>
/// <c>client.Webhooks</c> — the endpoints FoPost calls when something happens, and the
/// record of every delivery to them. Verify what arrives with <see cref="FoPostWebhooks"/>.
/// </summary>
/// <remarks>Every call needs the <c>webhooks</c> scope.</remarks>
public sealed class WebhooksResource
{
    private readonly FoPostHttpClient _http;

    internal WebhooksResource(FoPostHttpClient http) => _http = http;

    private static string WebhookPath(string webhookId) => $"/v1/webhooks/{Uri.EscapeDataString(webhookId)}";

    private static string DeliveryPath(string webhookId, string deliveryId) =>
        $"{WebhookPath(webhookId)}/deliveries/{Uri.EscapeDataString(deliveryId)}";

    public async Task<IReadOnlyList<Webhook>> ListAsync(CancellationToken cancellationToken = default)
    {
        var body = await _http.GetAsync("/v1/webhooks", null, cancellationToken).ConfigureAwait(false);
        return ToList<Webhook>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Create a webhook. The answer carries the signing <see cref="Webhook.Secret"/>; keep it.</summary>
    public async Task<Webhook> CreateAsync(
        CreateWebhookOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>
        {
            ["workspaceId"] = options.WorkspaceId,
            ["url"] = options.Url,
            ["events"] = options.Events,
        };
        if (options.Name is not null)
        {
            body["name"] = options.Name;
        }
        if (options.Secret is not null)
        {
            body["secret"] = options.Secret;
        }
        if (options.Headers is not null)
        {
            body["headers"] = options.Headers;
        }

        var response = await _http.PostAsync("/v1/webhooks", body, cancellationToken).ConfigureAwait(false);
        return Require<Webhook>(FoPostHttpClient.Unwrap(response));
    }

    /// <summary>Partial update: only the fields you set are sent.</summary>
    public async Task<Webhook> UpdateAsync(
        string webhookId,
        UpdateWebhookOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        var body = new Dictionary<string, object?>();
        if (options.Name.IsSet)
        {
            body["name"] = options.Name.Value;
        }
        if (options.Url.IsSet)
        {
            body["url"] = options.Url.Value;
        }
        if (options.Events.IsSet)
        {
            body["events"] = options.Events.Value;
        }
        if (options.Active.IsSet)
        {
            body["active"] = options.Active.Value;
        }
        if (options.Secret.IsSet)
        {
            body["secret"] = options.Secret.Value;
        }
        if (options.Headers.IsSet)
        {
            body["headers"] = options.Headers.Value;
        }

        var response = await _http.PutAsync(WebhookPath(webhookId), body, cancellationToken).ConfigureAwait(false);
        return Require<Webhook>(FoPostHttpClient.Unwrap(response));
    }

    public async Task DeleteAsync(
        string webhookId,
        CancellationToken cancellationToken = default)
    {
        await _http.DeleteAsync(WebhookPath(webhookId), null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Send a test event to the endpoint; the outcome lands in the deliveries list.</summary>
    public async Task TestAsync(
        string webhookId,
        CancellationToken cancellationToken = default)
    {
        await _http.PostAsync($"{WebhookPath(webhookId)}/test", null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Recent deliveries, newest first.</summary>
    public async Task<IReadOnlyList<WebhookDelivery>> DeliveriesAsync(
        string webhookId,
        ListWebhookDeliveriesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Dictionary<string, object?>
        {
            ["status"] = options?.Status,
            ["limit"] = options?.Limit,
        };
        var body = await _http
            .GetAsync($"{WebhookPath(webhookId)}/deliveries", query, cancellationToken)
            .ConfigureAwait(false);
        return ToList<WebhookDelivery>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>One delivery, with the payload that was sent.</summary>
    public async Task<WebhookDelivery> GetDeliveryAsync(
        string webhookId,
        string deliveryId,
        CancellationToken cancellationToken = default)
    {
        var body = await _http
            .GetAsync(DeliveryPath(webhookId, deliveryId), null, cancellationToken)
            .ConfigureAwait(false);
        return Require<WebhookDelivery>(FoPostHttpClient.Unwrap(body));
    }

    /// <summary>Send the same payload again, as a new attempt under the same delivery key.</summary>
    public async Task RedeliverAsync(
        string webhookId,
        string deliveryId,
        CancellationToken cancellationToken = default)
    {
        await _http
            .PostAsync($"{DeliveryPath(webhookId, deliveryId)}/redeliver", null, cancellationToken)
            .ConfigureAwait(false);
    }
}
