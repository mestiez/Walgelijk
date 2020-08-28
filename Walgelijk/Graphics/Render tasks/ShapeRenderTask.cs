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
            ScreenSpace = false;
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
        /// <summary>
        /// Should the task set the view matrix to <see cref="Matrix4x4.Identity"/> 
        /// </summary>
        public bool ScreenSpace { get; set; }

        public void Execute(RenderTarget target)
        {
            var prev = target.ViewMatrix;

            if (ScreenSpace)
                target.ViewMatrix = Matrix4x4.Identity;

            target.ModelMatrix = ModelMatrix;
            target.Draw(VertexBuffer, Material);

            target.ViewMatrix = prev;
        }
    }
}
