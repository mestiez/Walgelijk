namespace Walgelijk
{
    public class VertexBuffer
    {
        private Vertex[] vertices;
        private int[] indices;

        public VertexBuffer(Vertex[] vertices, int[] indices)
        {
            this.vertices = vertices;
            this.indices = indices;
        }

        public VertexBuffer(Vertex[] vertices)
        {
            Vertices = vertices;
            indices = new int[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                indices[i] = i;
        }

        public bool HasChanged { get; set; } = false;

        public Vertex[] Vertices
        {
            get => vertices; 

            set
            {
                vertices = value;
                HasChanged = true;
            }
        }

        public int[] Indices
        {
            get => indices; 

            set
            {
                indices = value;
                HasChanged = true;
            }
        }
    }
}
