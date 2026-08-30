using System.Net;
using System.Text;

namespace FoPost.Tests;

/// <summary>One canned response, and the request that asked for it.</summary>
internal sealed class StubHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();

    public List<HttpRequestMessage> Requests { get; } = new();

    public List<string?> Bodies { get; } = new();

    public HttpRequestMessage LastRequest => Requests[^1];

    public string? LastBody => Bodies[^1];

    public StubHandler Enqueue(HttpResponseMessage response)
    {
        _responses.Enqueue(response);
        return this;
    }

    public StubHandler Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        Enqueue(new HttpResponseMessage(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        });

    public StubHandler TooManyRequests(string? retryAfter = null)
    {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
        {
            Content = new StringContent(
                """{"error":"too_many_requests","message":"Slow down"}""",
                Encoding.UTF8,
                "application/json"),
        };
        if (retryAfter is not null)
        {
            response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        }

        return Enqueue(response);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        Bodies.Add(request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken));

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException($"No stubbed response for {request.Method} {request.RequestUri}");
        }

        return _responses.Dequeue();
    }
}

/// <summary>A client wired to a stub, with the retry wait replaced by a recorder.</summary>
internal sealed class TestClient : IDisposable
{
    public const string BaseUrl = "https://api.test.fopost.com";
    public const string ApiKey = "fp_test_key";

    public TestClient(StubHandler handler, FoPostClientOptions? options = null)
    {
        Handler = handler;
        options ??= new FoPostClientOptions();
        if (options.ApiKey is null && options.BearerToken is null)
        {
            options.ApiKey = ApiKey;
        }
        options.BaseUrl = BaseUrl;
        options.HttpMessageHandler = handler;

        Client = new FoPostClient(options);
        Client.Transport.Delay = (wait, _) =>
        {
            Waits.Add(wait);
            return Task.CompletedTask;
        };
    }

    public StubHandler Handler { get; }

    public FoPostClient Client { get; }

    public List<TimeSpan> Waits { get; } = new();

    public void Dispose() => Client.Dispose();
}

internal static class Fixtures
{
    public const string Post = """
    {
      "id": "post_1",
      "workspace_id": "ws_1",
      "status": "draft",
      "content_type": "post",
      "schedule_at": null,
      "repeatable": false,
      "title": null,
      "summary": null,
      "content": [{ "id": 1, "text": "Hello from .NET", "media": [], "position": 0 }],
      "accounts": [
        {
          "id": "acc_1",
          "platform": "twitter",
          "username": "fopost",
          "name": "FoPost",
          "publish_status": "pending",
          "attempts": 0,
          "max_attempts": 3
        }
      ],
      "labels": [],
      "settings": {},
      "created_at": "2026-08-12T10:00:00.000Z",
      "updated_at": "2026-08-12T10:00:00.000Z"
    }
    """;

    // The accounts endpoint answers camelCase where posts answer snake_case.
    public const string Account = """
    {
      "id": "acc_1",
      "workspaceId": "ws_1",
      "platform": "twitter",
      "username": "fopost",
      "name": "FoPost",
      "avatar": null,
      "isPrimary": true,
      "active": true,
      "healthStatus": "healthy",
      "lastHealthCheck": "2026-08-12T09:00:00.000Z"
    }
    """;
}
