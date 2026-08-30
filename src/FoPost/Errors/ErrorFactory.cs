using System.Text.Json;
using System.Text.Json.Nodes;

namespace FoPost;

/// <summary>Maps an HTTP status plus a decoded body onto the most specific exception.</summary>
internal static class ErrorFactory
{
    public static FoPostException FromResponse(int status, JsonNode? body, TimeSpan? retryAfter = null)
    {
        var code = StringField(body, "error");
        var message = StringField(body, "message");

        if (string.IsNullOrEmpty(message))
        {
            message = code ?? $"HTTP {status}";
        }

        return status switch
        {
            400 or 422 => new FoPostValidationException(message, status, code, body),
            401 => new FoPostAuthenticationException(message, status, code, body),
            402 => new FoPostPaymentRequiredException(message, status, code, body),
            403 => new FoPostPermissionDeniedException(message, status, code, body),
            404 => new FoPostNotFoundException(message, status, code, body),
            429 => new FoPostRateLimitException(message, status, code, body, retryAfter),
            _ => new FoPostException(message, status, code, body),
        };
    }

    private static string? StringField(JsonNode? body, string field)
    {
        if (body is not JsonObject obj || !obj.TryGetPropertyValue(field, out var node))
        {
            return null;
        }

        return node is JsonValue value && value.GetValueKind() == JsonValueKind.String
            ? value.GetValue<string>()
            : null;
    }
}
