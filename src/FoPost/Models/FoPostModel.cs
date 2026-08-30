using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>
/// Base for every response model. Keys the SDK does not know about are kept in
/// <see cref="AdditionalData"/> rather than dropped, so a field added
/// server-side is still reachable from an older SDK.
/// </summary>
public abstract class FoPostModel
{
    [JsonExtensionData]
    public IDictionary<string, JsonElement>? AdditionalData { get; set; }
}
