using System.Text.Json;
using System.Text.Json.Nodes;
using FoPost.Http;
using Xunit;

namespace FoPost.Tests;

public class ModelTests
{
    [Fact]
    public void Snake_case_payloads_bind()
    {
        var post = JsonNode.Parse(Fixtures.Post).Deserialize<Post>(FoPostJson.Options)!;

        Assert.Equal("ws_1", post.WorkspaceId);
        Assert.Equal("Hello from .NET", post.Content[0].Text);
        Assert.Equal("pending", post.Accounts[0].PublishStatus);
        Assert.Equal(3, post.Accounts[0].MaxAttempts);
    }

    [Fact]
    public void CamelCase_payloads_bind_to_the_same_members()
    {
        var account = JsonNode.Parse(Fixtures.Account).Deserialize<SocialAccount>(FoPostJson.Options)!;

        Assert.Equal("ws_1", account.WorkspaceId);
        Assert.True(account.IsPrimary);
        Assert.Equal("healthy", account.HealthStatus);
        Assert.Equal(
            new DateTimeOffset(2026, 8, 12, 9, 0, 0, TimeSpan.Zero),
            account.LastHealthCheck);
    }

    [Fact]
    public void Unknown_fields_are_kept_rather_than_dropped()
    {
        var account = JsonNode.Parse("""{"id":"acc_1","platform":"twitter","futureField":42}""")
            .Deserialize<SocialAccount>(FoPostJson.Options)!;

        Assert.NotNull(account.AdditionalData);
        Assert.Equal(42, account.AdditionalData!["futureField"].GetInt32());
    }

    [Fact]
    public void An_alias_never_writes_a_duplicate_key_back_out()
    {
        var account = new SocialAccount { Id = "acc_1", WorkspaceId = "ws_1", Platform = "twitter" };

        var json = JsonSerializer.Serialize(account, FoPostJson.Options);

        Assert.Contains("\"workspace_id\":\"ws_1\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("workspaceId", json, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("workspace_id", "workspaceId")]
    [InlineData("last_health_check", "lastHealthCheck")]
    [InlineData("id", "id")]
    [InlineData("from", "from")]
    public void Snake_case_names_camelize(string given, string expected) =>
        Assert.Equal(expected, FoPostJson.ToCamelCase(given));

    [Fact]
    public void Optional_separates_unset_from_an_explicit_null()
    {
        Optional<string?> unset = default;
        Optional<string?> cleared = Optional<string?>.Of(null);
        Optional<string?> set = "hello";

        Assert.False(unset.IsSet);
        Assert.True(cleared.IsSet);
        Assert.Null(cleared.Value);
        Assert.True(set.IsSet);
        Assert.Equal("hello", set.Value);
    }

    [Fact]
    public void Every_platform_name_is_listed_once()
    {
        Assert.Equal(Platforms.All.Count, Platforms.All.Distinct().Count());
        Assert.Contains(Platforms.InstagramBusiness, Platforms.All);
        Assert.Contains(Platforms.GoogleBusiness, Platforms.All);
    }
}
