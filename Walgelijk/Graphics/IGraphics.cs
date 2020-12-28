namespace Walgelijk
{
    /// <summary>
    /// Graphics utility interface meant to be implemented by the rendering implementation
    /// </summary>
    public interface IGraphics
    {
        /// <summary>
        /// Clear current target
        /// </summary>
        public void Clear(Color color);

        /// <summary>
        /// Draw a vertex buffer to the currently activated target
        /// </summary>
        /// <param name="vertexBuffer">VertexBuffer to draw</param>
        /// <param name="material">Material to draw it with</param>
        public void Draw(VertexBuffer vertexBuffer, Material material = null);

        /// <summary>
        /// Draw a instanced vertex buffer to the currently activated target
        /// </summary>
        /// <param name="vertexBuffer">VertexBuffer to draw</param>
        /// <param name="instanceCount">Amount of elements to draw</param>
        /// <param name="material">Material to draw it with</param>
        public void DrawInstanced(VertexBuffer vertexBuffer, int instanceCount, Material material = null);

        /// <summary>
        /// Set a shader program uniform
        /// </summary>
        void SetUniform(Material material, string uniformName, object data);

        /// <summary>
        /// Drawing bounds settings 
        /// </summary>
        public DrawBounds DrawBounds { get; set; }

        /// <summary>
        /// Set or get the currently active target
        /// </summary>
        public RenderTarget CurrentTarget { get; set; }
    }
}
