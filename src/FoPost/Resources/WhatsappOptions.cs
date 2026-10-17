using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FoPost;

/// <summary>A partial profile update: omitted fields keep their value.</summary>
public sealed class UpdateWhatsappProfileOptions
{
    [JsonPropertyName("about")]
    public string? About { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("vertical")]
    public string? Vertical { get; set; }

    [JsonPropertyName("websites")]
    public IList<string>? Websites { get; set; }

    /// <summary>A library media id, uploaded first.</summary>
    [JsonPropertyName("profile_picture_media_id")]
    public string? ProfilePictureMediaId { get; set; }
}

/// <summary>Files a template for review.</summary>
public sealed class CreateWhatsappTemplateOptions
{
    /// <summary>Lowercase letters, digits and underscores.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    /// <summary>MARKETING, UTILITY or AUTHENTICATION.</summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("components")]
    public IList<JsonNode?> Components { get; set; } = new List<JsonNode?>();

    /// <summary>Lets the platform re-file a template it judges differently.</summary>
    [JsonPropertyName("allow_category_change")]
    public bool? AllowCategoryChange { get; set; }
}

/// <summary>Creates a template from one of the platform's library entries.</summary>
public sealed class ImportWhatsappTemplateOptions
{
    [JsonPropertyName("library_template_name")]
    public string LibraryTemplateName { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("library_template_button_inputs")]
    public IList<JsonNode?>? LibraryTemplateButtonInputs { get; set; }
}

/// <summary>Edits a template. The name cannot change.</summary>
public sealed class UpdateWhatsappTemplateOptions
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("components")]
    public IList<JsonNode?>? Components { get; set; }
}

/// <summary>Creates or updates a group.</summary>
public sealed class WhatsappGroupOptions
{
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>Turns the cart or the catalog on or off.</summary>
public sealed class UpdateWhatsappCommerceOptions
{
    [JsonPropertyName("is_cart_enabled")]
    public bool? CartEnabled { get; set; }

    [JsonPropertyName("is_catalog_visible")]
    public bool? CatalogVisible { get; set; }
}

/// <summary>Creates a draft flow; its screens are uploaded separately.</summary>
public sealed class CreateWhatsappFlowOptions
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public IList<string> Categories { get; set; } = new List<string>();

    /// <summary>Where the platform calls back for a flow that reads live data.</summary>
    [JsonPropertyName("endpoint_uri")]
    public string? EndpointUri { get; set; }

    [JsonPropertyName("clone_flow_id")]
    public string? CloneFlowId { get; set; }
}

/// <summary>Changes a flow's metadata, not its screens.</summary>
public sealed class UpdateWhatsappFlowOptions
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("categories")]
    public IList<string>? Categories { get; set; }

    [JsonPropertyName("endpoint_uri")]
    public string? EndpointUri { get; set; }
}
