using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

/// <summary>
/// The knowledge base: the path, the query casing, the snake_case request body,
/// and that a camelCase response reads back into the model.
/// </summary>
public class KnowledgeTests
{
    private const string Source = """
    {
      "id": "know_1",
      "kind": "url",
      "title": "Refund policy",
      "status": "ready",
      "statusMessage": null,
      "url": "https://yourbrand.com/help/refunds",
      "mediaId": null,
      "brandVoiceId": null,
      "chunkCount": 3,
      "content": null,
      "lastSyncedAt": "2026-09-20T00:00:00.000Z",
      "createdAt": "2026-09-19T00:00:00.000Z",
      "updatedAt": "2026-09-20T00:00:00.000Z"
    }
    """;

    [Fact]
    public async Task List_sends_the_workspace_filter_and_reads_camel_case_fields()
    {
        var handler = new StubHandler().Json($$"""{ "data": [{{Source}}] }""");
        using var test = new TestClient(handler);

        var sources = await test.Client.Knowledge.ListAsync("ws_1");

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("/v1/knowledge/sources", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("workspace_id=ws_1", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);

        var source = Assert.Single(sources);
        Assert.Equal("ready", source.Status);
        Assert.Equal(3, source.ChunkCount);
        Assert.Null(source.StatusMessage);
    }

    [Fact]
    public async Task Create_sends_a_snake_case_body_and_omits_what_the_kind_does_not_use()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{Source}} }""");
        using var test = new TestClient(handler);

        await test.Client.Knowledge.CreateAsync(new CreateKnowledgeSourceOptions
        {
            Kind = KnowledgeSourceKinds.File,
            Title = "Price list",
            MediaId = "media_1",
            WorkspaceId = "ws_1",
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/knowledge/sources", handler.LastRequest.RequestUri!.AbsolutePath);

        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("file", body.GetProperty("kind").GetString());
        Assert.Equal("media_1", body.GetProperty("media_id").GetString());
        Assert.Equal("ws_1", body.GetProperty("workspace_id").GetString());
        // Nothing the kind does not use reaches the wire.
        Assert.False(body.TryGetProperty("url", out _));
        Assert.False(body.TryGetProperty("content", out _));
    }

    [Fact]
    public async Task Update_patches_only_the_fields_that_were_set()
    {
        var handler = new StubHandler().Json($$"""{ "data": {{Source}} }""");
        using var test = new TestClient(handler);

        await test.Client.Knowledge.UpdateAsync("know_1", new UpdateKnowledgeSourceOptions
        {
            Title = Optional<string>.Of("Refunds"),
        });

        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("/v1/knowledge/sources/know_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"title":"Refunds"}""", handler.LastBody);
    }

    [Fact]
    public async Task Search_sends_top_k_and_reads_the_matches()
    {
        var handler = new StubHandler().Json("""
        { "data": [{
          "sourceId": "know_1",
          "sourceTitle": "Refund policy",
          "sourceKind": "url",
          "sourceUrl": "https://yourbrand.com/help/refunds",
          "text": "We refund within 30 days.",
          "score": 0.82
        }] }
        """);
        using var test = new TestClient(handler);

        var matches = await test.Client.Knowledge.SearchAsync(
            "how long do refunds take?",
            new SearchKnowledgeOptions { TopK = 3 });

        Assert.Equal("/v1/knowledge/search", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("top_k=3", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);

        var match = Assert.Single(matches);
        Assert.Equal("Refund policy", match.SourceTitle);
        Assert.Equal(0.82, match.Score, 4);
    }

    [Fact]
    public async Task Sync_posts_to_the_sources_sync_path()
    {
        var handler = new StubHandler().Json("""{ "data": { "id": "know_1", "status": "pending" } }""");
        using var test = new TestClient(handler);

        var queued = await test.Client.Knowledge.SyncAsync("know_1");

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/knowledge/sources/know_1/sync", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("pending", queued.Status);
    }
}
