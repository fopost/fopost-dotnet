using System.Net;
using Xunit;

namespace FoPost.Tests;

/// <summary>
/// Business Profile management: one call per route, pinning the URL, the verb
/// and the body each endpoint actually receives.
/// </summary>
public class GoogleBusinessTests
{
    private const string Ok = """{"data":{"ok":true}}""";
    private static string Base => $"{TestClient.BaseUrl}/v1/accounts/a1/gbp";

    [Fact]
    public async Task Every_method_maps_onto_its_route()
    {
        var calls = new (HttpMethod Method, string Url, Func<FoPostClient, Task>)[]
        {
            (HttpMethod.Get, $"{Base}/location", c => c.GoogleBusiness.GetLocationAsync("a1")),
            (HttpMethod.Patch, $"{Base}/location",
                c => c.GoogleBusiness.UpdateLocationAsync("a1", new Dictionary<string, object?>())),
            (HttpMethod.Get, $"{Base}/attributes", c => c.GoogleBusiness.GetAttributesAsync("a1")),
            (HttpMethod.Patch, $"{Base}/attributes",
                c => c.GoogleBusiness.UpdateAttributesAsync("a1", Array.Empty<IReadOnlyDictionary<string, object?>>())),
            (HttpMethod.Get, $"{Base}/menus", c => c.GoogleBusiness.GetMenusAsync("a1")),
            (HttpMethod.Put, $"{Base}/menus", c => c.GoogleBusiness.ReplaceMenusAsync("a1", Array.Empty<object>())),
            (HttpMethod.Get, $"{Base}/services", c => c.GoogleBusiness.GetServicesAsync("a1")),
            (HttpMethod.Put, $"{Base}/services",
                c => c.GoogleBusiness.ReplaceServicesAsync("a1", Array.Empty<object>())),
            (HttpMethod.Get, $"{Base}/media", c => c.GoogleBusiness.ListMediaAsync("a1")),
            (HttpMethod.Post, $"{Base}/media", c => c.GoogleBusiness.AddMediaAsync("a1", "m1")),
            (HttpMethod.Delete, $"{Base}/media/CAoSL", c => c.GoogleBusiness.DeleteMediaAsync("a1", "CAoSL")),
            (HttpMethod.Get, $"{Base}/place-actions", c => c.GoogleBusiness.ListPlaceActionsAsync("a1")),
            (HttpMethod.Post, $"{Base}/place-actions",
                c => c.GoogleBusiness.CreatePlaceActionAsync("a1", "https://example.test/book", "APPOINTMENT")),
            (HttpMethod.Patch, $"{Base}/place-actions/links-1",
                c => c.GoogleBusiness.UpdatePlaceActionAsync("a1", "links-1", isPreferred: true)),
            (HttpMethod.Delete, $"{Base}/place-actions/links-1",
                c => c.GoogleBusiness.DeletePlaceActionAsync("a1", "links-1")),
            (HttpMethod.Get, $"{Base}/verification", c => c.GoogleBusiness.GetVerificationOptionsAsync("a1")),
            (HttpMethod.Post, $"{Base}/verification/start",
                c => c.GoogleBusiness.StartVerificationAsync("a1", "SMS")),
            (HttpMethod.Post, $"{Base}/verification/complete",
                c => c.GoogleBusiness.CompleteVerificationAsync("a1", "v1", "123456")),
            (HttpMethod.Post, $"{Base}/assign", c => c.GoogleBusiness.AssignAsync("a1", "w2")),
        };

        foreach (var (method, url, call) in calls)
        {
            var handler = new StubHandler().Json(Ok);
            using var test = new TestClient(handler);

            await call(test.Client);

            Assert.Equal(method, handler.LastRequest.Method);
            Assert.Equal(url, handler.LastRequest.RequestUri!.ToString());
        }
    }

    [Fact]
    public async Task A_patch_carries_only_the_fields_the_caller_set()
    {
        var handler = new StubHandler().Json("""{"data":{}}""");
        using var test = new TestClient(handler);

        await test.Client.GoogleBusiness.UpdateLocationAsync(
            "a1",
            new Dictionary<string, object?> { ["store_code"] = "S-12" });

        Assert.Equal("""{"store_code":"S-12"}""", handler.LastBody);
    }

    [Fact]
    public async Task A_photo_is_named_by_its_library_id()
    {
        var handler = new StubHandler().Json("""{"data":{}}""");
        using var test = new TestClient(handler);

        await test.Client.GoogleBusiness.AddMediaAsync("a1", "m1", "INTERIOR");

        Assert.Equal("""{"media_id":"m1","category":"INTERIOR"}""", handler.LastBody);
    }

    [Fact]
    public async Task Performance_repeats_the_metric_parameter()
    {
        var handler = new StubHandler().Json("""{"data":{}}""");
        using var test = new TestClient(handler);

        await test.Client.GoogleBusiness.GetPerformanceAsync(
            "a1", "2026-09-01", "2026-09-07", new[] { "CALL_CLICKS", "WEBSITE_CLICKS" });

        Assert.Equal(
            $"{Base}/performance?start_date=2026-09-01&end_date=2026-09-07"
                + "&daily_metrics=CALL_CLICKS&daily_metrics=WEBSITE_CLICKS",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Search_keywords_asks_the_same_route_for_the_monthly_terms()
    {
        var handler = new StubHandler().Json("""{"data":{}}""");
        using var test = new TestClient(handler);

        await test.Client.GoogleBusiness.GetSearchKeywordsAsync("a1", "2026-08-01", "2026-09-01");

        Assert.Contains("keywords=true", handler.LastRequest.RequestUri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_pending_api_grant_surfaces_as_an_error()
    {
        var handler = new StubHandler().Json(
            """{"error":"configuration_error","message":"Not available yet"}""",
            HttpStatusCode.ServiceUnavailable);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostException>(
            () => test.Client.GoogleBusiness.GetLocationAsync("a1"));

        Assert.Equal(503, error.Status);
        Assert.Equal("configuration_error", error.Code);
    }
}
