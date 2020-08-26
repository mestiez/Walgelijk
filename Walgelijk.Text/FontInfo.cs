namespace Walgelijk.Text
{
    internal struct FontInfo
    {
        public string Name;
        public int Size;
        public bool Bold;
        public bool Italic;

        public int PageCount;
        public int Width;
        public int Height;
        public int LineHeight;

        public string[] PagePaths;

        public int GlyphCount;
        public int KerningCount;
    }
}
