using System.Numerics;

namespace Walgelijk.SimpleDrawing
{
    public static class DrawingPrimitives
    {
        /// <summary>
        /// Circle with 64 edges
        /// </summary>
        public static readonly VertexBuffer Circle = PrimitiveMeshes.Circle64;

        /// <summary>
        /// Top left quad
        /// </summary>
        public static readonly VertexBuffer Quad = new VertexBuffer(
                new Vertex[]
                {
                    /* bottom left
                     * bottom right
                     * top right
                     * top left
                     */
                    new Vertex(0, -1) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(1, -1) { TexCoords = new Vector2(1,0) },
                    new Vertex(1, 0) { TexCoords = new Vector2(1,1) },
                    new Vertex(0, 0) { TexCoords = new Vector2(0,1) },
                },
                new uint[]
                {
                    0,1,2,
                    0,3,2
                }
            );

        /// <summary>
        /// Top left isosceles triangle
        /// </summary>
        public static readonly VertexBuffer IsoscelesTriangle = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(0, -1) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(0.5f, 0) { TexCoords = new Vector2(0.5f,1) },
                    new Vertex(1, -1) { TexCoords = new Vector2(1,0) },
                },
                new uint[]
                {
                    0, 1, 2
                }
            );

        /// <summary>
        /// Centered isosceles triangle
        /// </summary>
        public static readonly VertexBuffer CenteredIsoscelesTriangle = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(-0.5f, -0.5f) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(0, 0.5f) { TexCoords = new Vector2(0.5f,1) },
                    new Vertex(0.5f, -0.5f) { TexCoords = new Vector2(1,0) },
                },
                new uint[]
                {
                    0, 1, 2
                }
            );

        /// <summary>
        /// Top left right angled triangle
        /// </summary>
        public static readonly VertexBuffer RightAngledTriangle = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(0, -1) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(1, -1) { TexCoords = new Vector2(1,0) },
                    new Vertex(0, 0) { TexCoords = new Vector2(1,1) },
                },
                new uint[]
                {
                    0, 1, 2
                }
            );
    }
}
