using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Simple rectangle structure
    /// </summary>
    public struct Rect
    {
        /// <summary>
        /// Minimum X point
        /// </summary>
        public float MinX;
        /// <summary>
        /// Minimum Y point
        /// </summary>
        public float MinY;
        /// <summary>
        /// Maximum X point
        /// </summary>
        public float MaxX;
        /// <summary>
        /// Maximum Y point
        /// </summary>
        public float MaxY;

        /// <summary>
        /// Width of rectangle
        /// </summary>
        public float Width
        {
            get => MaxX - MinX;
            set => MaxX = MinX + value;
        }

        /// <summary>
        /// Height of rectangle
        /// </summary>
        public float Height
        {
            get => MaxY - MinY;
            set => MaxY = MinY + value;
        }

        /// <summary>
        /// The top right of the rectangle
        /// </summary>
        public Vector2 TopRight => new(MaxX, MaxY);

        /// <summary>
        /// The bottom right of the rectangle
        /// </summary>
        public Vector2 BottomRight => new(MaxX, MinY);

        /// <summary>
        /// The top left of the rectangle
        /// </summary>
        public Vector2 TopLeft => new(MinX, MaxY);

        /// <summary>
        /// The bottom left of the rectangle
        /// </summary>
        public Vector2 BottomLeft => new(MinX, MinY);

        /// <summary>
        /// Returns the center of the rectangle. Calculated using (min + max) * 0.5
        /// </summary>
        public readonly Vector2 GetCenter() => new(
                (MinX + MaxX) * 0.5f,
                (MinY + MaxY) * 0.5f
                );
        /// <summary>
        /// Returns the size of the rectangle. Calculated using (max - min)
        /// </summary>
        public readonly Vector2 GetSize() => new(MaxX - MinX, MaxY - MinY);     
        
        /// <summary>
        /// Returns a random point inside this rectangle
        /// </summary>
        public readonly Vector2 GetRandomPoint() => new(Utilities.RandomFloat(MinX, MaxX), Utilities.RandomFloat(MinY, MaxY));

        /// <summary>
        /// Create a rectangle
        /// </summary>
        public Rect(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;

            MaxX = maxX;
            MaxY = maxY;
        }

        /// <summary>
        /// Create a rectangle given the center and the size
        /// </summary>
        public Rect(Vector2 center, Vector2 size)
        {
            var halfSize = size / 2;
            MinX = center.X - halfSize.X;
            MinY = center.Y - halfSize.Y;

            MaxX = center.X + halfSize.X;
            MaxY = center.Y + halfSize.Y;
        }

        /// <summary>
        /// Does the rectangle contain the given point?
        /// </summary>
        public readonly bool ContainsPoint(Vector2 point) =>
            point.X > MinX && point.X < MaxX && point.Y > MinY && point.Y < MaxY;

        /// <summary>
        /// Does the rectangle, optionally expanded, contain the given point?
        /// </summary>
        public readonly bool ContainsPoint(Vector2 point, float expand) =>
            point.X > MinX - expand && point.X < MaxX + expand && point.Y > MinY - expand && point.Y < MaxY + expand;

        /// <summary>
        /// Does the rectangle overlap with the given rectangle?
        /// </summary>
        public readonly bool IntersectsRectangle(Rect b) => !(MaxX < b.MinX || MinX > b.MaxX || MinY > b.MaxY || MaxY < b.MinY);

        /// <summary>
        /// Returns the point on the rectangle that is closest to the given point
        /// </summary>
        public readonly Vector2 ClosestPoint(Vector2 point) => new(
                Utilities.Clamp(point.X, MinX, MaxX),
                Utilities.Clamp(point.Y, MinY, MaxY)
                );

        /// <summary>
        /// This will return a copy of this rectangle that is stretched just enough to contain the given point
        /// </summary>
        public readonly Rect StretchToContain(Vector2 point)
        {
            return new Rect(
                MathF.Min(MinX, point.X),
                MathF.Min(MinY, point.Y), 
                MathF.Max(MaxX, point.X), 
                MathF.Max(MaxY, point.Y));
        }
    }
}
