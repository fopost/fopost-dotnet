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
            .Json("""{"data":{"url":"https://pinterest.example/login"}}""")
            .Json("""{"data":[{"id":"conn_1","provider":"meta","authType":"business","name":"Your Brand","businessId":"77","createdAt":"2026-09-01T00:00:00.000Z","workspaceId":"ws_1"}]}""")
            .Json("""{"message":"Connection removed"}""");
        using var test = new TestClient(handler);

        var url = await test.Client.Ads.AuthorizeAsync(new AuthorizeAdsOptions { WorkspaceId = "ws_1", ReturnTo = "/ads" });
        Assert.Equal("/v1/ads/connections/meta/authorize", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"workspaceId":"ws_1","returnTo":"/ads"}""", handler.LastBody);
        Assert.Equal("https://ads.example/login?state=abc", url);

        // The provider names the path, so a connection is not Meta-only.
        await test.Client.Ads.AuthorizeAsync(new AuthorizeAdsOptions { WorkspaceId = "ws_1", Provider = "pinterest" });
        Assert.Equal("/v1/ads/connections/pinterest/authorize", handler.LastRequest.RequestUri!.AbsolutePath);

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
            .Json("""{"data":{"leads":[{"id":"lead_1","createdAt":null,"fields":[{"name":"full_name","values":["Sam Lee"]}],"adName":null,"campaignName":null,"platform":"fb","isOrganic":true}],"nextCursor":"c2"}}""");
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
        Assert.Equal("full_name", Assert.Single(Assert.Single(leads.Leads).Fields).Name);
    }

    [Fact]
    public async Task Account_tree_nests_ad_sets_and_ads_under_campaigns()
    {
        var handler = new StubHandler().Json("""{"data":{"adAccountId":"act_123","currency":"USD","workspaceId":"ws_1","campaigns":[{"id":"c_1","name":"Launch","status":"ACTIVE","effectiveStatus":"ACTIVE","objective":"OUTCOME_TRAFFIC","budgetMinor":null,"budgetType":null,"createdAt":null,"adSets":[{"id":"s_1","name":"US","campaignId":"c_1","status":"PAUSED","budgetMinor":2000,"budgetType":"daily","ads":[{"id":"a_1","name":"Morning","adSetId":"s_1","creativeId":"cr_1","status":"PAUSED"}]}]}]}}""");
        using var test = new TestClient(handler);

        var tree = await test.Client.Ads.AccountTreeAsync("act_123", "conn_1", "ws_1");

        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/ads/accounts/act_123/tree?workspace_id=ws_1&connection_id=conn_1",
            handler.LastRequest.RequestUri!.ToString());
        var campaign = Assert.Single(tree.Campaigns);
        Assert.Null(campaign.BudgetMinor);
        var adSet = Assert.Single(campaign.AdSets!);
        Assert.Equal(2000, adSet.BudgetMinor);
        Assert.Equal("cr_1", Assert.Single(adSet.Ads!).CreativeId);
    }

    [Fact]
    public async Task Campaign_writes_carry_workspace_and_connection_in_the_query()
    {
        var campaign = """{"id":"c_1","name":"Launch","status":"PAUSED","effectiveStatus":"PAUSED","objective":"OUTCOME_TRAFFIC","budgetMinor":null,"budgetType":null,"createdAt":null}""";
        var handler = new StubHandler()
            .Json($$"""{"data":{{campaign}}}""", System.Net.HttpStatusCode.Created)
            .Json($$"""{"data":{{campaign}}}""")
            .Json("""{"data":{"id":"c_2"}}""", System.Net.HttpStatusCode.Created)
            .Json("""{"message":"Campaign deleted"}""");
        using var test = new TestClient(handler);

        await test.Client.Ads.CreateCampaignAsync(new CreateAdCampaignOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "act_123",
            Name = "Launch",
            Goal = AdGoals.Traffic,
        });
        Assert.Equal("/v1/ads/campaigns", handler.LastRequest.RequestUri!.AbsolutePath);
        var created = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("conn_1", created.GetProperty("connectionId").GetString());
        Assert.Equal("traffic", created.GetProperty("goal").GetString());
        Assert.False(created.TryGetProperty("paused", out _));

        await test.Client.Ads.UpdateCampaignAsync("c_1", "ws_1", "conn_1", new UpdateAdCampaignOptions { Status = AdStatuses.Active });
        Assert.Equal(HttpMethod.Patch, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/ads/campaigns/c_1?workspace_id=ws_1&connection_id=conn_1", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"status":"active"}""", handler.LastBody);

        var copy = await test.Client.Ads.DuplicateCampaignAsync("c_1", "ws_1", "conn_1", paused: false);
        Assert.Equal("/v1/ads/campaigns/c_1/duplicate", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal("""{"paused":false}""", handler.LastBody);
        Assert.Equal("c_2", copy);

        await test.Client.Ads.DeleteCampaignAsync("c_1", "ws_1", "conn_1");
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/ads/campaigns/c_1?workspace_id=ws_1&connection_id=conn_1", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Bulk_status_sends_each_object_with_its_level()
    {
        var handler = new StubHandler().Json("""{"data":[{"id":"c_1","level":"campaign","ok":true,"error":null},{"id":"a_1","level":"ad","ok":false,"error":"Not found"}]}""");
        using var test = new TestClient(handler);

        var results = await test.Client.Ads.BulkSetStatusAsync(new BulkAdStatusOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            Status = AdStatuses.Paused,
            Objects = new List<AdObjectRef> { new("c_1", AdObjectLevels.Campaign), new("a_1", AdObjectLevels.Ad) },
        });

        Assert.Equal("/v1/ads/status", handler.LastRequest.RequestUri!.AbsolutePath);
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("ad", body.GetProperty("objects")[1].GetProperty("level").GetString());
        Assert.False(results[1].Ok);
        Assert.Equal("Not found", results[1].Error);
    }

    [Fact]
    public async Task Insights_send_the_range_breakdown_and_daily_flag()
    {
        var report = """{"data":{"objectId":"c_1","currency":"USD","since":"2026-09-01","until":"2026-09-07","breakdownBy":"age","totals":{"impressions":1000,"reach":800,"clicks":25,"spendMinor":1200,"ctr":2.5,"leads":3},"breakdown":[{"key":"25-34","metrics":{"impressions":600,"reach":500,"clicks":15,"spendMinor":700,"ctr":2.5,"leads":2}}],"timeline":[{"date":"2026-09-01","metrics":{"impressions":100,"reach":90,"clicks":2,"spendMinor":150,"ctr":2,"leads":0}}]}}""";
        var handler = new StubHandler().Json(report).Json(report);
        using var test = new TestClient(handler);

        var insights = await test.Client.Ads.InsightsAsync(
            "conn_1", "c_1", "2026-09-01", "2026-09-07", AdInsightsBreakdowns.Age, daily: true, workspaceId: "ws_1");
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/ads/insights?workspace_id=ws_1&connection_id=conn_1&object_id=c_1&since=2026-09-01&until=2026-09-07&breakdown=age&daily=true",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(2.5, insights.Totals!.Ctr);
        Assert.Equal("25-34", Assert.Single(insights.Breakdown).Key);
        Assert.Equal(150, Assert.Single(insights.Timeline).Metrics.SpendMinor);

        await test.Client.Ads.AdInsightsAsync("ad_1", "ws_1", "2026-09-01", "2026-09-07");
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/ads/ad_1/insights?workspace_id=ws_1&since=2026-09-01&until=2026-09-07",
            handler.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Leads_feed_passes_the_cursor_back()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"leads":[{"id":"l_1","leadId":"m_1","connectionId":"conn_1","pageId":"1234","formId":"form_1","adId":null,"adName":null,"campaignName":null,"platform":"fb","isOrganic":false,"fields":[{"name":"email","values":["sam@yourbrand.com"]}],"submittedAt":"2026-09-10T12:00:00.000Z","workspaceId":"ws_1"}],"nextCursor":"cur_2"}}""")
            .Json("""{"data":{"leads":[],"nextCursor":null}}""");
        using var test = new TestClient(handler);

        var first = await test.Client.Ads.LeadsFeedAsync("ws_1", formId: "form_1", limit: 50);
        Assert.Equal(
            $"{TestClient.BaseUrl}/v1/ads/leads?workspace_id=ws_1&form_id=form_1&limit=50",
            handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("m_1", Assert.Single(first.Leads).LeadId);
        Assert.Equal("cur_2", first.NextCursor);

        var second = await test.Client.Ads.LeadsFeedAsync("ws_1", formId: "form_1", cursor: first.NextCursor, limit: 50);
        Assert.Contains("cursor=cur_2", handler.LastRequest.RequestUri!.Query, StringComparison.Ordinal);
        Assert.Empty(second.Leads);
        Assert.Null(second.NextCursor);
    }

    [Fact]
    public async Task Lead_pages_audience_users_and_creatives()
    {
        var handler = new StubHandler()
            .Json("""{"data":{"pageId":"1234","backfilled":7}}""", System.Net.HttpStatusCode.Created)
            .Json("""{"message":"Unsubscribed"}""")
            .Json("""{"data":{"added":2}}""")
            .Json("""{"data":{"id":"cr_1","name":"Carousel","format":"carousel","status":"ACTIVE","title":null,"body":"Hi","link":null,"thumbnailUrl":null,"callToAction":"SHOP_NOW","urlTags":"utm_source=meta"}}""", System.Net.HttpStatusCode.Created);
        using var test = new TestClient(handler);

        var page = await test.Client.Ads.SubscribeLeadPageAsync("ws_1", "conn_1", "1234");
        Assert.Equal("""{"workspaceId":"ws_1","connectionId":"conn_1","pageId":"1234"}""", handler.LastBody);
        Assert.Equal(7, page.Backfilled);

        await test.Client.Ads.UnsubscribeLeadPageAsync("1234", "ws_1", "conn_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/ads/lead-pages/1234?workspace_id=ws_1&connection_id=conn_1", handler.LastRequest.RequestUri!.ToString());

        var added = await test.Client.Ads.AddAudienceUsersAsync("aud_1", "ws_1", "conn_1", new[] { "a@yourbrand.com", "b@yourbrand.com" });
        Assert.Equal("/v1/ads/audiences/aud_1/users", handler.LastRequest.RequestUri!.AbsolutePath);
        Assert.Equal(2, added);

        var creative = await test.Client.Ads.CreateCreativeAsync(new CreateAdCreativeOptions
        {
            WorkspaceId = "ws_1",
            ConnectionId = "conn_1",
            AdAccountId = "act_123",
            PageId = "1234",
            Name = "Carousel",
            Format = AdCreativeFormats.Carousel,
            Text = "Hi",
            UrlTags = "utm_source=meta",
            Cards = new List<AdCreativeCard> { new() { MediaUrl = "https://cdn.yourbrand.com/1.png" }, new() { MediaUrl = "https://cdn.yourbrand.com/2.png" } },
        });
        var body = JsonDocument.Parse(handler.LastBody!).RootElement;
        Assert.Equal("utm_source=meta", body.GetProperty("urlTags").GetString());
        Assert.Equal(2, body.GetProperty("cards").GetArrayLength());
        Assert.Equal("SHOP_NOW", creative.CallToAction);
    }
}
