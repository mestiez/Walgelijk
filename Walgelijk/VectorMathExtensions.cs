using System.Numerics;
using System.Runtime.CompilerServices;

namespace Walgelijk;

public static class VectorMathExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ProjectOnPlane(this Vector3 vector, Vector3 planeNormal)
    {
        float sqrMag = Vector3.Dot(planeNormal, planeNormal);
        if (sqrMag < float.Epsilon)
            return vector;
        else
        {
            var dot = Vector3.Dot(vector, planeNormal);
            return new Vector3(vector.X - planeNormal.X * dot / sqrMag,
                vector.Y - planeNormal.Y * dot / sqrMag,
                vector.Z - planeNormal.Z * dot / sqrMag);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ProjectOnPlane(this Vector2 vector, Vector2 planeNormal)
    {
        float sqrMag = Vector2.Dot(planeNormal, planeNormal);
        if (sqrMag < float.Epsilon)
            return vector;
        else
        {
            var dot = Vector2.Dot(vector, planeNormal);
            return new Vector2(vector.X - planeNormal.X * dot / sqrMag,
                vector.Y - planeNormal.Y * dot / sqrMag);
        }
    }
}
