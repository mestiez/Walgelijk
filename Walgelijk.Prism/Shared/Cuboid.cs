using System.Numerics;

namespace koploper;

public struct Cuboid(Vector3 min, Vector3 max) : IEquatable<Cuboid>
{
    public Vector3 Min = min, Max = max;

    public readonly float SizeX => Max.X - Min.X;
    public readonly float SizeY => Max.Y - Min.Y;
    public readonly float SizeZ => Max.Z - Min.Z;
    public readonly Vector3 Center => (Min + Max) / 2;
    public readonly Vector3 Extents => new(SizeX / 2, SizeY / 2, SizeZ / 2);

    public static Cuboid Cube(Vector3 center, float size)
    {
        return new Cuboid(new Vector3(-size / 2), new Vector3(size / 2)).Translate(center);
    }

    public readonly Cuboid Inflate(float v)
    {
        var min = Vector3.Min(Min, Max);
        var max = Vector3.Max(Min, Max);

        min.X -= v;
        min.Y -= v;
        min.Z -= v;

        max.X += v;
        max.Y += v;
        max.Z += v;

        return new(min, max);
    }

    public readonly bool ContainsPoint(Vector3 point)
    {
        return
            point.X >= Min.X && point.X <= Max.X &&
            point.Y >= Min.Y && point.Y <= Max.Y &&
            point.Z >= Min.Z && point.Z <= Max.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is Cuboid box && Equals(box);
    }

    public readonly Cuboid MinkowskiDifference(Cuboid other)
    {
        var amin = Min;
        var amax = Max;
        var bmin = other.Min;
        var bmax = other.Max;

        Vector3 min = new()
        {
            X = amin.X - bmax.X,
            Y = amin.Y - bmax.Y,
            Z = amin.Z - bmax.Z
        };

        Vector3 max = new()
        {
            X = amax.X - bmin.X,
            Y = amax.Y - bmin.Y,
            Z = amax.Z - bmin.Z
        };

        return new Cuboid(min, max);
    }

    public readonly Cuboid StretchToContain(Vector3 point)
    {
        var b = this;

        b.Min.X = float.Min(point.X, b.Min.X);
        b.Min.Y = float.Min(point.Y, b.Min.Y);
        b.Min.Z = float.Min(point.Z, b.Min.Z);

        b.Max.X = float.Max(point.X, b.Max.X);
        b.Max.Y = float.Max(point.Y, b.Max.Y);
        b.Max.Z = float.Max(point.Z, b.Max.Z);

        return b;
    }

    public readonly bool Contains(Cuboid other)
    {
        bool minContained = Min.X <= other.Min.X &&
                            Min.Y <= other.Min.Y &&
                            Min.Z <= other.Min.Z;

        bool maxContained = Max.X >= other.Max.X &&
                            Max.Y >= other.Max.Y &&
                            Max.Z >= other.Max.Z;

        return minContained && maxContained;
    }

    public IEnumerable<Vector3> EnumerateCorners()
    {
        var xx = Max;
        var oo = Min;

        yield return new Vector3(xx.X, xx.Y, xx.Z);
        yield return new Vector3(xx.X, xx.Y, oo.Z);
        yield return new Vector3(xx.X, oo.Y, xx.Z);
        yield return new Vector3(xx.X, oo.Y, oo.Z);
        yield return new Vector3(oo.X, xx.Y, xx.Z);
        yield return new Vector3(oo.X, xx.Y, oo.Z);
        yield return new Vector3(oo.X, oo.Y, xx.Z);
        yield return new Vector3(oo.X, oo.Y, oo.Z);
    }

    public Cuboid Translate(Vector3 offset)
    {
        return new Cuboid(Min + offset, Max + offset);
    }


    public bool Equals(Cuboid other)
    {
        return Min.Equals(other.Min) &&
               Max.Equals(other.Max);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Min, Max);
    }

    public static bool operator ==(Cuboid left, Cuboid right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Cuboid left, Cuboid right)
    {
        return !(left == right);
    }
}
