using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace FoPost.Tests;

/// <summary>
/// The webhook verifier: the timestamped signature is preferred and bounded, the
/// plain body signature only stands in when no timestamp was sent.
/// </summary>
public class WebhooksTests
{
    private const string Secret = "whsec_test";

    private const string Body = """
    {"event":"article.published","data":{"id":"art_1","title":"Hello","slug":"hello","excerpt":null,"metaTitle":null,"metaDescription":null,"bodyMarkdown":"# Hello","bodyHtml":"<h1>Hello</h1>","featuredImage":{"mediaId":"media_1","url":"https://cdn.test/f.png","alt":"F"},"images":[],"tags":["a"],"keyword":"hello","wordCount":900,"publishedAt":"2026-10-11T09:00:00.000Z","publishedUrl":"https://yourbrand.com/blog/hello"},"timestamp":"2026-10-11T09:00:01.000Z"}
    """;

    private static string Sign(string payload) =>
        "sha256=" + Convert.ToHexString(
            HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();

    private static Dictionary<string, string?> Headers(string body, long? timestamp)
    {
        var headers = new Dictionary<string, string?>
        {
            ["X-FoPost-Event"] = "article.published",
            ["X-FoPost-Delivery"] = "dlv_1",
            ["X-FoPost-Signature"] = Sign(body),
        };
        if (timestamp is { } at)
        {
            headers["X-FoPost-Timestamp"] = at.ToString();
            headers["X-FoPost-Signature-256"] = Sign($"{at}.{body}");
        }

        return headers;
    }

    [Fact]
    public void A_fresh_timestamped_signature_passes_and_parses_the_envelope()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var envelope = FoPostWebhooks.Verify(Body, Headers(Body, now), Secret);

        Assert.Equal("article.published", envelope.Event);
        Assert.Equal("dlv_1", envelope.DeliveryId);
        Assert.Equal(new DateTimeOffset(2026, 10, 11, 9, 0, 1, TimeSpan.Zero), envelope.Timestamp);

        var article = envelope.DataAs<BlogArticlePublishedPayload>();
        Assert.Equal("hello", article.Slug);
        Assert.Equal("https://yourbrand.com/blog/hello", article.PublishedUrl);
        Assert.Equal("media_1", article.FeaturedImage!.MediaId);
        Assert.Equal(900, article.WordCount);
    }

    [Fact]
    public void Headers_are_matched_case_insensitively_and_bytes_verify_the_same()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var headers = Headers(Body, now)
            .ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value);

        var envelope = FoPostWebhooks.Verify(Encoding.UTF8.GetBytes(Body), headers, Secret);

        Assert.Equal("article.published", envelope.Event);
    }

    [Fact]
    public void A_tampered_body_fails()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var headers = Headers(Body, now);
        var tampered = Body.Replace("\"wordCount\":900", "\"wordCount\":901", StringComparison.Ordinal);

        var error = Assert.Throws<FoPostWebhookVerificationException>(
            () => FoPostWebhooks.Verify(tampered, headers, Secret));

        Assert.Equal("Signature verification failed", error.Message);
    }

    [Fact]
    public void A_stale_timestamp_fails_even_with_a_correct_signature()
    {
        var stale = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600;

        var error = Assert.Throws<FoPostWebhookVerificationException>(
            () => FoPostWebhooks.Verify(Body, Headers(Body, stale), Secret));

        Assert.Equal("The delivery is outside the timestamp tolerance", error.Message);
    }

    [Fact]
    public void The_plain_body_signature_only_stands_in_when_no_timestamp_was_sent()
    {
        var legacy = Headers(Body, null);

        var envelope = FoPostWebhooks.Verify(Body, legacy, Secret);
        Assert.Equal("article.published", envelope.Event);

        // With a timestamp present the plain signature alone is not enough.
        var withTimestamp = Headers(Body, null);
        withTimestamp["X-FoPost-Timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        Assert.Throws<FoPostWebhookVerificationException>(
            () => FoPostWebhooks.Verify(Body, withTimestamp, Secret));
    }

    [Fact]
    public void A_wrong_secret_fails()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Assert.Throws<FoPostWebhookVerificationException>(
            () => FoPostWebhooks.Verify(Body, Headers(Body, now), "whsec_other"));
    }
}
