using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FoPost.Http;

namespace FoPost;

/// <summary>
/// Verifies a webhook the API delivered and parses its envelope. Works for every FoPost
/// event, and for the signed deliveries a <c>website</c> blog target receives.
/// </summary>
/// <remarks>
/// Verify the exact bytes that arrived, never re-serialised JSON. The timestamped
/// signature (<c>X-FoPost-Signature-256</c> over <c>{timestamp}.{body}</c>) is preferred and
/// refused past <see cref="Tolerance"/>; the plain body signature
/// (<c>X-FoPost-Signature</c>) is accepted only when no timestamp header was sent.
/// </remarks>
/// <example>
/// <code>
/// var envelope = FoPostWebhooks.Verify(rawBody, headers, secret);
/// if (envelope.Event == "article.published")
/// {
///     var article = envelope.DataAs&lt;BlogArticlePublishedPayload&gt;();
/// }
/// </code>
/// </example>
public static class FoPostWebhooks
{
    public const string SignatureHeader = "X-FoPost-Signature";
    public const string TimestampedSignatureHeader = "X-FoPost-Signature-256";
    public const string TimestampHeader = "X-FoPost-Timestamp";
    public const string EventHeader = "X-FoPost-Event";
    public const string DeliveryHeader = "X-FoPost-Delivery";

    /// <summary>How far a delivery's timestamp may sit from the clock before it is refused.</summary>
    public static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(300);

    /// <summary>Verifies a delivery whose body was read as text, exactly as received.</summary>
    public static FoPostWebhookEnvelope Verify(
        string rawBody,
        IEnumerable<KeyValuePair<string, string?>> headers,
        string secret)
    {
        ArgumentNullException.ThrowIfNull(rawBody);
        return Verify(Encoding.UTF8.GetBytes(rawBody), headers, secret);
    }

    /// <summary>Verifies a delivery from its raw bytes, headers and the webhook secret.</summary>
    /// <exception cref="FoPostWebhookVerificationException">The signature, timestamp or body did not check out.</exception>
    public static FoPostWebhookEnvelope Verify(
        ReadOnlySpan<byte> rawBody,
        IEnumerable<KeyValuePair<string, string?>> headers,
        string secret)
    {
        ArgumentNullException.ThrowIfNull(headers);
        if (string.IsNullOrEmpty(secret))
        {
            throw new FoPostWebhookVerificationException("A webhook secret is required");
        }

        var lookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, value) in headers)
        {
            if (value is not null)
            {
                lookup[name] = value;
            }
        }

        var key = Encoding.UTF8.GetBytes(secret);
        if (lookup.TryGetValue(TimestampHeader, out var timestampText))
        {
            if (!long.TryParse(timestampText.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp))
            {
                throw new FoPostWebhookVerificationException("The timestamp header is not a unix time");
            }

            var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(timestamp);
            if (age.Duration() > Tolerance)
            {
                throw new FoPostWebhookVerificationException("The delivery is outside the timestamp tolerance");
            }

            var prefix = Encoding.UTF8.GetBytes(timestampText.Trim() + ".");
            var signed = new byte[prefix.Length + rawBody.Length];
            prefix.CopyTo(signed, 0);
            rawBody.CopyTo(signed.AsSpan(prefix.Length));
            Check(lookup, TimestampedSignatureHeader, key, signed);
        }
        else
        {
            Check(lookup, SignatureHeader, key, rawBody);
        }

        return Parse(rawBody, lookup);
    }

    private static void Check(
        IReadOnlyDictionary<string, string> headers,
        string header,
        byte[] key,
        ReadOnlySpan<byte> signed)
    {
        if (!headers.TryGetValue(header, out var presented) || string.IsNullOrWhiteSpace(presented))
        {
            throw new FoPostWebhookVerificationException($"Missing {header} header");
        }

        var hex = presented.Trim();
        if (hex.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex["sha256=".Length..];
        }

        var expected = Encoding.ASCII.GetBytes(Convert.ToHexString(HMACSHA256.HashData(key, signed)).ToLowerInvariant());
        var actual = Encoding.ASCII.GetBytes(hex.ToLowerInvariant());
        // FixedTimeEquals returns false on a length mismatch; the length is not a secret.
        if (!CryptographicOperations.FixedTimeEquals(expected, actual))
        {
            throw new FoPostWebhookVerificationException("Signature verification failed");
        }
    }

    private static FoPostWebhookEnvelope Parse(ReadOnlySpan<byte> rawBody, IReadOnlyDictionary<string, string> headers)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(rawBody.ToArray());
        }
        catch (JsonException error)
        {
            throw new FoPostWebhookVerificationException("Body is not valid JSON", error);
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new FoPostWebhookVerificationException("Body is not a webhook envelope");
            }

            var eventName = root.TryGetProperty("event", out var eventNode) && eventNode.ValueKind == JsonValueKind.String
                ? eventNode.GetString()
                : headers.GetValueOrDefault(EventHeader);
            if (string.IsNullOrEmpty(eventName))
            {
                throw new FoPostWebhookVerificationException("Missing or unrecognized event");
            }

            var data = root.TryGetProperty("data", out var dataNode) ? dataNode.Clone() : default;
            DateTimeOffset? timestamp = root.TryGetProperty("timestamp", out var stamp) &&
                                        stamp.ValueKind == JsonValueKind.String &&
                                        stamp.TryGetDateTimeOffset(out var parsed)
                ? parsed
                : null;

            return new FoPostWebhookEnvelope(eventName, data, timestamp, headers.GetValueOrDefault(DeliveryHeader));
        }
    }
}

/// <summary>A verified delivery: the event, its data and when it was sent.</summary>
public sealed class FoPostWebhookEnvelope
{
    public FoPostWebhookEnvelope(string @event, JsonElement data, DateTimeOffset? timestamp, string? deliveryId)
    {
        Event = @event;
        Data = data;
        Timestamp = timestamp;
        DeliveryId = deliveryId;
    }

    /// <summary>For example <c>article.published</c> or <c>post.published</c>.</summary>
    public string Event { get; }

    /// <summary>The event-specific body, untyped. See <see cref="DataAs{T}"/>.</summary>
    public JsonElement Data { get; }

    public DateTimeOffset? Timestamp { get; }

    /// <summary>The <c>X-FoPost-Delivery</c> header, when sent.</summary>
    public string? DeliveryId { get; }

    /// <summary>Reads <see cref="Data"/> as a model, for example <see cref="BlogArticlePublishedPayload"/>.</summary>
    public T DataAs<T>()
    {
        if (Data.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new FoPostWebhookVerificationException("The envelope carries no data");
        }

        return Data.Deserialize<T>(FoPostJson.Options)
            ?? throw new FoPostWebhookVerificationException("The envelope data could not be read");
    }
}

/// <summary>A webhook delivery that could not be trusted: bad signature, stale timestamp or malformed body.</summary>
public sealed class FoPostWebhookVerificationException : Exception
{
    public FoPostWebhookVerificationException(string message)
        : base(message)
    {
    }

    public FoPostWebhookVerificationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
