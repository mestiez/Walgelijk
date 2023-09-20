using System.Numerics;

namespace Walgelijk.Prism;

public static class PrismPrimitives
{
    public static void GenerateQuad(Vector2 size, out Vertex[] vertices, out uint[] indices)
    {
        var halfSize = size / 2;

        vertices = new Vertex[]
        {
            new Vertex(-halfSize.X, -halfSize.Y, 0){ TexCoords = new Vector2(-1, -1) },
            new Vertex(halfSize.X, -halfSize.Y, 0){ TexCoords = new Vector2(1, -1) },
            new Vertex(-halfSize.X, halfSize.Y, 0){ TexCoords = new Vector2(-1, 1) },
            new Vertex(halfSize.X, halfSize.Y, 0){ TexCoords = new Vector2(1, 1) },
        };

        indices = new uint[]
        {
            0, 1, 2,
            1, 3, 2
        };
    }

    public static void GenerateCuboid(Vector3 size, out Vertex[] vertices, out uint[] indices)
    {
        var halfSize = size / 2;

        vertices = new Vertex[]
        {
            new Vertex(-halfSize.X, -halfSize.Y, -halfSize.Z),
            new Vertex(halfSize.X, -halfSize.Y, -halfSize.Z),
            new Vertex(-halfSize.X, halfSize.Y, -halfSize.Z),
            new Vertex(halfSize.X, halfSize.Y, -halfSize.Z),
            new Vertex(-halfSize.X, -halfSize.Y, halfSize.Z),
            new Vertex(halfSize.X, -halfSize.Y, halfSize.Z),
            new Vertex(-halfSize.X, halfSize.Y, halfSize.Z),
            new Vertex(halfSize.X, halfSize.Y, halfSize.Z),
        };

        // Indices for 12 triangles, two for each face of the cuboid
        indices = new uint[]
        {
            0, 1, 2, 1, 3, 2, // Front face
            1, 5, 3, 5, 7, 3, // Right face
            5, 4, 7, 4, 6, 7, // Back face
            4, 0, 6, 0, 2, 6, // Left face
            2, 3, 6, 3, 7, 6, // Top face
            4, 5, 0, 5, 1, 0  // Bottom face
        };
    }
}
