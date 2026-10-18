using System.Net;
using Xunit;

namespace FoPost.Tests;

/// <summary>X communities on an account: list, sync, search, add by id, remove.</summary>
public class CommunitiesTests
{
    private const string Community = """
    {"id":7,"account_id":"acc_1","community_id":"1234567890","name":"Builders","member_count":120,"description":null,"image_url":null,"last_synced_at":"2026-09-01T00:00:00Z","created_at":"2026-09-01T00:00:00Z"}
    """;

    [Fact]
    public async Task List_sync_search_add_and_remove_hit_the_community_paths()
    {
        var handler = new StubHandler()
            .Json($$"""{"data":[{{Community}}]}""")
            .Json($$"""{"data":[{{Community}}],"note":"X returned one community"}""")
            .Json("""{"data":[{"id":"1234567890","name":"Builders","description":"People who build","member_count":120}]}""")
            .Json(Community)
            .Enqueue(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var test = new TestClient(handler);

        var stored = await test.Client.Accounts.ListCommunitiesAsync("acc_1");
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/acc_1/communities", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(7, Assert.Single(stored).Id);

        var sync = await test.Client.Accounts.SyncCommunitiesAsync("acc_1");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/acc_1/communities/sync", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("X returned one community", sync.Note);
        Assert.Equal("1234567890", Assert.Single(sync.Communities).CommunityId);

        var found = await test.Client.Accounts.SearchCommunitiesAsync("acc_1", "build");
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/acc_1/communities/search?q=build", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(120, Assert.Single(found).MemberCount);

        var added = await test.Client.Accounts.AddCommunityAsync("acc_1", "1234567890", "Builders");
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/acc_1/communities/manual", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("""{"communityId":"1234567890","name":"Builders"}""", handler.LastBody);
        Assert.Equal("Builders", added.Name);

        await test.Client.Accounts.RemoveCommunityAsync("acc_1", 7);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/accounts/acc_1/communities/7", handler.LastRequest.RequestUri!.ToString());
    }
}
