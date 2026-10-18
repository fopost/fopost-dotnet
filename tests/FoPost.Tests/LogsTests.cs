using Xunit;

namespace FoPost.Tests;

public class LogsTests
{
    [Fact]
    public async Task List_sends_the_filters_and_reads_data_meta_and_summary()
    {
        var handler = new StubHandler().Json("""
        {
          "data":[{"id":41,"workspace_id":"ws_1","time":"2026-09-01T10:00:00Z","auth_method":"api_key","method":"POST","route":"/v1/posts","path":"/v1/posts","status_code":201,"duration_ms":88,"platform":null,"message":null}],
          "meta":{"next_cursor":"40","has_more":true},
          "summary":{"since":"2026-08-25T00:00:00Z","total":120,"errors":3,"buckets":[{"at":"2026-09-01T00:00:00Z","total":12,"errors":1}]}
        }
        """);
        using var test = new TestClient(handler);

        var page = await test.Client.Logs.ListAsync(new ListLogsOptions { WorkspaceId = "ws_1", Days = 7, Status = "failed", Before = "50" });

        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal($"{TestClient.BaseUrl}/v1/logs?workspace_id=ws_1&days=7&status=failed&before=50", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal(41, Assert.Single(page).Id);
        Assert.Equal("api_key", page[0].AuthMethod);
        Assert.Equal("40", page.NextCursor);
        Assert.True(page.HasMore);
        Assert.Equal(120, page.Summary.Total);
        Assert.Equal(1, Assert.Single(page.Summary.Buckets).Errors);
    }
}
