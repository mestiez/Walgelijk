namespace Walgelijk.FontGenerator;

public struct MsdfDataStructs
{
    // all of these are here for the deserialisation of the metadata that msdf-atlas-gen outputs.
    // DO NOT CHANGE ANY MEMBER NAMES

    public class MsdfGenFont
    {
        public MsdfAtlas Atlas;
        public MsdfMetrics Metrics;
        public MsdfGlyph[]? Glyphs;
        public MsdfKerning[]? Kerning;
    }

    public struct MsdfAtlas
    {
        public string Type;
        public float DistanceRange;
        public float Size;
        public int Width;
        public int Height;
        public string YOrigin;
    }

    public struct MsdfMetrics
    {
        public int EmSize;
        public float LineHeight;
        public float Ascender;
        public float Descender;
        public float UnderlineY;
        public float UnderlineThickness;
    }

    public struct MsdfGlyph
    {
        public char Unicode;
        public float Advance;
        public MsdfRect PlaneBounds, AtlasBounds;
    }

    public struct MsdfKerning
    {
        public char Unicode1, Unicode2;
        public float Advance;
    }

    public struct MsdfRect
    {
        public float Left, Bottom, Right, Top;

        public readonly Rect GetRect() => new(Left, Bottom, Right, Top);
    }
}