using System;
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
                        Kernings = format.Kernings.ToDictionary(a => new KerningPair(a.FirstChar, a.SecondChar)),
                        Glyphs = format.Glyphs.ToDictionary(static g => g.Character),
                        Rendering = FontRendering.MSDF,
                        Width = format.Atlas.Width,
                        Height = format.Atlas.Height,
                        Base = 12,
                        LineHeight = 12,
                        Size = 12,
                        Material = FontMaterialCreator.CreateMSDFMaterial(format.Atlas)
                    };
                }
            default:
                throw new Exception("Could not detect format for given font file: " + path);
        }
    }
}