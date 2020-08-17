using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Render task that renders a vertex buffer with a material
    /// </summary>
    public struct ShapeRenderTask : IRenderTask
    {
        public ShapeRenderTask(VertexBuffer vertexBuffer, Matrix4x4 modelMatrix = default, Material material = null)
        {
            VertexBuffer = vertexBuffer;
            Material = material ?? Material.DefaultMaterial;
            ModelMatrix = modelMatrix;
        }

        /// <summary>
        /// The matrix to transform the vertices with
        /// </summary>
        public Matrix4x4 ModelMatrix { get; set; } 
        /// <summary>
        /// Vertex buffer to draw
        /// </summary>
        public VertexBuffer VertexBuffer { get; set; }
        /// <summary>
        /// Material to draw with
        /// </summary>
        public Material Material { get; set; }

        public void Execute(RenderTarget target)
        {
            target.ModelMatrix = ModelMatrix;
            target.Draw(VertexBuffer, Material);
        }
    }
}
