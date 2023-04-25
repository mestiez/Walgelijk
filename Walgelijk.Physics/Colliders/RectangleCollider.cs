using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.Physics;

public struct RectangleCollider : ICollider
{
    public Shape Shape => Shape.Rectangle;
    public TransformComponent Transform { get; set; }

    public Rect Bounds { get; private set; }

    public Vector2 Size;
    public Vector2 Offset;

    public RectangleCollider(TransformComponent owner, Vector2 size = default, Vector2 offset = default)
    {
        Transform = owner;
        Size = size;
        Offset = offset;
        Bounds = default;
        RecalculateBounds();
    }

    public void RecalculateBounds()
    {
        var a = this.PointToWorld(Offset + new Vector2(-Size.X, -Size.Y) * 0.5f);
        var b = this.PointToWorld(Offset + new Vector2(Size.X, Size.Y) * 0.5f);
        var c = this.PointToWorld(Offset + new Vector2(-Size.X, Size.Y) * 0.5f);
        var d = this.PointToWorld(Offset + new Vector2(Size.X, -Size.Y) * 0.5f);
        Bounds = new Rect(a, Vector2.Zero).StretchToContain(b).StretchToContain(c).StretchToContain(d);
    }

    public bool IsPointInside(Vector2 point)
    {
        //No need to check of bounds (yet) 
        //TODO als je rotatie toevoegt dan moet je wel checken voor axis-aligned bounds

        var transformed = this.PointToLocal(point);

        if (transformed.X > Size.X / 2 + Offset.X)
            return false;

        if (transformed.X < -Size.X / 2 + Offset.X)
            return false;

        if (transformed.Y > Size.Y / 2 + Offset.Y)
            return false;

        if (transformed.Y < -Size.Y / 2 + Offset.Y)
            return false;

        return true;
    }

    public Vector2 GetNearestPoint(Vector2 point)
    {
        var transformed = this.PointToLocal(point);
        var p = GetNearestLocal(transformed);

        return this.PointToWorld(p);
    }

    public Vector2 SampleNormal(Vector2 point)
    {
        var transformed = this.PointToLocal(point);
        var p = GetNearestLocal(transformed);

        p.X -= Offset.X;
        p.Y -= Offset.Y;

        p.X /= Size.X;
        p.Y /= Size.Y;

        return getLocalNormal();// Vector2.Normalize(this.DirToWorld(getLocalNormal()));

        Vector2 getLocalNormal()
        {
            if (p.X > MathF.Abs(p.Y))
                return Vector2.UnitX;

            if (p.X < -MathF.Abs(p.Y))
                return -Vector2.UnitX;

            if (p.Y > MathF.Abs(p.X))
                return Vector2.UnitY;

            if (p.Y < -MathF.Abs(p.X))
                return -Vector2.UnitY;

            return default;
        }
    }

    private Vector2 GetNearestLocal(Vector2 local)
    {
        return new Vector2(
            Utilities.Clamp(local.X, -Size.X / 2 + Offset.X, Size.X / 2 + Offset.X),
            Utilities.Clamp(local.Y, -Size.Y / 2 + Offset.Y, Size.Y / 2 + Offset.Y)
            );
    }

    public IEnumerable<Vector2> GetLineIntersections(Geometry.Ray ray)
    {
        //TODO transformations.. op de een of andere manier
        int returned = 0;

        var leftside = new Geometry.LineSegment(Bounds.MinX, Bounds.MaxY, Bounds.MinX, Bounds.MinY);
        if (Geometry.TryGetIntersection(ray, leftside, out var i1))
        {
            returned++;
            yield return i1;
        }

        var rightside = new Geometry.LineSegment(Bounds.MaxX, Bounds.MaxY, Bounds.MaxX, Bounds.MinY);
        if (Geometry.TryGetIntersection(ray, rightside, out var i2))
        {
            returned++;
            yield return i2;
            if (returned == 2)
                yield break;
        }

        var topside = new Geometry.LineSegment(Bounds.MinX, Bounds.MaxY, Bounds.MaxX, Bounds.MaxY);
        if (Geometry.TryGetIntersection(ray, topside, out var i3))
        {
            returned++;
            yield return i3;
            if (returned == 2)
                yield break;
        }

        var bottomside = new Geometry.LineSegment(Bounds.MinX, Bounds.MinY, Bounds.MaxX, Bounds.MinY);
        if (Geometry.TryGetIntersection(ray, bottomside, out var i4))
            yield return i4;
    }
}
