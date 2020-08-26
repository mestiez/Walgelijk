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
        public Glyph[] Glyphs { get; internal set; }
        public Kerning[] Kernings { get; internal set; }

        public Material Material { get; internal set; }

        public static Font Load(string path)
        {
            return FontLoader.LoadFromMetadata(path);
        }

        public Glyph GetGlyph(char c)
        {
            foreach (var glyph in Glyphs)
                if ((char)glyph.Identity == c)
                    return glyph;
            return Glyphs.Last();
        }
    }
}
