using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.Physics
{
    public interface ICollider
    {
        /// <summary>
        /// Shape identifier of the collider
        /// </summary>
        public Shape Shape { get; }

        /// <summary>
        /// The axis-aligned worldspace collider bounds
        /// </summary>
        public Rect Bounds { get; }

        /// <summary>
        /// Owning transform
        /// </summary>
        public TransformComponent Transform { get; set; }

        /// <summary>
        /// Returns if the given point is inside or on the collider
        /// </summary>
        public bool IsPointInside(Vector2 p);

        /// <summary>
        /// Returns the nearest point on (or in) the collider from the given point
        /// </summary>
        public Vector2 GetNearestPoint(Vector2 p);

        /// <summary>
        /// Returns the nearest normal vector on (or in) the collider
        /// </summary>
        public Vector2 SampleNormal(Vector2 p);

        /// <summary>
        /// Recalculates the <see cref="Bounds"/> property. This is automatically done at the beginning of each frame by the <see cref="PhysicsSystem"/>
        /// </summary>
        public void RecalculateBounds();

        /// <summary>
        /// Returns an enumerable collection of all intersections between the given line (not line segment) and the collider
        /// </summary>
        public IEnumerable<Vector2> GetLineIntersections(Geometry.Ray ray);
    }
}
