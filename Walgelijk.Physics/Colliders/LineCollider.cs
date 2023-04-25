using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.Physics;

public struct LineCollider : ICollider
{
    public Shape Shape => Shape.Line;
    public TransformComponent Transform { get; set; }
    public Rect Bounds { get; private set; }

    public Vector2 Start;
    public Vector2 End;
    public float Width;

    public LineCollider(TransformComponent transform, Vector2 start, Vector2 end, float width)
    {
        Start = start;
        End = end;
        Width = width;
        Bounds = default;
        Transform = transform;
        RecalculateBounds();
    }

    public IEnumerable<Vector2> GetLineIntersections(Geometry.Ray ray)
    {
        var a = this.PointToWorld(Start);
        var b = this.PointToWorld(End);

        var s = new Geometry.LineSegment(a, b);
        if (Geometry.TryGetIntersection(ray, s, Width / 2, out var i1, out var i2))
        {
            yield return i1;
            yield return i2;
        }
        yield break;
    }

    public Vector2 GetNearestPoint(Vector2 p)
    {
        var a = this.PointToWorld(Start);
        var b = this.PointToWorld(End);

        var dir = -SampleNormal(p);
        var dist = MathF.Max(0, SDF.LineSegment(p, a, b) - Width / 2);
        var v = p + dir * dist;
        return v;
    }

    public bool IsPointInside(Vector2 p)
    {
        if (!Bounds.ContainsPoint(p))
            return false;

        var a = this.PointToWorld(Start);
        var b = this.PointToWorld(End);

        return (SDF.LineSegment(p, a, b) < Width / 2);
    }

    public void RecalculateBounds()
    {
        var a = this.PointToWorld(Start);
        var b = this.PointToWorld(End);
        float w = Width / 2;

        Bounds =
            new Rect(MathF.Min(a.X, b.X) - w,
                     MathF.Min(a.Y, b.Y) - w,

                     MathF.Max(a.X, b.X) + w,
                     MathF.Max(a.Y, b.Y) + w);
    }

    public Vector2 SampleNormal(Vector2 p)
    {
        var a = this.PointToWorld(Start);
        var b = this.PointToWorld(End);

        const float offset = 0.01f;

        var left = SDF.LineSegment(new Vector2(p.X - offset, p.Y), a, b);
        var right = SDF.LineSegment(new Vector2(p.X + offset, p.Y), a, b);
        var up = SDF.LineSegment(new Vector2(p.X, p.Y + offset), a, b);
        var down = SDF.LineSegment(new Vector2(p.X, p.Y - offset), a, b);

        var normal = Vector2.Normalize(new Vector2(
                right - left,
                up - down
            ));

        return normal;
    }
}
