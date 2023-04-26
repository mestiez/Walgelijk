using System.Numerics;

namespace Walgelijk.Physics
{
    internal static class ColliderExtensions
    {
        internal static Vector2 PointToLocal(this ICollider coll, Vector2 p)
            => Vector2.Transform(p, coll.Transform.WorldToLocalMatrix);

        internal static Vector2 PointToWorld(this ICollider coll, Vector2 p)
            => Vector2.Transform(p, coll.Transform.LocalToWorldMatrix);


        internal static Vector2 DirToLocal(this ICollider coll, Vector2 p)
            => Vector2.TransformNormal(p, coll.Transform.WorldToLocalMatrix);

        internal static Vector2 DirToWorld(this ICollider coll, Vector2 p)
            => Vector2.TransformNormal(p, coll.Transform.LocalToWorldMatrix);
    }
}
