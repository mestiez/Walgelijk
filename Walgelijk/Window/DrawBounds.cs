using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Drawing bounds structure
    /// </summary>
    public struct DrawBounds : IEquatable<DrawBounds>
    {
        /// <summary>
        /// Size of the rectangle in pixels
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// Top left corner of the rectangle in pixels
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// Activate / deactivate the drawing bounds
        /// </summary>
        public bool Enabled;

        /// <summary>
        /// Construct draw bounds settings
        /// </summary>
        public DrawBounds(Vector2 size, Vector2 position, bool enabled = true)
        {
            Size = size;
            Position = position;
            Enabled = enabled;
        }

        /// <summary>
        /// Construct draw bounds settings
        /// </summary>
        public DrawBounds(Rect rect, bool enabled = true)
        {
            Size = rect.GetSize();
            Position = rect.BottomLeft;
            Enabled = enabled;
        }

        /// <summary>
        /// Returns an instance that disables the drawbounds
        /// </summary>
        public static DrawBounds DisabledBounds => new DrawBounds(default, default, false);

        public bool Equals(DrawBounds other)
        {
            return Size.Equals(other.Size) && Position.Equals(other.Position) && Enabled == other.Enabled;
        }

        public override bool Equals(object? obj)
        {
            return obj is DrawBounds other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Size, Position, Enabled);
        }
    }
}
