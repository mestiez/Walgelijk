using System.Numerics;

namespace Walgelijk.Prism;

public struct Frustum
{
    public Plane Near, Far, Top, Bottom, Left, Right;

    public Frustum(Matrix4x4 m)
    {
        Left = Plane.Normalize(new Plane(m.M14 + m.M11, m.M24 + m.M21, m.M34 + m.M31, m.M44 + m.M41));
        Right = Plane.Normalize(new Plane(m.M14 - m.M11, m.M24 - m.M21, m.M34 - m.M31, m.M44 - m.M41));
        Bottom = Plane.Normalize(new Plane(m.M14 + m.M12, m.M24 + m.M22, m.M34 + m.M32, m.M44 + m.M42));
        Top = Plane.Normalize(new Plane(m.M14 - m.M12, m.M24 - m.M22, m.M34 - m.M32, m.M44 - m.M42));
        Near = Plane.Normalize(new Plane(m.M13, m.M23, m.M33, m.M43));
        Far = Plane.Normalize(new Plane(m.M14 - m.M13, m.M24 - m.M23, m.M34 - m.M33, m.M44 - m.M43));
    }

    public readonly bool IsPointInside(Vector3 point)
    {
        if (Plane.DotCoordinate(Left, point) < 0)
            return false;
        if (Plane.DotCoordinate(Right, point) < 0)
            return false;
        if (Plane.DotCoordinate(Bottom, point) < 0)
            return false;
        if (Plane.DotCoordinate(Top, point) < 0)
            return false;
        if (Plane.DotCoordinate(Near, point) < 0)
            return false;
        if (Plane.DotCoordinate(Far, point) < 0)
            return false;
        return true;
    }

    public readonly bool IsSphereInside(Vector3 center, float radius)
    {
        if (Plane.DotCoordinate(Left, center) < -radius)
            return false;
        if (Plane.DotCoordinate(Right, center) < -radius)
            return false;
        if (Plane.DotCoordinate(Bottom, center) < -radius)
            return false;
        if (Plane.DotCoordinate(Top, center) < -radius)
            return false;
        if (Plane.DotCoordinate(Near, center) < -radius)
            return false;
        if (Plane.DotCoordinate(Far, center) < -radius)
            return false;
        return true;
    }
}
