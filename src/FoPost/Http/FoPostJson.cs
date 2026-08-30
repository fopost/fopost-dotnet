using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace FoPost.Http;

/// <summary>
/// The serializer settings every response is decoded with.
/// </summary>
/// <remarks>
/// The API is not consistent about its wire casing — posts come back
/// snake_case, accounts and deliveries camelCase — so models declare the
/// snake_case name and <see cref="AddCamelCaseAliases"/> registers a
/// read-only camelCase alias beside it. Unknown keys land in
/// <see cref="FoPostModel.AdditionalData"/> rather than being dropped, so a
/// field added server-side never breaks an older client.
/// </remarks>
internal static class FoPostJson
{
    public static readonly JsonSerializerOptions Options = Build();

    private static JsonSerializerOptions Build()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { AddCamelCaseAliases },
            },
        };
        return options;
    }

    /// <summary>
    /// Give every snake_case property a camelCase twin that only reads, so a
    /// payload spelled either way binds to the same member.
    /// </summary>
    internal static void AddCamelCaseAliases(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
        {
            return;
        }

        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in typeInfo.Properties)
        {
            existing.Add(property.Name);
        }

        var aliases = new List<JsonPropertyInfo>();
        foreach (var property in typeInfo.Properties)
        {
            if (property.Set is null)
            {
                continue;
            }

            var camel = ToCamelCase(property.Name);
            if (camel == property.Name || existing.Contains(camel))
            {
                continue;
            }

            var alias = typeInfo.CreateJsonPropertyInfo(property.PropertyType, camel);
            alias.Set = property.Set;
            // Get stays null: the alias binds on read and never writes, so a
            // round-trip emits the snake_case name once and only once.
            aliases.Add(alias);
            existing.Add(camel);
        }

        foreach (var alias in aliases)
        {
            typeInfo.Properties.Add(alias);
        }
    }

    internal static string ToCamelCase(string name)
    {
        if (!name.Contains('_', StringComparison.Ordinal))
        {
            return name;
        }

        var builder = new StringBuilder(name.Length);
        var upperNext = false;
        foreach (var character in name)
        {
            if (character == '_')
            {
                upperNext = builder.Length > 0;
                continue;
            }

            builder.Append(upperNext ? char.ToUpperInvariant(character) : character);
            upperNext = false;
        }

        return builder.ToString();
    }
}
