namespace Walgelijk.OpenTK
{
    public struct VertexBufferCacheHandles
    {
        /// <summary>
        /// Vertex buffer object index
        /// </summary>
        public int VBO;

        /// <summary>
        /// Vertex array object index
        /// </summary>
        public int VAO;

        /// <summary>
        /// Index buffer object index
        /// </summary>
        public int IBO;

        public VertexBufferCacheHandles(int vbo, int vao, int ibo)
        {
            VBO = vbo;
            VAO = vao;
            IBO = ibo;
        }
    }
}
