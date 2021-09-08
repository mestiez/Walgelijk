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

        /// <summary>
        /// User added vertex buffer object indices
        /// </summary>
        public int[] ExtraVBO;

        public VertexBufferCacheHandles(int vbo, int vao, int ibo, int[] extraVbos = null)
        {
            VBO = vbo;
            VAO = vao;
            IBO = ibo;

            ExtraVBO = extraVbos;
        }
    }
}
