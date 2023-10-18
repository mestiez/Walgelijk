using System.Collections.Generic;

namespace Walgelijk;

/// <summary>
/// Object that holds font information and pages
/// </summary>
public class Font
{
    /// <summary>
    /// Display name of the font
    /// </summary>
    public string Name { get; internal set; } = string.Empty;
    /// <summary>
    /// Retrieve the point size this font was loaded with
    /// </summary>
    public int Size { get; internal set; }
    /// <summary>
    /// Retrieve if the font was loaded with the bold style
    /// </summary>
    public bool Bold { get; internal set; }
    /// <summary>
    /// Specifies the rendering method of the font. This determines what shader should be used.
    /// </summary>
    public FontRendering Rendering;
    /// <summary>
    /// Retrieve if the font was loaded with the italic style
    /// </summary>
    public bool Italic { get; internal set; }
    /// <summary>
    /// This is the distance in pixels between each line of text
    /// </summary>
    public int LineHeight { get; internal set; }
    /// <summary>
    /// The distance from the baseline to the top of lowercase characters (a.k.a the mean line)
    /// </summary>
    public int XHeight { get; internal set; }
    /// <summary>
    /// The texture containing all glyphs
    /// </summary>
    public IReadableTexture Page { get; internal set; } = Texture.ErrorTexture;
    /// <summary>
    /// Glyphs by character
    /// </summary>
    public Dictionary<char, Glyph> Glyphs { get; internal set; } = new Dictionary<char, Glyph>();
    /// <summary>
    /// Kernings by <see cref="KerningPair"/>
    /// </summary>
    public Dictionary<KerningPair, Kerning> Kernings { get; internal set; } = new Dictionary<KerningPair, Kerning>();

    /// <summary>
    /// Material this font uses. Be aware this may be shared across text. Use <see cref="FontMaterialCreator.CreateFor(Font)"/> to create a new material.
    /// </summary>
    public Material? Material { get; set; }

    /// <summary>
    /// Get the glyph for a character. 
    /// </summary>
    public Glyph GetGlyph(char c, Glyph fallback = default)
    {
        if (Glyphs == null || Glyphs.Count == 0)
            return default;
        if (Glyphs.TryGetValue(c, out var glyph))
            return glyph;
        return fallback;
    }

    /// <summary>
    /// Get kerning for two characters
    /// </summary>
    public Kerning GetKerning(char current, char next)
    {
        if (Kernings == null || Kernings.Count == 0)
            return default;
        if (Kernings.TryGetValue(new KerningPair { Current = current, Next = next }, out var kerning))
            return kerning;
        return default;
    }

    /// <summary>
    /// The default font
    /// </summary>
    public static Font Default = Resources.Load<Font>("resources/fonts/roboto mono.fnt", true);

    public override string ToString() => Name;
}
