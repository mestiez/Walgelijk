using System.Numerics;

namespace Walgelijk
{
    public abstract class RenderTarget
    {
        /// <summary>
        /// Size of the target. This should be automatically set to the window size
        /// </summary>
        public abstract Vector2 Size { get; set; }

        /// <summary>
        /// Colour to clear with
        /// </summary>
        public abstract Color ClearColour { get; set; }

        /// <summary>
        /// Clear target
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Draw a vertex buffer
        /// </summary>
        /// <param name="vertexBuffer">VertexBuffer to draw</param>
        /// <param name="material">Material to draw it with</param>
        public abstract void Draw(VertexBuffer vertexBuffer, Material material);
    }
}
