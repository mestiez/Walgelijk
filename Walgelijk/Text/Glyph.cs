using System;

namespace Walgelijk
{
    /// <summary>
    /// Structure with font glyph information
    /// </summary>
    public struct Glyph : IEquatable<Glyph>
    {
        /// <summary>
        /// Character this glyph belongs to
        /// </summary>
        public char Identity;

        /// <summary>
        /// X position on the page in pixels
        /// </summary>
        public int X;
        /// <summary>
        /// Y position on the page in pixels
        /// </summary>
        public int Y;

        /// <summary>
        /// Width of the glyph in pixels
        /// </summary>
        public int Width;
        /// <summary>
        /// Height of the glyph in pixels
        /// </summary>
        public int Height;

        /// <summary>
        /// Horizontal offset of this character in pixels
        /// </summary>
        public int XOffset;
        /// <summary>
        /// Vertical offset of this character in pixels
        /// </summary>
        public int YOffset;

        /// <summary>
        /// How many pixels to advance the cursor after this glyph
        /// </summary>
        public int Advance;

        /// <summary>
        /// Page index of the glyph
        /// </summary>
        public int Page;

        public override bool Equals(object? obj)
        {
            return obj is Glyph glyph &&
                   Identity == glyph.Identity &&
                   X == glyph.X &&
                   Y == glyph.Y &&
                   Width == glyph.Width &&
                   Height == glyph.Height &&
                   XOffset == glyph.XOffset &&
                   YOffset == glyph.YOffset &&
                   Advance == glyph.Advance &&
                   Page == glyph.Page;
        }

        public bool Equals(Glyph glyph)
        {
            return Identity == glyph.Identity &&
                   X == glyph.X &&
                   Y == glyph.Y &&
                   Width == glyph.Width &&
                   Height == glyph.Height &&
                   XOffset == glyph.XOffset &&
                   YOffset == glyph.YOffset &&
                   Advance == glyph.Advance &&
                   Page == glyph.Page;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Identity);
            hash.Add(X);
            hash.Add(Y);
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(XOffset);
            hash.Add(YOffset);
            hash.Add(Advance);
            hash.Add(Page);
            return hash.ToHashCode();
        }

        public static bool operator ==(Glyph left, Glyph right)
        {
            return left.Identity == right.Identity &&
                   left.X == right.X &&
                   left.Y == right.Y &&
                   left.Width == right.Width &&
                   left.Height == right.Height &&
                   left.XOffset == right.XOffset &&
                   left.YOffset == right.YOffset &&
                   left.Advance == right.Advance &&
                   left.Page == right.Page;
        }

        public static bool operator !=(Glyph left, Glyph right)
        {
            return !(left == right);
        }
    }
}
