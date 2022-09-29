using System;
using System.Collections.Generic;

namespace Walgelijk
{
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
        /// Retrieve if the font was loaded with the smooth flag. The flag determined the <see cref="FilterMode"/> the pages were loaded with.
        /// </summary>
        public bool Smooth { get; internal set; }
        /// <summary>
        /// Retrieve if the font was loaded with the italic style
        /// </summary>
        public bool Italic { get; internal set; }
        /// <summary>
        /// Page width in pixels
        /// </summary>
        public int Width { get; internal set; }
        /// <summary>
        /// Page height in pixels
        /// </summary>
        public int Height { get; internal set; }
        /// <summary>
        /// This is the distance in pixels between each line of text
        /// </summary>
        public int LineHeight { get; internal set; }
        /// <summary>
        /// The number of pixels from the absolute top of the line to the base of the characters
        /// </summary>
        public int Base { get; internal set; }
        /// <summary>
        /// Array of texture pages this font uses
        /// </summary>
        public IReadableTexture[]? Pages { get; internal set; }
        /// <summary>
        /// Glyphs by character
        /// </summary>
        public Dictionary<char, Glyph>? Glyphs { get; internal set; }
        /// <summary>
        /// Kernings by <see cref="KerningPair"/>
        /// </summary>
        public Dictionary<KerningPair, Kerning>? Kernings { get; internal set; }

        /// <summary>
        /// Material this font uses. Be aware this may be shared across text. Use <see cref="TextMaterial.CreateFor(Font)"/> to create a new material.
        /// </summary>
        public Material? Material
        {
            get;
            set;
        }

        /// <summary>
        /// Load a font from a metadata file (BMFont .fnt)
        /// </summary>
        public static Font Load(string path)
        {
            return FontLoader.LoadFromMetadata(path);
        }

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
        public Kerning GetKerning(char previous, char current)
        {
            if (Kernings == null || Kernings.Count == 0)
                return default;
            if (Kernings.TryGetValue(new KerningPair { CurrentChar = current, PreviousChar = previous }, out var kerning))
                return kerning;
            return default;
        }

        /// <summary>
        /// The default font
        /// </summary>
        public static Font Default = Resources.Load<Font>("resources/fonts/roboto mono.fnt", true);
    }

    /// <summary>
    /// The two characters a kerning amount applies to
    /// </summary>
    public struct KerningPair : IEquatable<KerningPair>
    {
        /// <summary>
        /// Previous character in the sequence
        /// </summary>
        public char PreviousChar;
        /// <summary>
        /// Current character in the sequence
        /// </summary>
        public char CurrentChar;

        public override bool Equals(object? obj)
        {
            return obj is KerningPair pair &&
                   PreviousChar == pair.PreviousChar &&
                   CurrentChar == pair.CurrentChar;
        }

        public bool Equals(KerningPair other)
        {
            return PreviousChar == other.PreviousChar &&
                   CurrentChar == other.CurrentChar;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PreviousChar, CurrentChar);
        }

        public static bool operator ==(KerningPair left, KerningPair right)
        {
            return left.PreviousChar == right.PreviousChar &&
                   left.CurrentChar == right.CurrentChar;
        }

        public static bool operator !=(KerningPair left, KerningPair right)
        {
            return !(left == right);
        }
    }
}
