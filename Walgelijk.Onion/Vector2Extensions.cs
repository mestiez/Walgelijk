using System.Numerics;

namespace Walgelijk.Onion;

public static class QuantiseExtensions
{
    public static Vector2 Quantise(this Vector2 v) => new Vector2((int)v.X, (int)v.Y);
    public static Rect Quantise(this Rect v) => new Rect((int)v.MinX, (int)v.MinY, (int)v.MaxX, (int)v.MaxY);
}