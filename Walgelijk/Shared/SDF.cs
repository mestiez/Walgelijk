using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Utility struct full of SDF functions
/// </summary>
public struct SDF
{
    public static float Rectangle(Vector2 point, Vector2 offset, Vector2 size)
    {
        var d = Vector2.Abs(point - offset) - size;
        return (Vector2.Max(d, Vector2.Zero)).Length() + MathF.Min(MathF.Max(d.X, d.Y), 0);
    }

    public static float RoundedRectangle(Vector2 point, Vector2 offset, Vector2 size, float radius) => RoundedRectangle(point, offset, size, new Vector4(radius));

    public static float RoundedRectangle(Vector2 point, Vector2 offset, Vector2 size, Vector4 radii)
    {
        var rxy = (point.X > 0) ? new Vector2(radii.X, radii.Y) : new Vector2(radii.Z, radii.W);
        float rx = (point.Y > 0) ? rxy.X : rxy.Y;
        var q = Vector2.Abs(point - offset) - size + new Vector2(rx);
        return MathF.Min(MathF.Max(q.X, q.Y), 0) + (Vector2.Max(q, Vector2.Zero)).Length() - rx;
    }

    public static float OrientedBox(Vector2 p, Vector2 a, Vector2 b, float th)
    {
        float l = (b - a).Length();
        Vector2 d = (b - a) / l;
        Vector2 q = (p - (a + b) * 0.5f);
        q = new Vector2(d.X * q.X - d.Y * q.Y, d.Y * q.X + d.X * q.Y);
        q = Vector2.Abs(q) - new Vector2(l, th) * 0.5f;
        return q.Length() + Math.Min(Math.Max(q.X, q.Y), 0.0f);
    }

    public static float Triangle(Vector2 point, Vector2 offset, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        point -= offset;

        Vector2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
        Vector2 v0 = point - p0, v1 = point - p1, v2 = point - p2;
        Vector2 pq0 = v0 - e0 * Utilities.Clamp(Vector2.Dot(v0, e0) / Vector2.Dot(e0, e0), 0, 1);
        Vector2 pq1 = v1 - e1 * Utilities.Clamp(Vector2.Dot(v1, e1) / Vector2.Dot(e1, e1), 0, 1);
        Vector2 pq2 = v2 - e2 * Utilities.Clamp(Vector2.Dot(v2, e2) / Vector2.Dot(e2, e2), 0, 1);
        float s = Math.Sign(e0.X * e2.Y - e0.Y * e2.X);
        Vector2 d = Vector2.Min(Vector2.Min(new Vector2(Vector2.Dot(pq0, pq0), s * (v0.X * e0.Y - v0.Y * e0.X)),
                                             new Vector2(Vector2.Dot(pq1, pq1), s * (v1.X * e1.Y - v1.Y * e1.X))),
                                             new Vector2(Vector2.Dot(pq2, pq2), s * (v2.X * e2.Y - v2.Y * e2.X)));
        return -MathF.Sqrt(d.X) * Math.Sign(d.Y);
    }

    public static float LineSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var pa = point - a;
        var ba = b - a;
        float h = Math.Clamp(Vector2.Dot(pa, ba) / Vector2.Dot(ba, ba), 0.0f, 1.0f);
        return (pa - ba * h).Length();
    }


    public static float Circle(Vector2 point, Vector2 center, float radius)
    {
        return Vector2.Distance(point, center) - radius;
    }

    public static float Polygon(in Vector2[] v, in Vector2 p)
    {
        return Polygon(v.AsSpan(), p);
    }

    public static float Polygon(in ReadOnlySpan<Vector2> v, in Vector2 p)
    {
        float d = Vector2.Dot(p - v[0], p - v[0]);
        float s = 1.0f;
        for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
        {
            Vector2 e = v[j] - v[i];
            Vector2 w = p - v[i];
            Vector2 b = w - e * Utilities.Clamp(Vector2.Dot(w, e) / Vector2.Dot(e, e), 0.0f, 1.0f);
            d = MathF.Min(d, Vector2.Dot(b, b));
            bool c1 = p.Y >= v[i].Y;
            bool c2 = p.Y < v[j].Y;
            bool c3 = e.X * w.Y > e.Y * w.X;
            if (c1 && c2 && c3 || !c1 && !c2 && !c3) s *= -1.0f;
        }
        return s * MathF.Sqrt(d);
    }
}
