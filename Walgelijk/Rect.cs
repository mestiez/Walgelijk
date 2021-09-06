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
        /// Returns the center of the rectangle. Calculated using (min + max) * 0.5
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCenter()
        {
            return new Vector2(
                (MinX + MaxX) * 0.5f,
                (MinY + MaxY) * 0.5f
                );
        }

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
    }
}
