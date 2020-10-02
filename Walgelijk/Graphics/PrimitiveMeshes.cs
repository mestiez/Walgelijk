using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Static class that holds primitive <see cref="VertexBuffer"/> instances. Use these instances instead of creating new ones.
    /// </summary>
    public static class PrimitiveMeshes
    {
        /// <summary>
        /// Unit quad where (0, 0) is the center
        /// </summary>
        public static readonly VertexBuffer CenteredQuad = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(-0.5f, -0.5f) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(0.5f, -0.5f)  { TexCoords = new Vector2(1,0) },
                    new Vertex(0.5f, 0.5f)   { TexCoords = new Vector2(1,1) },
                    new Vertex(-0.5f, 0.5f)  { TexCoords = new Vector2(0,1) },
                },
                new uint[]
                {
                    0,1,2,
                    0,3,2
                }
            );

        /// <summary>
        /// Unit quad where (0, 0) is the bottom left
        /// </summary>
        public static readonly VertexBuffer Quad = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(0, 0) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(1, 0) { TexCoords = new Vector2(1,0) },
                    new Vertex(1, 1) { TexCoords = new Vector2(1,1) },
                    new Vertex(0, 1) { TexCoords = new Vector2(0,1) },
                },
                new uint[]
                {
                    0,1,2,
                    0,3,2
                }
            );
    }
}
