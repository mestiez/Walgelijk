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
        /// Create a rectangle
        /// </summary>
        public Rect(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;

            MaxX = maxX;
            MaxY = maxY;
        }
    }
}
