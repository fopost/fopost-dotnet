using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using FoPost.Http;

namespace FoPost.Resources;

/// <summary>Shared decoding used by every resource.</summary>
internal static class ResourceHelpers
{
    public static T Require<T>(JsonNode? node)
    {
        var value = node.Deserialize<T>(FoPostJson.Options);
        if (value is null)
        {
            throw new FoPostException("The API returned an empty body where a resource was expected", 200);
        }

        return value;
    }

    public static IReadOnlyList<T> ToList<T>(JsonNode? node)
    {
        if (node is not JsonArray)
        {
            return Array.Empty<T>();
        }

        return node.Deserialize<List<T>>(FoPostJson.Options) ?? new List<T>();
    }

    public static PageMeta ReadMeta(JsonNode? body)
    {
        if (body is JsonObject obj &&
            obj.TryGetPropertyValue("meta", out var meta) &&
            meta is JsonObject)
        {
            return meta.Deserialize<PageMeta>(FoPostJson.Options) ?? new PageMeta();
        }

        return new PageMeta();
    }

    /// <summary>The API takes UTC ISO 8601 timestamps; normalize whatever the caller had.</summary>
    public static string Iso(DateTimeOffset moment) =>
        moment.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);
}
