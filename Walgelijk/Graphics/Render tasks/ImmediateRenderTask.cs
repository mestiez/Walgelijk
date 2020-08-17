using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that renders a collection of vertices immediately
    /// </summary>
    public struct ImmediateRenderTask : IRenderTask
    {
        public ImmediateRenderTask(Vertex[] vertices, Primitive primitiveType, Material material = null)
        {
            Vertices = vertices;
            PrimitiveType = primitiveType;
            Material = material;
            ModelMatrix = Matrix4x4.Identity;
        }

        /// <summary>
        /// The matrix to transform the vertices with
        /// </summary>
        public Matrix4x4 ModelMatrix { get; set; }
        /// <summary>
        /// Vertices to draw
        /// </summary>
        public Vertex[] Vertices { get; set; }
        /// <summary>
        /// Material to draw with
        /// </summary>
        public Material Material { get; set; }
        /// <summary>
        /// Primitive type to draw the vertices as
        /// </summary>
        public Primitive PrimitiveType { get; set; }

        public void Execute(RenderTarget target)
        {
            target.ModelMatrix = ModelMatrix;
            target.Draw(Vertices, PrimitiveType, Material);
        }
    }
}
