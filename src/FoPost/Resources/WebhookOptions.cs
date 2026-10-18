namespace FoPost;

/// <summary>The body of <see cref="Resources.WebhooksResource.CreateAsync"/>.</summary>
public sealed class CreateWebhookOptions
{
    public required string WorkspaceId { get; set; }

    public string? Name { get; set; }

    public required string Url { get; set; }

    /// <summary>See <see cref="WebhookEvents"/>.</summary>
    public required IList<string> Events { get; set; }

    /// <summary>Your own signing secret; omitted, the API generates one and returns it once.</summary>
    public string? Secret { get; set; }

    /// <summary>Custom headers sent with every delivery.</summary>
    public IDictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// The body of <see cref="Resources.WebhooksResource.UpdateAsync"/>. Only the fields you set
/// are sent.
/// </summary>
public sealed class UpdateWebhookOptions
{
    public Optional<string?> Name { get; set; }

    public Optional<string> Url { get; set; }

    public Optional<IList<string>> Events { get; set; }

    public Optional<bool> Active { get; set; }

    public Optional<string> Secret { get; set; }

    /// <summary>Replaces every custom header; null clears them.</summary>
    public Optional<IDictionary<string, string>?> Headers { get; set; }
}

/// <summary>Filters for <see cref="Resources.WebhooksResource.DeliveriesAsync"/>.</summary>
public sealed class ListWebhookDeliveriesOptions
{
    /// <summary><c>success</c> or <c>failed</c>.</summary>
    public string? Status { get; set; }

    public int? Limit { get; set; }
}
