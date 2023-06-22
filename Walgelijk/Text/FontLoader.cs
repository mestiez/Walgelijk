using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Walgelijk.BmFont;

namespace Walgelijk;

/// <summary>
/// Loads fonts based on the format that is detected or provided
/// </summary>
public static class FontLoader
{
    public static Font Load(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        switch (ext)
        {
            case ".fnt": return BmFontReader.LoadFromMetadata(path);
            case ".ttf": throw new Exception("TrueType font files need to be packed into a .wf file by Walgelijk.FontGenerator before being loaded");
            case ".wf":
                {
                    var format = FontFormat.Load(path);
                    return new Font
                    {
                        Name = format.Name,
                        Page = format.Atlas,
                        Kernings = format.Kernings.Distinct(new KerningComparer()).ToDictionary(static a => new KerningPair(a.FirstChar, a.SecondChar)),
                        Glyphs = format.Glyphs.Distinct().ToDictionary(static g => g.Character),
                        Rendering = FontRendering.MSDF,
                        XHeight = (int)format.XHeight,
                        LineHeight = (int)format.LineHeight,
                        Size = format.Size,
                        Material = FontMaterialCreator.CreateMSDFMaterial(format.Atlas)
                    };
                }
            default:
                throw new Exception("Could not detect format for given font file: " + path);
        }
    }

    private class KerningComparer : IEqualityComparer<Kerning>
    {
        public bool Equals(Kerning x, Kerning y)
        {
            return (x.FirstChar == y.FirstChar) && (y.SecondChar == x.SecondChar);
        }

        public int GetHashCode([DisallowNull] Kerning obj)
        {
            return HashCode.Combine(obj.FirstChar, obj.SecondChar);
        }
    }
}