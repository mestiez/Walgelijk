﻿using System.Numerics;

namespace Walgelijk.Prism;

public static class PrismPrimitives
{
    public static void GenerateQuad(Vector2 size, out Vertex[] vertices, out uint[] indices)
    {
        var halfSize = size / 2;

        vertices = new Vertex[]
        {
            new Vertex(-halfSize.X, -halfSize.Y, 0, new Vector3(0, 0, 1)){ TexCoords = new Vector2(-0.5f, 0.5f) },
            new Vertex(halfSize.X, -halfSize.Y, 0, new Vector3(0, 0, 1)){ TexCoords = new Vector2(0.5f, -0.5f) },
            new Vertex(-halfSize.X, halfSize.Y, 0, new Vector3(0, 0, 1)){ TexCoords = new Vector2(-0.5f, 0.5f) },
            new Vertex(halfSize.X, halfSize.Y, 0, new Vector3(0, 0, 1)){ TexCoords = new Vector2(0.5f, 0.5f) },
        };

        indices = new uint[]
        {
            0, 2, 1,
            1, 2, 3
        };
    }

    public static void GenerateCuboid(Vector3 size, out Vertex[] vertices, out uint[] indices)
    {
        var halfSize = size / 2;

        vertices = new Vertex[]
        {
            new Vertex(-halfSize.X, -halfSize.Y, -halfSize.Z, new Vector3(-1, -1, -1)), // 0
            new Vertex(halfSize.X, -halfSize.Y, -halfSize.Z, new Vector3(1, -1, -1)), // 1
            new Vertex(-halfSize.X, halfSize.Y, -halfSize.Z, new Vector3(-1, 1, -1)), // 2
            new Vertex(halfSize.X, halfSize.Y, -halfSize.Z, new Vector3(1, 1, -1)), // 3

            new Vertex(halfSize.X, -halfSize.Y, -halfSize.Z, new Vector3(1, -1, -1)), // 4
            new Vertex(halfSize.X, -halfSize.Y, halfSize.Z, new Vector3(1, -1, 1)), // 5
            new Vertex(halfSize.X, halfSize.Y, -halfSize.Z, new Vector3(1, 1, -1)), // 6
            new Vertex(halfSize.X, halfSize.Y, halfSize.Z, new Vector3(1, 1, 1)), // 7

            new Vertex(halfSize.X, -halfSize.Y, halfSize.Z, new Vector3(1, -1, 1)), // 8
            new Vertex(-halfSize.X, -halfSize.Y, halfSize.Z, new Vector3(-1, -1, 1)), // 9
            new Vertex(halfSize.X, halfSize.Y, halfSize.Z, new Vector3(1, 1, 1)), // 10
            new Vertex(-halfSize.X, halfSize.Y, halfSize.Z, new Vector3(-1, 1, 1)), // 11

            new Vertex(-halfSize.X, -halfSize.Y, halfSize.Z, new Vector3(-1, -1, 1)), // 12
            new Vertex(-halfSize.X, -halfSize.Y, -halfSize.Z, new Vector3(-1, -1, -1)), // 13
            new Vertex(-halfSize.X, halfSize.Y, halfSize.Z, new Vector3(-1, 1, 1)), // 14
            new Vertex(-halfSize.X, halfSize.Y, -halfSize.Z, new Vector3(-1, 1, -1)), // 15

            new Vertex(-halfSize.X, halfSize.Y, -halfSize.Z, new Vector3(-1, 1, -1)), // 16
            new Vertex(halfSize.X, halfSize.Y, -halfSize.Z, new Vector3(1, 1, -1)), // 17
            new Vertex(-halfSize.X, halfSize.Y, halfSize.Z, new Vector3(-1, 1, 1)), // 18
            new Vertex(halfSize.X, halfSize.Y, halfSize.Z, new Vector3(1, 1, 1)), // 19

            new Vertex(-halfSize.X, -halfSize.Y, halfSize.Z, new Vector3(-1, -1, 1)), // 20
            new Vertex(halfSize.X, -halfSize.Y, halfSize.Z, new Vector3(1, -1, 1)), // 21
            new Vertex(-halfSize.X, -halfSize.Y, -halfSize.Z, new Vector3(-1, -1, -1)), // 22
            new Vertex(halfSize.X, -halfSize.Y, -halfSize.Z, new Vector3(1, -1, -1)) // 23
        };

        indices = new uint[]
        {
            0, 2, 1, 1, 2, 3,       // Front face
            4, 6, 5, 5, 6, 7,       // Right face
            8, 10, 9, 9, 10, 11,    // Back face
            12, 14, 13, 13, 14, 15, // Left face
            16, 18, 17, 17, 18, 19, // Top face
            20, 22, 21, 21, 22, 23  // Bottom face
        };
    }
}
