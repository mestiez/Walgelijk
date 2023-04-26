using System;
using System.Numerics;

namespace Walgelijk.Physics;

internal struct Vector2Utils
{
    public static bool IsNan(Vector2 a) => float.IsNaN(a.X) || float.IsNaN(a.Y);

    public static Vector2 GetNearest(Vector2 point, ReadOnlySpan<Vector2> others)
    {
        float minDist = float.MaxValue;
        Vector2 nearest = new(float.NaN, float.NaN);

        foreach (var other in others)
        {
            if (IsNan(other))
                continue;

            float dist = Vector2.DistanceSquared(point, other);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = other;
            }
        }

        return nearest;
    }

    public static Vector2 GetFurthest(Vector2 point, ReadOnlySpan<Vector2> others)
    {
        float maxDist = float.MinValue;
        Vector2 nearest = new(float.NaN, float.NaN);

        foreach (var other in others)
        {
            if (IsNan(other))
                continue;

            float dist = Vector2.DistanceSquared(point, other);
            if (dist > maxDist)
            {
                maxDist = dist;
                nearest = other;
            }
        }

        return nearest;
    }
}
