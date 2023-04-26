using System;
using System.Globalization;
using System.Numerics;

namespace Walgelijk.Physics;

public struct Geometry
{
    public static bool TryGetIntersection(Ray ray, Rect rect, out Vector2? intersectionA, out Vector2? intersectionB)
    {
        Vector2 placeholderIntersection;
        intersectionA = null;
        intersectionB = null;

        var side1 = new LineSegment(rect.MinX, rect.MaxY, rect.MaxX, rect.MaxY); //maxY
        var side2 = new LineSegment(rect.MinX, rect.MinY, rect.MaxX, rect.MinY); //minY
        var side3 = new LineSegment(rect.MinX, rect.MinY, rect.MinX, rect.MaxY); //minX
        var side4 = new LineSegment(rect.MaxX, rect.MinY, rect.MaxX, rect.MaxY); //maxX

        if (TryGetIntersection(ray.Line, side1, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        if (TryGetIntersection(ray.Line, side2, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        if (intersectionA.HasValue && intersectionB.HasValue)
            return true;

        if (TryGetIntersection(ray.Line, side3, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        if (intersectionA.HasValue && intersectionB.HasValue)
            return true;

        if (TryGetIntersection(ray.Line, side4, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        return (intersectionA.HasValue && intersectionB.HasValue);
    }

    //TODO dit kan beter he
    public static bool TryGetIntersection(LineSegment line, Rect rect, out Vector2? intersectionA, out Vector2? intersectionB)
    {
        Vector2 placeholderIntersection;
        intersectionA = null;
        intersectionB = null;

        var side1 = new LineSegment(rect.MinX, rect.MaxY, rect.MaxX, rect.MaxY); //maxY
        var side2 = new LineSegment(rect.MinX, rect.MinY, rect.MaxX, rect.MinY); //minY
        var side3 = new LineSegment(rect.MinX, rect.MinY, rect.MinX, rect.MaxY); //minX
        var side4 = new LineSegment(rect.MaxX, rect.MinY, rect.MaxX, rect.MaxY); //maxX

        if (TryGetIntersection(line, side1, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        if (TryGetIntersection(line, side2, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        if (intersectionA.HasValue && intersectionB.HasValue)
            return true;

        if (TryGetIntersection(line, side3, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        if (intersectionA.HasValue && intersectionB.HasValue)
            return true;

        if (TryGetIntersection(line, side4, out placeholderIntersection))
            if (intersectionA.HasValue)
                intersectionB = placeholderIntersection;
            else
                intersectionA = placeholderIntersection;
        return (intersectionA.HasValue && intersectionB.HasValue);
    }

    public static bool TryGetIntersection(LineSegment L1, LineSegment L2, out Vector2 intersection)
    {
        L1.GetLinearEquation(out var A1, out var B1, out var C1);
        L2.GetLinearEquation(out var A2, out var B2, out var C2);

        float d = A1 * B2 - A2 * B1;

        if (Math.Abs(d) > float.Epsilon)
        {
            intersection = new Vector2(
                (B2 * C1 - B1 * C2) / d,
                (A1 * C2 - A2 * C1) / d);

            return IsPointOnLine(intersection.X, intersection.Y, L1) && IsPointOnLine(intersection.X, intersection.Y, L2);
        }

        intersection = new Vector2(float.NaN, float.NaN);
        return false;
    }

    public static bool TryGetIntersection(Line L1, LineSegment L2, out Vector2 intersection)
    {
        L2.GetLinearEquation(out var A2, out var B2, out var C2);

        float d = L1.A * B2 - A2 * L1.B;

        if (Math.Abs(d) > float.Epsilon)
        {
            intersection = new Vector2(
                (B2 * L1.C - L1.B * C2) / d,
                (L1.A * C2 - A2 * L1.C) / d);

            return IsPointOnLine(intersection.X, intersection.Y, L2);
        }

        intersection = new Vector2(float.NaN, float.NaN);
        return false;
    }

    public static bool TryGetIntersection(Ray ray, LineSegment L2, out Vector2 intersection)
    {
        var L1 = ray.Line;
        L2.GetLinearEquation(out var A2, out var B2, out var C2);

        float d = L1.A * B2 - A2 * L1.B;

        if (Math.Abs(d) > float.Epsilon)
        {
            intersection = new Vector2(
                (B2 * L1.C - L1.B * C2) / d,
                (L1.A * C2 - A2 * L1.C) / d);

            return IsPointOnLine(intersection.X, intersection.Y, L2) && Vector2.Dot(intersection - ray.Origin, ray.Direction) > 0;
        }

        intersection = new Vector2(float.NaN, float.NaN);
        return false;
    }

    public static bool TryGetIntersection(Ray ray, LineSegment L2, float radius, out Vector2 intersection1, out Vector2 intersection2)
    {
        var perpendicular = Vector2.Normalize(L2.A - L2.B);
        perpendicular = new Vector2(-perpendicular.Y, perpendicular.X) * radius;

        //TODO dit kan sneller
        var v = new Vector2[] {
            new(float.NaN),
            new(float.NaN),
            new(float.NaN),
            new(float.NaN),
            new(float.NaN),
            new(float.NaN)
        };

        var startsInside = SDF.LineSegment(ray.Origin, L2.A, L2.B) - radius < 0;

        if (!TryGetIntersection(ray, new LineSegment(L2.A + perpendicular, L2.B + perpendicular), out v[0]))
            v[0] = new Vector2(float.NaN);

        if (!TryGetIntersection(ray, new LineSegment(L2.A - perpendicular, L2.B - perpendicular), out v[1]))
            v[1] = new Vector2(float.NaN);

        if (!TryGetIntersection(ray, new Circle(L2.A, radius), out v[2], out v[3]))
            v[2] = v[3] = new Vector2(float.NaN);
        else if (startsInside)
        {
            v[2] = Vector2Utils.GetFurthest(ray.Origin, v.AsSpan(2,2));
            v[3] = new Vector2(float.NaN);
        }

        if (!TryGetIntersection(ray, new Circle(L2.B, radius), out v[4], out v[5]))
            v[4] = v[5] = new Vector2(float.NaN);
        else if (startsInside)
        {
            v[4] = Vector2Utils.GetFurthest(ray.Origin, v.AsSpan(4, 2));
            v[5] = new Vector2(float.NaN);
        }

        intersection1 = Vector2Utils.GetNearest(ray.Origin, v);
        intersection2 = Vector2Utils.GetFurthest(ray.Origin, v);

        return !Vector2Utils.IsNan(intersection1) && !Vector2Utils.IsNan(intersection2);
    }

    public static bool TryGetIntersection(Line L1, Line L2, out Vector2 intersection)
    {
        float d = L1.A * L2.B - L2.A * L1.B;

        if (Math.Abs(d) > float.Epsilon)
        {
            intersection = new Vector2(
                (L2.B * L1.C - L1.B * L2.C) / d,
                (L1.A * L2.C - L2.A * L1.C) / d);

            return true;
        }

        intersection = new Vector2(float.NaN, float.NaN);
        return false;
    }

    public static bool TryGetIntersection(Ray ray, Circle circle, out Vector2 intersection1, out Vector2 intersection2)
    {
        intersection1 = new Vector2(float.NaN, float.NaN);
        intersection2 = new Vector2(float.NaN, float.NaN);

        var e = ray.Direction;
        var h = circle.Center - ray.Origin;
        float lf = Vector2.Dot(e, h);
        float s = circle.Radius * circle.Radius - h.LengthSquared() + lf * lf;
        if (s < 0)
            return false;

        s = MathF.Sqrt(s);

        int results = 0;
        if (lf < s)
        {
            if (lf + s >= 0)
            {
                s = -s;
                results = 1;
            }
        }
        else
            results = 2;

        intersection1 = e * (lf - s) + ray.Origin;
        intersection2 = e * (lf + s) + ray.Origin;

        return results > 0;
    }

    public static bool IsPointInRect(float x, float y, Rect rect) => x > rect.MinX && x < rect.MaxX && y > rect.MinY && y < rect.MaxY;

    public static bool IsPointOnLine(float x, float y, LineSegment l)
    {
        return
            MathF.Min(l.X1, l.X2) - 0.001f <= x && x - 0.001f <= MathF.Max(l.X1, l.X2) &&
            MathF.Min(l.Y1, l.Y2) - 0.001f <= y && y - 0.001f <= MathF.Max(l.Y1, l.Y2);
    }

    public struct Line
    {
        public float A;
        public float B;
        public float C;

        public Line(float a, float b, float c)
        {
            A = a;
            B = b;
            C = c;
        }

        public Vector2 GetDirection() => Vector2.Normalize(new(B, -A));
    }

    public struct Circle
    {
        public Vector2 Center;
        public float Radius;

        public Circle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }
    }

    public struct Ray
    {
        public readonly Vector2 Direction;
        public readonly Vector2 Origin;

        public readonly Line Line;

        public Ray(Vector2 origin, Vector2 direction)
        {
            Direction = direction;
            Origin = origin;
            Line = new LineSegment(origin, origin + direction).ToLine();
        }
    }

    public struct LineSegment
    {
        public float X1, X2, Y1, Y2;

        public LineSegment(float x1, float y1, float x2, float y2)
        {
            X1 = x1;
            Y1 = y1;

            X2 = x2;
            Y2 = y2;
        }

        public LineSegment(Vector2 a, Vector2 b)
        {
            X1 = a.X;
            Y1 = a.Y;

            X2 = b.X;
            Y2 = b.Y;
        }

        public Vector2 A => new Vector2(X1, Y1);
        public Vector2 B => new Vector2(X2, Y2);

        public readonly void GetLinearEquation(out float a, out float b, out float c)
        {
            a = Y2 - Y1;
            b = X1 - X2;
            c = a * X1 + b * Y1;
        }

        public readonly Line ToLine()
        {
            GetLinearEquation(out var a, out var b, out var c);
            return new Line(a, b, c);
        }
    }
}
