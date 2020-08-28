namespace Walgelijk
{
    /// <summary>
    /// Structure with font glyph information
    /// </summary>
    public struct Glyph
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
        /// Width of the page in pixels
        /// </summary>
        public int Width;
        /// <summary>
        /// Height of the page in pixels
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
    }
}
