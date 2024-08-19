
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

    public struct MsdfKerning : IEquatable<MsdfKerning>
    {
        public char Unicode1, Unicode2;
        public float Advance;

        public override bool Equals(object? obj)
        {
            return obj is MsdfKerning kerning && Equals(kerning);
        }

        public bool Equals(MsdfKerning other)
        {
            return Unicode1 == other.Unicode1 &&
                   Unicode2 == other.Unicode2 &&
                   float.Abs(Advance - other.Advance) < 0.001f;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Unicode1, Unicode2, (int)(Advance * 1000));
        }

        public static bool operator ==(MsdfKerning left, MsdfKerning right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MsdfKerning left, MsdfKerning right)
        {
            return !(left == right);
        }
    }

    public struct MsdfRect
    {
        public float Left, Bottom, Right, Top;

        public readonly Rect GetRect() => new(Left, Bottom, Right, Top);
    }
}