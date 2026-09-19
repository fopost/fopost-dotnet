using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace FoPost.Tests;

public class AdsTests
{
    private const string Ad = """
    {
      "id": "ad_1",
      "workspaceId": "ws_1",
      "kind": "boost",
      "name": "Launch boost",
      "goal": "engagement",
      "status": "paused",
      "effectiveStatus": "PAUSED",
      "connectionId": "conn_1",
      "accountId": "acc_1",
      "platform": "facebook",
      "adAccountId": "act_123",
      "sourcePostId": "post_1",
      "budgetMinor": 2000,
      "budgetType": "daily",
      "currency": "USD",
      "endAt": null,
      "targeting": { "countries": ["US"], "ageMin": 18, "ageMax": 65, "gender": "all", "interests": [{ "id": "6003", "name": "Coffee" }] },
      "creative": null,
      "insights": { "impressions": 1200, "reach": 900, "clicks": 40, "spendMinor": 1550 },
      "insightsAt": "2026-09-02T00:00:00.000Z",
      "lastError": null,
      "createdAt": "2026-09-01T00:00:00.000Z"
    }
    """;

    [Fact]
    public async Task List_reads_ads_with_their_insights()
    {
        var handler = new StubHandler().Json($$"""{"data":[{{Ad}}]}""");
        using var test = new TestClient(handler);

        var ads = await test.Client.Ads.ListAsync("ws_1");

        Assert.Equal($"{TestClient.BaseUrl}/v1/ads?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        var ad = Assert.Single(ads);
        Assert.Equal("boost", ad.Kind);
        Assert.Equal(2000, ad.BudgetMinor);
        Assert.Equal("US", Assert.Single(ad.Targeting!.Countries));
        Assert.Equal("Coffee", Assert.Single(ad.Targeting.Interests!).Name);
        Assert.Equal(1550, ad.Insights!.SpendMinor);
    }

    [Fact]
    public async Task Boost_sends_a_camelCase_body_and_leaves_paused_to_the_api()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Ad}}}""", System.Net.HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var ad = await test.Client.Ads.BoostAsync(new BoostPostOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "act_123",
            PostId = "post_1",
            AccountId = "acc_1",
            Name = "Launch boost",
            Goal = AdGoals.Engagement,
            Budget = new AdBudget(2000, AdBudgetTypes.Daily),
            Targeting = new AdTargeting
            {
                Countries = new List<string> { "US" },
                Interests = new List<AdTargetingItem> { new("6003", "Coffee") },
            },
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal("/v1/ads/boost", handler.LastRequest.RequestUri!.AbsolutePath);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("ws_1", body.GetProperty("workspaceId").GetString());
        Assert.Equal("act_123", body.GetProperty("adAccountId").GetString());
        Assert.Equal("post_1", body.GetProperty("postId").GetString());
        Assert.Equal(2000, body.GetProperty("budget").GetProperty("minor").GetInt64());
        Assert.Equal("daily", body.GetProperty("budget").GetProperty("type").GetString());
        Assert.Equal(18, body.GetProperty("targeting").GetProperty("ageMin").GetInt32());
        Assert.Equal("Coffee", body.GetProperty("targeting").GetProperty("interests")[0].GetProperty("name").GetString());
        Assert.False(body.TryGetProperty("paused", out _));
        Assert.False(body.TryGetProperty("pageId", out _));
        Assert.Equal("ad_1", ad.Id);
    }

    [Fact]
    public async Task Create_sends_the_creative_and_an_explicit_paused_false()
    {
        var handler = new StubHandler().Json($$"""{"data":{{Ad}}}""", System.Net.HttpStatusCode.Created);
        using var test = new TestClient(handler);

        await test.Client.Ads.CreateAsync(new CreateAdOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "act_123",
            PageId = "1234",
            Text = "Fresh roast, every morning",
            Headline = "Your Brand Coffee",
            DestinationUrl = "https://yourbrand.com",
            Name = "Morning ad",
            Goal = AdGoals.Traffic,
            Budget = new AdBudget(50000, AdBudgetTypes.Lifetime, new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero)),
            Targeting = new AdTargeting { Countries = new List<string> { "US", "CA" }, AgeMin = 21, AgeMax = 45 },
            Paused = false,
        });

        Assert.Equal("/v1/ads", handler.LastRequest.RequestUri!.AbsolutePath);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("1234", body.GetProperty("pageId").GetString());
        Assert.Equal("Your Brand Coffee", body.GetProperty("headline").GetString());
        Assert.False(body.GetProperty("paused").GetBoolean());
        Assert.False(body.TryGetProperty("mediaUrl", out _));
        Assert.StartsWith("2026-10-01T00:00:00", body.GetProperty("budget").GetProperty("endAt").GetString(), StringComparison.Ordinal);
        Assert.Equal(2, body.GetProperty("targeting").GetProperty("countries").GetArrayLength());
    }

    [Fact]
    public async Task Status_refresh_and_delete_carry_the_workspace_in_the_query()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":{{Ad}}}""")
            .Json($$"""{"data":{{Ad}}}""")
            .Json("""{"message":"Ad deleted"}""");
        using var test = new TestClient(handler);

        await test.Client.Ads.SetStatusAsync("ad_1", "ws_1", AdStatuses.Active);
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/ads/ad_1?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"status":"active"}""", handler.LastBody);

        await test.Client.Ads.RefreshAsync("ad_1", "ws_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/ads/ad_1/refresh?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Null(handler.LastBody);

        await test.Client.Ads.DeleteAsync("ad_1", "ws_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/ads/ad_1?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Connections_authorize_and_delete()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"url":"https://ads.example/login?state=abc"}}""")
            .Json("""{"data":[{"id":"conn_1","provider":"meta","authType":"business","name":"Your Brand","businessId":"77","createdAt":"2026-09-01T00:00:00.000Z","workspaceId":"ws_1"}]}""")
            .Json("""{"message":"Connection removed"}""");
        using var test = new TestClient(handler);

        var url = await test.Client.Ads.AuthorizeMetaAsync(new AuthorizeMetaAdsOptions { WorkspaceId = "ws_1", ReturnTo = "/ads" });
        Assert.Equal("/v1/ads/connections/meta/authorize", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"workspaceId":"ws_1","returnTo":"/ads"}""", handler.LastBody);
        Assert.Equal("https://ads.example/login?state=abc", url);

        var connections = await test.Client.Ads.ConnectionsAsync("ws_1");
        Assert.Equal("business", Assert.Single(connections).AuthType);

        await test.Client.Ads.DeleteConnectionAsync("conn_1", "ws_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/ads/connections/conn_1?workspace_id=ws_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Audiences_read_and_create()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"audiences":[{"id":"aud_1","name":"Buyers","subtype":"CUSTOM","description":null,"sizeLower":1000,"sizeUpper":1500,"deliveryStatus":"ready","createdAt":"2026-08-01"}],"pixels":[{"id":"px_1","name":"Site"}],"workspaceId":"ws_1"}}""")
            .Json("""{"data":{"id":"aud_2","added":0}}""");
        using var test = new TestClient(handler);

        var result = await test.Client.Ads.AudiencesAsync("conn_1", "act_123", "ws_1");
        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Contains("connection_id=conn_1", query, StringComparison.Ordinal);
        Assert.Contains("ad_account_id=act_123", query, StringComparison.Ordinal);
        Assert.Equal("Buyers", Assert.Single(result.Audiences).Name);
        Assert.Equal("px_1", Assert.Single(result.Pixels).Id);

        var created = await test.Client.Ads.CreateAudienceAsync(new CreateAudienceOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "act_123",
            Name = "Lookalike buyers",
            Spec = AudienceSpec.Lookalike("aud_1", "US", 0.05),
        });

        Assert.Equal("/v1/ads/audiences", handler.LastRequest.RequestUri!.AbsolutePath);
        var spec = JsonDocument.Parse(handler.LastBody!).RootElement.GetProperty("spec");
        Assert.Equal("LOOKALIKE", spec.GetProperty("subtype").GetString());
        Assert.Equal("aud_1", spec.GetProperty("originAudienceId").GetString());
        Assert.Equal(0.05, spec.GetProperty("ratio").GetDouble());
        Assert.False(spec.TryGetProperty("emails", out _));
        Assert.Equal("aud_2", created.Id);
    }

    [Fact]
    public async Task Targeting_search_and_lead_forms()
    {
        var handler = new StubHandler()
            .Json("""{"data":[{"id":"6003","name":"Coffee","detail":"Interest"}]}""")
            .Json("""{"data":[{"connectionId":"conn_1","connectionName":"Your Brand","pageId":"1234","pageName":"Your Brand","forms":[{"id":"form_1","name":"Newsletter","status":"ACTIVE","leadsCount":12,"createdAt":null,"questions":["EMAIL"]}],"error":null,"workspaceId":"ws_1"}]}""")
            .Json("""{"data":{"id":"form_2"}}""")
            .Json("""{"data":{"leads":[{"id":"lead_1","createdAt":null,"fields":[{"name":"email","values":["sam@yourbrand.com"]}],"adName":null,"campaignName":null,"platform":"fb","isOrganic":true}],"nextCursor":"c2"}}""");
        using var test = new TestClient(handler);

        var options = await test.Client.Ads.SearchTargetingAsync("conn_1", TargetingSearchTypes.Interest, "coffee");
        var query = handler.LastRequest.RequestUri!.Query;
        Assert.Equal("/v1/ads/targeting/search", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("type=interest", query, StringComparison.Ordinal);
        Assert.Contains("q=coffee", query, StringComparison.Ordinal);
        Assert.DoesNotContain("workspace_id", query, StringComparison.Ordinal);
        Assert.Equal("Coffee", Assert.Single(options).Name);

        var sources = await test.Client.Ads.LeadFormsAsync("ws_1");
        Assert.Equal(12, Assert.Single(Assert.Single(sources).Forms).LeadsCount);

        var formId = await test.Client.Ads.CreateLeadFormAsync(new CreateLeadFormOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            PageId = "1234",
            Name = "Newsletter",
            Questions = new List<string> { LeadFormQuestions.Email },
            PrivacyPolicyUrl = "https://yourbrand.com/privacy",
            ThankYouMessage = "Thanks!",
        });
        Assert.Equal("form_2", formId);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("EMAIL", body.GetProperty("questions")[0].GetString());
        Assert.False(body.TryGetProperty("followUpUrl", out _));

        var leads = await test.Client.Ads.LeadsAsync("form_1", "conn_1", "1234", after: "c1");
        Assert.Equal("/v1/ads/lead-forms/form_1/leads", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Contains("page_id=1234", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        Assert.Contains("after=c1", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        Assert.Equal("c2", leads.NextCursor);
        Assert.Equal("email", Assert.Single(Assert.Single(leads.Leads).Fields).Name);
    }
}
