using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// A target that can be rendered to
    /// </summary>
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

        public abstract Matrix4x4 ViewMatrix { get; set; }
        public abstract Matrix4x4 ProjectionMatrix { get; set; }
        public abstract Matrix4x4 ModelMatrix { get; set; }

        /// <summary>
        /// Clear target
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Draw a vertex buffer
        /// </summary>
        /// <param name="vertexBuffer">VertexBuffer to draw</param>
        /// <param name="material">Material to draw it with</param>
        public abstract void Draw(VertexBuffer vertexBuffer, Material material = null);

        /// <summary>
        /// Draw vertices immediately
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="material"></param>
        public abstract void Draw(Vertex[] vertices, Primitive primitive, Material material = null);
    }
}
