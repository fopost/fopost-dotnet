using System.Text.Json;
using System.Text.Json.Nodes;

namespace FoPost;

/// <summary>
/// Base class for every error returned by the FoPost API.
/// </summary>
/// <remarks>
/// The API answers failures with an <c>{ "error": "&lt;code&gt;", "message":
/// "&lt;human readable&gt;" }</c> envelope, which maps onto <see cref="Code"/>
/// and <see cref="System.Exception.Message"/>.
/// </remarks>
public class FoPostException : Exception
{
    public FoPostException(string message, int status, string? code = null, JsonNode? body = null)
        : base(message)
    {
        Status = status;
        Code = code;
        Body = body;
    }

    /// <summary>HTTP status the API answered with.</summary>
    public int Status { get; }

    /// <summary>Machine-readable error code, from the body's <c>error</c> field.</summary>
    public string? Code { get; }

    /// <summary>The decoded response body, when there was one.</summary>
    public JsonNode? Body { get; }

    /// <summary>Reads a string field off the error body, or null when it is absent.</summary>
    protected string? BodyString(string field)
    {
        if (Body is not JsonObject obj || !obj.TryGetPropertyValue(field, out var node))
        {
            return null;
        }

        return node is JsonValue value && value.GetValueKind() == JsonValueKind.String
            ? value.GetValue<string>()
            : null;
    }

    public override string ToString()
    {
        var suffix = Code is null ? string.Empty : $" ({Code})";
        return $"[{Status}{suffix}] {Message}";
    }
}

/// <summary>400 or 422 — the request body or query failed validation.</summary>
public sealed class FoPostValidationException : FoPostException
{
    public FoPostValidationException(string message, int status, string? code = null, JsonNode? body = null)
        : base(message, status, code, body) { }
}

/// <summary>401 — missing, invalid, or expired API key.</summary>
public sealed class FoPostAuthenticationException : FoPostException
{
    public FoPostAuthenticationException(string message, int status, string? code = null, JsonNode? body = null)
        : base(message, status, code, body) { }
}

/// <summary>402 — no active subscription, or AI credits exhausted.</summary>
public sealed class FoPostPaymentRequiredException : FoPostException
{
    public FoPostPaymentRequiredException(string message, int status, string? code = null, JsonNode? body = null)
        : base(message, status, code, body) { }

    /// <summary>The path the API suggests sending the user to, when it sends one.</summary>
    public string? UpgradeUrl => BodyString("upgrade_url");
}

/// <summary>403 — the credential is valid but lacks the scope or workspace access.</summary>
public sealed class FoPostPermissionDeniedException : FoPostException
{
    public FoPostPermissionDeniedException(string message, int status, string? code = null, JsonNode? body = null)
        : base(message, status, code, body) { }
}

/// <summary>404 — no such resource, or it is outside the credential's reach.</summary>
public sealed class FoPostNotFoundException : FoPostException
{
    public FoPostNotFoundException(string message, int status, string? code = null, JsonNode? body = null)
        : base(message, status, code, body) { }
}

/// <summary>429 — rate limit exceeded.</summary>
public sealed class FoPostRateLimitException : FoPostException
{
    public FoPostRateLimitException(
        string message,
        int status,
        string? code = null,
        JsonNode? body = null,
        TimeSpan? retryAfter = null)
        : base(message, status, code, body)
    {
        RetryAfter = retryAfter;
    }

    /// <summary>How long the API asked us to wait, when it said.</summary>
    public TimeSpan? RetryAfter { get; }
}
