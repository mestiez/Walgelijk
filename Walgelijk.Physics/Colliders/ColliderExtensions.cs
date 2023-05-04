using System.Numerics;

namespace Walgelijk.Physics
{
    internal static class ColliderExtensions
    {
        internal static Vector2 PointToLocal<T>(this T coll, Vector2 p) where T : ICollider
            => Vector2.Transform(p, coll.Transform.WorldToLocalMatrix);

        internal static Vector2 PointToWorld<T>(this T coll, Vector2 p) where T : ICollider
            => Vector2.Transform(p, coll.Transform.LocalToWorldMatrix);


        internal static Vector2 DirToLocal<T>(this T coll, Vector2 p) where T : ICollider
            => Vector2.TransformNormal(p, coll.Transform.WorldToLocalMatrix);

        internal static Vector2 DirToWorld<T>(this T coll, Vector2 p) where T : ICollider
            => Vector2.TransformNormal(p, coll.Transform.LocalToWorldMatrix);
    }
}
