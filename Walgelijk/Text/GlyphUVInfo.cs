using System;

namespace Walgelijk;

internal struct GlyphUVInfo
{
    public float X;
    public float Y;
    public float Width;
    public float Height;

    public GlyphUVInfo(float x, float y, float w, float h)
    {
        X = x;
        Y = y;
        Width = w;
        Height = h;
    }

    public override bool Equals(object? obj)
    {
        return obj is GlyphUVInfo info &&
               X == info.X &&
               Y == info.Y &&
               Width == info.Width &&
               Height == info.Height;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public static bool operator ==(GlyphUVInfo left, GlyphUVInfo right)
    {
        return left.X == right.X &&
               left.Y == right.Y &&
               left.Width == right.Width &&
               left.Height == right.Height;
    }

    public static bool operator !=(GlyphUVInfo left, GlyphUVInfo right)
    {
        return !(left == right);
    }
}
