using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Simple rectangle structure
/// </summary>
public struct Rect : IEquatable<Rect>
{
    /// <summary>
    /// Minimum X point
    /// </summary>
    public float MinX;
    /// <summary>
    /// Minimum Y point
    /// </summary>
    public float MinY;
    /// <summary>
    /// Maximum X point
    /// </summary>
    public float MaxX;
    /// <summary>
    /// Maximum Y point
    /// </summary>
    public float MaxY;

    /// <summary>
    /// Width of rectangle
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public float Width
    {
        get => MaxX - MinX;
        set => MaxX = MinX + value;
    }

    /// <summary>
    /// Height of rectangle
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public float Height
    {
        get => MaxY - MinY;
        set => MaxY = MinY + value;
    }

    /// <summary>
    /// The top right of the rectangle
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public Vector2 TopRight
    {
        get => new(MaxX, MaxY);
        set { MaxX = value.X; MaxY = value.Y; }
    }

    /// <summary>
    /// The bottom right of the rectangle
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public Vector2 BottomRight
    {
        get => new(MaxX, MinY);
        set { MaxX = value.X; MinY = value.Y; }
    }

    /// <summary>
    /// The top left of the rectangle
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public Vector2 TopLeft
    {
        get => new(MinX, MaxY);
        set { MinX = value.X; MaxY = value.Y; }
    }

    /// <summary>
    /// The bottom left of the rectangle
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public Vector2 BottomLeft
    {
        get => new(MinX, MinY);
        set { MinX = value.X; MinY = value.Y; }
    }

    /// <summary>
    /// Equivalent to Width * Height
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public float Area => Width * Height;

    /// <summary>
    /// Returns the center of the rectangle. Calculated using (min + max) * 0.5
    /// </summary>
    public readonly Vector2 GetCenter() => new((MinX + MaxX) * 0.5f, (MinY + MaxY) * 0.5f);

    /// <summary>
    /// Offset the rectangle such that the center is the given point
    /// </summary>
    public void SetCenter(Vector2 point)
    {
        var center = GetCenter();
        var offset = point - center;
        MaxX += offset.X;
        MinX += offset.X;
        MaxY += offset.Y;
        MinY += offset.Y;
    }

    /// <summary>
    /// Returns the size of the rectangle. Calculated using (max - min)
    /// </summary>
    public readonly Vector2 GetSize() => new(MaxX - MinX, MaxY - MinY);

    /// <summary>
    /// Returns a random point inside this rectangle
    /// </summary>
    public readonly Vector2 GetRandomPoint() => new(Utilities.RandomFloat(MinX, MaxX), Utilities.RandomFloat(MinY, MaxY));

    /// <summary>
    /// Create a rectangle
    /// </summary>
    public Rect(float minX, float minY, float maxX, float maxY)
    {
        MinX = minX;
        MinY = minY;

        MaxX = maxX;
        MaxY = maxY;
    }

    /// <summary>
    /// Create a rectangle given the center and the size
    /// </summary>
    public Rect(Vector2 center, Vector2 size)
    {
        var halfSize = size / 2;
        MinX = center.X - halfSize.X;
        MinY = center.Y - halfSize.Y;

        MaxX = center.X + halfSize.X;
        MaxY = center.Y + halfSize.Y;
    }


    /// <summary>
    /// Ensures that the Min and Max components are the minimum and maximum respectively.
    /// </summary>
    /// <returns></returns>
    public Rect SortComponents()
        => new Rect(
                MathF.Min(MinX, MaxX), MathF.Min(MinY, MaxY),
                MathF.Max(MinX, MaxX), MathF.Max(MinY, MaxY));

    /// <summary>
    /// Identical to <see cref="SDF.Rectangle(Vector2, Vector2, Vector2)"/>
    /// </summary>
    public readonly float SignedDistanceTo(Vector2 p) => SDF.Rectangle(p, GetCenter(), GetSize());

    /// <summary>
    /// Return a copy of the rectangle but translated by the given amount
    /// </summary>
    public readonly Rect Translate(Vector2 offset)
    {
        return new Rect(MinX + offset.X, MinY + offset.Y, MaxX + offset.X, MaxY + offset.Y);
    }

    /// <summary>
    /// Return a copy of the rectangle but translated by the given amount
    /// </summary>
    public readonly Rect Translate(float x, float y)
    {
        return new Rect(MinX + x, MinY + y, MaxX + x, MaxY + y);
    }

