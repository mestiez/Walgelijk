namespace Walgelijk
{
    /// <summary>
    /// Render task that renders a vertex buffer with a material
    /// </summary>
    public struct ShapeRenderTask : IRenderTask
    {
        public ShapeRenderTask(VertexBuffer vertexBuffer, Material material = null)
        {
            VertexBuffer = vertexBuffer;
            Material = material;
        }

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
            target.Draw(VertexBuffer, Material);
        }
    }
}
