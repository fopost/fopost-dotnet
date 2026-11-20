namespace FoPost;

/// <summary>Optional fields of a font upload.</summary>
public sealed class UploadFontOptions
{
    /// <summary>The family name designs refer to; defaults to the file name.</summary>
    public string? Family { get; set; }

    /// <summary>100 to 1000; defaults to 400.</summary>
    public int? Weight { get; set; }

    /// <summary><c>normal</c> (the default) or <c>italic</c>.</summary>
    public string? Style { get; set; }
}
