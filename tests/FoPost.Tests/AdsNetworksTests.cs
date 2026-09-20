using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace FoPost.Tests;

/// <summary>A second ad network behind the same endpoints.</summary>
public class AdsNetworksTests
{
    [Fact]
    public async Task Authorize_reaches_whichever_network_the_registry_named()
    {
        var handler = new StubHandler().Json("""{"data":{"url":"https://www.linkedin.com/oauth"}}""");
        using var test = new TestClient(handler);

        var url = await test.Client.Ads.AuthorizeAsync(
            "linkedin",
            new AuthorizeAdsOptions { WorkspaceId = "ws_1", ReturnTo = "/ads" });

        Assert.Equal("/v1/ads/connections/linkedin/authorize", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"workspaceId":"ws_1","returnTo":"/ads"}""", handler.LastBody);
        Assert.Equal("https://www.linkedin.com/oauth", url);
    }

    [Fact]
    public async Task Providers_carry_what_each_network_supports()
    {
        var handler = new StubHandler().Json(
            """{"data":[{"id":"linkedin","name":"LinkedIn Ads","configured":false,"connectMethods":[],"capabilities":{"conversions":true},"targetingFacets":["country","job_title"],"trackingMacros":[{"token":"{{LINKEDIN_CAMPAIGN_ID}}","description":"Campaign"}]}]}""");
        using var test = new TestClient(handler);

        var providers = await test.Client.Ads.ProvidersAsync();

        var provider = Assert.Single(providers);
        Assert.False(provider.Configured);
        Assert.True(provider.Capabilities["conversions"]);
        Assert.Equal(new[] { "country", "job_title" }, provider.TargetingFacets);
        Assert.Equal("{{LINKEDIN_CAMPAIGN_ID}}", Assert.Single(provider.TrackingMacros).Token);
    }

    [Fact]
    public async Task Company_rows_travel_with_the_request()
    {
        var handler = new StubHandler().Json("""{"data":{"added":2}}""");
        using var test = new TestClient(handler);

        var added = await test.Client.Ads.AddAudienceCompaniesAsync(
            "urn:li:adSegment:44",
            "ws_1",
            "conn_1",
            new List<AdCompanyOptions>
            {
                new() { Domain = "northwind.example" },
                new() { Name = "Contoso" },
            });

        Assert.Equal(2, added);
        Assert.Equal(
            """{"companies":[{"domain":"northwind.example"},{"name":"Contoso"}]}""",
            handler.LastBody);
    }

    [Fact]
    public async Task Conversion_events_send_the_identity_the_api_hashes()
    {
        var handler = new StubHandler().Json("""{"data":{"accepted":1}}""");
        using var test = new TestClient(handler);

        var accepted = await test.Client.Ads.SendConversionEventsAsync(
            "urn:li:conversion:9",
            "ws_1",
            "conn_1",
            new List<ConversionEventOptions>
            {
                new() { HappenedAt = 1758326400000, Email = "buyer@example.test" },
            });

        Assert.Equal(1, accepted);
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Contains(
            "/v1/ads/linkedin/conversion-rules/urn%3Ali%3Aconversion%3A9/events",
            handler.LastRequest.RequestUri!.ToString());
    }
}
