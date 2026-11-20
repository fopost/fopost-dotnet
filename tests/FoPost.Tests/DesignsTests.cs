using System.Net;
using System.Text.Json.Nodes;
using Xunit;

namespace FoPost.Tests;

/// <summary>Designs: CRUD, versions, duplicate, export, bulk, posts and the text tools.</summary>
public class DesignsTests
{
    private const string Design = """
    {"id":"des_1","name":"Launch card","kind":"post","width":1080,"height":1080,"document":{"pages":[{"layers":[]}]},"thumbnail_media_id":null,"thumbnail_url":null,"created_at":"2026-11-01T00:00:00Z","updated_at":"2026-11-02T00:00:00Z"}
    """;

    private const string Version = """
    {"id":"ver_1","version":2,"created_at":"2026-11-02T00:00:00Z"}
    """;

    [Fact]
    public async Task List_sends_the_workspace_and_filters()
    {
        var handler = new StubHandler().Json($$"""{"data":[{{Design}}]}""");
        using var test = new TestClient(handler);

        var designs = await test.Client.Designs.ListAsync("ws_1", new ListDesignsOptions { Kind = DesignKinds.Post, Limit = 10, Offset = 20 });

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/designs?workspace_id=ws_1&kind=post&limit=10&offset=20",
            handler.LastRequest.RequestUri!.ToString());
        var design = Assert.Single(designs);
        Assert.Equal(1080, design.Width);
        Assert.Null(design.ThumbnailUrl);
    }

    [Fact]
    public async Task Create_get_and_delete()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":{{Design}}}""", HttpStatusCode.Created)
            .Json($$"""{"data":{{Design}}}""")
            .Json("""{"data":{"id":"des_1","deleted":true}}""");
        using var test = new TestClient(handler);

        var created = await test.Client.Designs.CreateAsync(new CreateDesignOptions
        {
            WorkspaceId = "ws_1",
            Kind = DesignKinds.Post,
            Name = "Launch card",
            Document = new Dictionary<string, object?> { ["pages"] = new[] { new Dictionary<string, object?> { ["layers"] = Array.Empty<object>() } } },
        });
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/designs", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(
            """{"workspace_id":"ws_1","kind":"post","name":"Launch card","document":{"pages":[{"layers":[]}]}}""",
            handler.LastBody);
        Assert.Equal("des_1", created.Id);
        Assert.Equal(1, created.Document!.Value.GetProperty("pages").GetArrayLength());

        var fetched = await test.Client.Designs.GetAsync("des_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/designs/des_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Launch card", fetched.Name);

        var deleted = await test.Client.Designs.DeleteAsync("des_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.True(deleted.Deleted);
    }

    [Fact]
    public async Task Update_patches_only_the_set_fields_and_reads_the_version()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":{{Design}},"version":{{Version}}}""")
            .Json($$"""{"data":{{Design}},"version":null}""");
        using var test = new TestClient(handler);

        var document = JsonNode.Parse("""{"pages":[]}""");
        var change = await test.Client.Designs.UpdateAsync("des_1", new UpdateDesignOptions { Document = document, CreateVersion = true });
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal("/v1/designs/des_1", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"document":{"pages":[]},"create_version":true}""", handler.LastBody);
        Assert.Equal("des_1", change.Design.Id);
        Assert.Equal(2, change.Version!.Version);

        var renamed = await test.Client.Designs.UpdateAsync("des_1", new UpdateDesignOptions { Name = "Renamed" });
        Assert.Equal("""{"name":"Renamed"}""", handler.LastBody);
        Assert.Null(renamed.Version);
    }

    [Fact]
    public async Task Versions_list_and_restore()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":[{{Version}}]}""")
            .Json($$"""{"data":{{Design}},"version":{{Version}}}""");
        using var test = new TestClient(handler);

        var versions = await test.Client.Designs.ListVersionsAsync("des_1", includeDocument: true);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/designs/des_1/versions?include=document",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("ver_1", Assert.Single(versions).Id);

        var restored = await test.Client.Designs.RestoreVersionAsync("des_1", "ver_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/designs/des_1/versions/ver_1/restore", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Null(handler.LastBody);
        Assert.Equal(2, restored.Version!.Version);
    }

    [Fact]
    public async Task Duplicate_export_bulk_and_posts()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":{{Design}}}""", HttpStatusCode.Created)
            .Json("""{"data":{"format":"png","files":[{"media_id":"m_1","page_index":0,"mime_type":"image/png","preview_url":"/v1/media/m_1/file"}]}}""")
            .Json($$"""{"data":[{{Design}},{{Design}}]}""", HttpStatusCode.Created)
            .Json("""{"data":[{"id":"post_1","status":"published","title":null,"text":"Hi","schedule_at":null,"published_at":"2026-11-03T00:00:00Z","platforms":["twitter"],"engagement":12,"views":340}]}""");
        using var test = new TestClient(handler);

        await test.Client.Designs.DuplicateAsync("des_1", "Copy");
        Assert.Equal("/v1/designs/des_1/duplicate", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"name":"Copy"}""", handler.LastBody);

        var export = await test.Client.Designs.ExportAsync("des_1", new ExportDesignOptions
        {
            Format = DesignExportFormats.Mp4,
            PageIndexes = new List<int> { 0, 2 },
            DurationSec = 10,
        });
        Assert.Equal("/v1/designs/des_1/export", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"format":"mp4","page_indexes":[0,2],"duration_sec":10}""", handler.LastBody);
        var file = Assert.Single(export.Files);
        Assert.Equal("m_1", file.MediaId);
        Assert.Equal(0, file.PageIndex);

        var copies = await test.Client.Designs.BulkCreateAsync(
            "des_1",
            new[] { new Dictionary<string, string> { ["title"] = "Monday" } },
            nameColumn: "title");
        Assert.Equal("/v1/designs/des_1/bulk", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"rows":[{"title":"Monday"}],"name_column":"title"}""", handler.LastBody);
        Assert.Equal(2, copies.Count);

        var posts = await test.Client.Designs.ListPostsAsync("des_1");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("/v1/designs/des_1/posts", handler.LastRequest.RequestUri!.AbsolutePath);
        var post = Assert.Single(posts);
        Assert.Equal(340, post.Views);
        Assert.Equal("twitter", Assert.Single(post.Platforms));
    }

    [Fact]
    public async Task Text_to_slides_and_from_post_send_snake_case_bodies()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"cover":{"title":"Ship faster","subtitle":"Five habits"},"slides":[{"heading":"One","body":"Plan"}],"cta":{"heading":"Follow","body":"More soon"},"brand":{"colors":["#000000"],"fonts":{"heading":"Geist"},"logo_media_id":null}}}""")
            .Json($$"""{"data":{{Design}}}""", HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var slides = await test.Client.Designs.TextToSlidesAsync(new TextToSlidesOptions
        {
            WorkspaceId = "ws_1",
            Text = "Long text",
            MaxSlides = 6,
        });
        Assert.Equal("/v1/designs/text-to-slides", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"workspace_id":"ws_1","text":"Long text","max_slides":6}""", handler.LastBody);
        Assert.Equal("Ship faster", slides.Cover!.Title);
        Assert.Equal("Plan", Assert.Single(slides.Slides).Body);
        Assert.Equal("Geist", slides.Brand!.Fonts!.Heading);

        var design = await test.Client.Designs.FromPostAsync(new DesignFromPostOptions
        {
            WorkspaceId = "ws_1",
            PostId = "post_1",
            Style = DesignFromPostStyles.Post,
            AccountId = "acc_1",
            Carousel = true,
        });
        Assert.Equal("/v1/designs/from-post", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(
            """{"workspace_id":"ws_1","post_id":"post_1","style":"post","account_id":"acc_1","carousel":true}""",
            handler.LastBody);
        Assert.Equal("des_1", design.Id);
    }
}
