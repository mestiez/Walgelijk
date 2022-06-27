using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.Physics
{
    public struct CircleCollider : ICollider
    {
        public Shape Shape => Shape.Circle;
        public TransformComponent Transform { get; set; }

        public Rect Bounds { get; private set; }

        public float Radius;

        public CircleCollider(TransformComponent owner, float radius = 0.5f)
        {
            Transform = owner;
            Radius = radius;
            Bounds = default;
            RecalculateBounds();
        }

        public void RecalculateBounds()
        {
            var min = this.PointToWorld(new Vector2(-Radius));
            var max = this.PointToWorld(new Vector2(Radius));

            Bounds = new Rect(
                min.X, min.Y,
                max.X, max.Y
                );
        }

        public bool IsPointInside(Vector2 point)
        {
            if (!Geometry.IsPointInRect(point.X, point.Y, Bounds))
                return false;

            var transformed = this.PointToLocal(point);
            return transformed.LengthSquared() < Radius * Radius;
        }

        public Vector2 GetNearestPoint(Vector2 point)
        {
            var transformed = this.PointToLocal(point);
            var r = transformed.Length();
            var p = r > Radius ? transformed / r * Radius : transformed;
            return this.PointToWorld(p);
        }

        public Vector2 SampleNormal(Vector2 point)
        {
            var transformed = this.PointToLocal(point);
            return Vector2.Normalize(this.DirToWorld(transformed));
        }

        public IEnumerable<Vector2> GetLineIntersections(Geometry.Ray ray)
        {
            //TODO transformations.. op de een of andere manier

            if (!Geometry.TryGetIntersection(ray, Bounds, out _, out _))
                yield break;

            if (Geometry.TryGetIntersection(ray, new Geometry.Circle(Transform.Position, Radius), out var i1, out var i2))
            {
                yield return i1;
                yield return i2;
            }
        }
    }
}
