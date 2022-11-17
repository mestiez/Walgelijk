using System.Numerics;

namespace Walgelijk.Imgui
{
    public static class UiExtensions
    {
        public static Rect ToRect(this ref DrawBounds bounds)
        {
            return new Rect(bounds.Position.X, bounds.Position.Y, bounds.Position.X + bounds.Size.X, bounds.Position.Y + bounds.Size.Y);
        }

        public static bool ContainsPoint(this in DrawBounds bounds, Vector2 point)
        {
            if (!bounds.Enabled)
                return false;

            var topLeft = bounds.Position;
            var size = bounds.Size;

            return point.X > topLeft.X && point.X < topLeft.X + size.X && point.Y > topLeft.Y && point.Y < topLeft.Y + size.Y;
        }
    }
}