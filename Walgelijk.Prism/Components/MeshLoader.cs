using System.Numerics;

namespace Walgelijk.Prism;

public static class MeshLoader
{
    private static Assimp.AssimpContext ctx = new();

    public static void Load(string path, out Vertex[] vertices, out uint[] indices)
    {
        vertices = Array.Empty<Vertex>();
        indices = Array.Empty<uint>();
        var scene = ctx.ImportFile(path);
        if (scene.HasMeshes)
        {
            foreach (var mesh in scene.Meshes)
            {
                indices = mesh.GetUnsignedIndices();
                vertices = new Vertex[mesh.VertexCount];

                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    var v = mesh.Vertices[i];
                    vertices[i].Position = new Vector3(v.X, v.Y, v.Z);
                    vertices[i].Color = Colors.White;
                }

                for (int i = 0; i < mesh.Normals.Count; i++)
                {
                    var v = mesh.Normals[i];
                    vertices[i].Normal = new Vector3(v.X, v.Y, v.Z);
                }

                for (int i = 0; i < mesh.TextureCoordinateChannels[0].Count; i++)
                {
                    var v = mesh.TextureCoordinateChannels[0][i];
                    vertices[i].TexCoords = new Vector2(v.X, v.Y);
                }
                return;
            }
        }
    }
}