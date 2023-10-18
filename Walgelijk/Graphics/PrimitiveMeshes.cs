using System;
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

        /// <summary>
        /// Right angled triangle where (0, 0) is the bottom left. Has a height of 1, a width of , and a hypotenuse of 1.414213562373095 or sqrt(2). 
        /// </summary>
        public static readonly VertexBuffer RightAngledTriangle = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(0, 0) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(0, 1) { TexCoords = new Vector2(0,1) },
                    new Vertex(1, 0) { TexCoords = new Vector2(1,0) },
                },
                new uint[]
                {
                    0, 1, 2
                }
            );

        /// <summary>
        /// Isosceles triangle where (0, 0) is the bottom left. Has a height of 1 and a width of 1. 
        /// </summary>
        public static readonly VertexBuffer IsoscelesTriangle = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(0, 0) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(0.5f, 1) { TexCoords = new Vector2(0.5f,1) },
                    new Vertex(1, 0) { TexCoords = new Vector2(1,0) },
                },
                new uint[]
                {
                    0, 1, 2
                }
            );

        /// <summary>
        /// A line segment from (0, 0) to (1, 0)
        /// </summary>
        public static readonly VertexBuffer Line = new VertexBuffer(
                new Vertex[]
                {
                    new Vertex(0, 0) { TexCoords = new Vector2(0,0) } ,
                    new Vertex(1, 0) { TexCoords = new Vector2(1,0) },
                },
                new uint[]
                {
                    0,1
                }
            )
        { PrimitiveType = Primitive.Lines };

        /// <summary>
        /// A unit circle primitive with 33 vertices
        /// </summary>
        public static readonly VertexBuffer Circle = GenerateCircle(32, 1);

        /// <summary>
        /// A unit circle primitive with 65 vertices
        /// </summary>
        public static readonly VertexBuffer Circle64 = GenerateCircle(64, 1);

        //TODO dit is een rare plek hiervoor
        /// <summary>
        /// Generates a centered circle vertex buffer
        /// </summary>
        public static VertexBuffer GenerateCircle(int edges = 32, float radius = 1)
        {
            Vertex[] vertices = new Vertex[edges + 1];
            uint[] indices = new uint[edges * 3];
            vertices[0] = new Vertex(0, 0) { TexCoords = new Vector2(0.5f, 0.5f) };

            uint n = 0;
            for (uint i = 0; i < edges; i++)
            {
                float p = i / ((float)edges - 1);
                float r = p * MathF.Tau;

                float x = MathF.Cos(r) * radius;
                float y = MathF.Sin(r) * radius;

                vertices[i + 1] = new Vertex(x, y) { TexCoords = new Vector2(x * .5f + .5f, y * .5f + .5f) };

                indices[n] = 0;
                indices[n + 1] = i + 1;
                indices[n + 2] = i + 2 > edges ? 1 : i + 2;
                n += 3;
            }

            return new VertexBuffer(vertices, indices) { PrimitiveType = Primitive.TriangleStrip };
        }

        /// <summary>
        /// Generate quad into the given arrays. Offset is the per-quad offset which is 4 for vertices and 6 for indices
        /// </summary>
        public static void GenerateQuad(Vertex[] vertices, uint[] indices, in Rect rect, int offset = 0)
        {
            int ii = offset * 4;
            vertices[0 + ii] = new Vertex(rect.MinX, rect.MinY) { TexCoords = new Vector2(0, 0) };
            vertices[1 + ii] = new Vertex(rect.MaxX, rect.MinY) { TexCoords = new Vector2(1, 0) };
            vertices[2 + ii] = new Vertex(rect.MaxX, rect.MaxY) { TexCoords = new Vector2(1, 1) };
            vertices[3 + ii] = new Vertex(rect.MinX, rect.MaxY) { TexCoords = new Vector2(0, 1) };

            int io = offset * 6;
            indices[0 + io] = 0 + (uint)ii;
            indices[1 + io] = 1 + (uint)ii;
            indices[2 + io] = 2 + (uint)ii;
            indices[3 + io] = 0 + (uint)ii;
            indices[4 + io] = 3 + (uint)ii;
            indices[5 + io] = 2 + (uint)ii;
        }
    }
}
