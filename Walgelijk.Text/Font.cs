using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;

namespace Walgelijk.Text
{
    public class Font
    {
        public string Name { get; internal set; }
        public int Size { get; internal set; }
        public bool Bold { get; internal set; }
        public bool Italic { get; internal set; }

        public int Width { get; internal set; }
        public int Height { get; internal set; }

        public int LineHeight { get; internal set; }

        public Texture[] Pages { get; internal set; }
        public Dictionary<char, Glyph> Glyphs { get; internal set; }
        public Dictionary<KerningPair, Kerning> Kernings { get; internal set; }

        public Material Material { get; internal set; }

        public static Font Load(string path)
        {
            return FontLoader.LoadFromMetadata(path);
        }

        public Glyph GetGlyph(char c)
        {
            if (Glyphs.Count == 0) return default;
            if (Glyphs.TryGetValue(c, out var glyph))
                return glyph;
            return Glyphs.Last().Value;
        }

        public Kerning GetKerning(char previous, char current)
        {
            if (Kernings.Count == 0) return default;
            if (Kernings.TryGetValue(new KerningPair { CurrentChar = current, PreviousChar = previous }, out var kerning))
                return kerning;
            return Kernings.Last().Value;
        }
    }

    public struct KerningPair
    {
        public char PreviousChar, CurrentChar;
    }
}