    /// <summary>
    /// Returns a copy of the rectangle that is clamped inside the given container rectangle, 
    /// by translating the current rectangle by the necessary offset.
    /// </summary>
    public readonly Rect ClampInside(in Rect container)
    {
        var offset = Vector2.Zero;

        if (MinX < container.MinX)
            offset.X += container.MinX - MinX;
        if (MinY < container.MinY)
            offset.Y += container.MinY - MinY;

        if (MaxX > container.MaxX)
            offset.X += container.MaxX - MaxX;
        if (MaxY > container.MaxY)
            offset.Y += container.MaxY - MaxY;

        return Translate(offset);
    }

    /// <summary>
    /// Return a copy of the rectangle expanded in all directions by the given amount
    /// </summary>
    public readonly Rect Expand(float f)
    {
        return new Rect(MinX - f, MinY - f, MaxX + f, MaxY + f);
    }

    /// <summary>
    /// Does the rectangle contain the given point?
    /// </summary>
    public readonly bool ContainsPoint(Vector2 point) =>
        point.X > MinX && point.X < MaxX && point.Y > MinY && point.Y < MaxY;

    /// <summary>
    /// Does the rectangle, optionally expanded, contain the given point?
    /// </summary>
    public readonly bool ContainsPoint(Vector2 point, float expand) =>
        point.X > MinX - expand && point.X < MaxX + expand && point.Y > MinY - expand && point.Y < MaxY + expand;

    /// <summary>
    /// Does the rectangle contain the given rectangle? Note that if the given rectangle is identical to the instance, this function will return true
    /// </summary>
    public readonly bool ContainsRect(Rect other) =>
        other.MinX >= MinX && other.MaxX <= MaxX && other.MinY >= MinY && other.MaxY <= MaxY;

    /// <summary>
    /// Does the rectangle overlap with the given rectangle?
    /// </summary>
    public readonly bool IntersectsRectangle(Rect b) => !(MaxX < b.MinX || MinX > b.MaxX || MinY > b.MaxY || MaxY < b.MinY);

    /// <summary>
    /// Returns the point on the rectangle that is closest to the given point
    /// </summary>
    public readonly Vector2 ClosestPoint(Vector2 point) => new(
            Utilities.Clamp(point.X, MinX, MaxX),
            Utilities.Clamp(point.Y, MinY, MaxY)
            );

    /// <summary>
    /// This will return a copy of this rectangle that is stretched just enough to contain the given point
    /// </summary>
    public readonly Rect StretchToContain(Vector2 point)
    {
        return new Rect(
            MathF.Min(MinX, point.X),
            MathF.Min(MinY, point.Y),
            MathF.Max(MaxX, point.X),
            MathF.Max(MaxY, point.Y));
    }

    /// <summary>
    /// This will return a copy of this rectangle that is stretched just enough to contain the given rect
    /// </summary>
    public readonly Rect StretchToContain(Rect rect)
    {
        return StretchToContain(rect.TopLeft).StretchToContain(rect.TopRight).StretchToContain(rect.BottomLeft).StretchToContain(rect.BottomRight);
    }

    /// <summary>
    /// This will return a copy of this rectangle scaled in each direction by the given value ranging, 1 meaning 100%
    /// </summary>
    public readonly Rect Scale(float scale)
    {
        return new Rect(GetCenter(), GetSize() * scale);
    }

    /// <summary>
    /// This will return a copy of this rectangle scaled in each direction by the given value ranging, 1 meaning 100%
    /// </summary>
    public readonly Rect Scale(float x, float y)
    {
        var v = GetSize();
        v.X *= x;
        v.Y *= y;
        return new Rect(GetCenter(), v);
    }

    /// <summary>
    /// Return the rectangle that represents the intersection between this rectangle and the given rectangle
    /// </summary>
    public readonly Rect Intersect(Rect other)
    {
        float minx = MathF.Max(MinX, other.MinX);
        float maxx = MathF.Min(MaxX, other.MaxX);

        float miny = MathF.Max(MinY, other.MinY);
        float maxy = MathF.Min(MaxY, other.MaxY);

        if (minx > maxx)
            maxx = minx;

        if (miny > maxy)
            maxy = miny;

        var r = new Rect(minx, miny, maxx, maxy);
        return r;
    }

    public bool Equals(Rect other)
    {
        return other.MinY == MinY && other.MinX == MinX && other.MaxX == MaxX && other.MaxY == MaxY;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MinX, MinY, MaxX, MaxY);
    }

    public static bool operator ==(Rect left, Rect right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rect left, Rect right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        var center = GetCenter();
        var size = GetSize();
        return $"(Center {center.X}, {center.Y}, Size {size.X}, {size.Y})";
    }
}
