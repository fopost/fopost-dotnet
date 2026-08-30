using System.Net;
using Xunit;

namespace FoPost.Tests;

public class ErrorTests
{
    [Theory]
    [InlineData(HttpStatusCode.BadRequest, typeof(FoPostValidationException))]
    [InlineData(HttpStatusCode.Unauthorized, typeof(FoPostAuthenticationException))]
    [InlineData(HttpStatusCode.PaymentRequired, typeof(FoPostPaymentRequiredException))]
    [InlineData(HttpStatusCode.Forbidden, typeof(FoPostPermissionDeniedException))]
    [InlineData(HttpStatusCode.NotFound, typeof(FoPostNotFoundException))]
    [InlineData(HttpStatusCode.UnprocessableEntity, typeof(FoPostValidationException))]
    [InlineData(HttpStatusCode.InternalServerError, typeof(FoPostException))]
    public async Task Each_status_maps_onto_its_exception(HttpStatusCode status, Type expected)
    {
        var handler = new StubHandler()
            .Json("""{"error":"nope","message":"Nope"}""", status);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAnyAsync<FoPostException>(
            () => test.Client.Posts.GetAsync("post_1"));

        Assert.IsType(expected, error);
        Assert.Equal((int)status, error.Status);
        Assert.Equal("nope", error.Code);
        Assert.Equal("Nope", error.Message);
    }

    [Fact]
    public async Task A_402_exposes_the_upgrade_url_the_api_suggests()
    {
        var handler = new StubHandler().Json(
            """{"error":"insufficient_credits","message":"Out of credits","upgrade_url":"/settings/billing"}""",
            HttpStatusCode.PaymentRequired);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostPaymentRequiredException>(
            () => test.Client.Ai.CreditsAsync());

        Assert.Equal("/settings/billing", error.UpgradeUrl);
    }

    [Fact]
    public async Task An_error_body_with_no_message_falls_back_to_the_code()
    {
        var handler = new StubHandler().Json("""{"error":"subscription_required"}""", HttpStatusCode.Forbidden);
        using var test = new TestClient(handler);

        var error = await Assert.ThrowsAsync<FoPostPermissionDeniedException>(
            () => test.Client.Posts.GetAsync("post_1"));

        Assert.Equal("subscription_required", error.Message);
    }

    [Fact]
    public void ToString_reads_as_status_code_and_message()
    {
        var error = new FoPostException("Nope", 404, "not_found");

        Assert.Equal("[404 (not_found)] Nope", error.ToString());
    }
}
