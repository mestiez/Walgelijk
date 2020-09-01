using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that renders a collection of vertices immediately
    /// </summary>
    public class ImmediateRenderTask : IRenderTask
    {
        public ImmediateRenderTask(Vertex[] vertices, Primitive primitiveType, Material material = null)
        {
            Vertices = vertices;
            PrimitiveType = primitiveType;
            Material = material ?? Material.DefaultTextured;
            ModelMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// The matrix to transform the vertices with
        /// </summary>
        public Matrix4x4 ModelMatrix;
        /// <summary>
        /// Vertices to draw
        /// </summary>
        public Vertex[] Vertices;
        /// <summary>
        /// Material to draw with
        /// </summary>
        public Material Material;
        /// <summary>
        /// Primitive type to draw the vertices as
        /// </summary>
        public Primitive PrimitiveType;

        public void Execute(RenderTarget target)
        {
            target.ModelMatrix = ModelMatrix;
            target.Draw(Vertices, PrimitiveType, Material);
        }
    }
}
