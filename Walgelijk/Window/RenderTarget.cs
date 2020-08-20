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

        /// <summary>
        /// The view matrix
        /// </summary>
        public abstract Matrix4x4 ViewMatrix { get; set; }
        /// <summary>
        /// The projection matrix
        /// </summary>
        public abstract Matrix4x4 ProjectionMatrix { get; set; }
        /// <summary>
        /// The model matrix
        /// </summary>
        public abstract Matrix4x4 ModelMatrix { get; set; }

        /// <summary>
        /// Calculate the aspect ratio from the current rendertarget size. Identical to Size.Y / Size.X
        /// </summary>
        public float AspectRatio => Size.Y / Size.X;

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
        public abstract void Draw(Vertex[] vertices, Primitive primitive, Material material = null);
    }
}
