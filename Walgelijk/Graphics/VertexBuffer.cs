namespace Walgelijk
{
    /// <summary>
    /// Holds all the data needed to draw vertices to the screen
    /// </summary>
    public class VertexBuffer
    {
        private Vertex[] vertices;
        private uint[] indices;

        /// <summary>
        /// The way vertices are drawn
        /// </summary>
        public Primitive PrimitiveType { get; set; } = Primitive.Triangles;

        /// <summary>
        /// Create a VertexBuffer with the specified vertices and indices
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        public VertexBuffer(Vertex[] vertices, uint[] indices)
        {
            this.vertices = vertices;
            this.indices = indices;
        }

        /// <summary>
        /// Create a VertexBuffer with the specified vertices. The indices will be set automatically
        /// </summary>
        /// <param name="vertices"></param>
        public VertexBuffer(Vertex[] vertices)
        {
            Vertices = vertices;
            indices = new uint[vertices.Length];
            for (uint i = 0; i < vertices.Length; i++)
                indices[i] = i;
        }

        /// <summary>
        /// Whether the data needs to be uploaded to the GPU again
        /// </summary>
        public bool HasChanged { get; set; } = false;

        /// <summary>
        /// Vertices to draw
        /// </summary>
        public Vertex[] Vertices
        {
            get => vertices; 

            set
            {
                vertices = value;
                HasChanged = true;
            }
        }

        /// <summary>
        /// Indices to draw vertices by
        /// </summary>
        public uint[] Indices
        {
            get => indices; 

            set
            {
                indices = value;
                HasChanged = true;
            }
        }

        /// <summary>
        /// Amount of indices
        /// </summary>
        public int IndexCount => Indices?.Length ?? 0;

        /// <summary>
        /// Amount of vertices
        /// </summary>
        public int VertexCount => Vertices?.Length ?? 0;

        /// <summary>
        /// Force the data to be reuploaded to the GPU
        /// </summary>
        public void ForceUpdate()
        {
            HasChanged = true;
        }
    }
}
