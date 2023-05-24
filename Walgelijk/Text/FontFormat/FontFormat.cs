using System.Collections.Generic;

namespace Walgelijk.FontFormat;

public class FontFormat
{
    public readonly string Name;
    public readonly Texture Atlas;
    public readonly Dictionary<KerningPair, Kerning> Kernings = new();
    public readonly Glyph[] Glyphs;

    public FontFormat(string name, Texture atlas, Dictionary<KerningPair, Kerning> kernings, Glyph[] glyphs)
    {
        Name = name;
        Atlas = atlas;
        Kernings = kernings;
        Glyphs = glyphs;
    }
}

public readonly struct Glyph
{
    public readonly char Character;
    public readonly float Advance;
    public readonly Rect GeometryRect;
    public readonly Rect TextureRect;

    public Glyph(char character, float advance, Rect geometryRect, Rect textureRect)
    {
        Character = character;
        Advance = advance;
        GeometryRect = geometryRect;
        TextureRect = textureRect;
    }
}
