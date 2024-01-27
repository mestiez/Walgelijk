using System.Numerics;

namespace Walgelijk.Prism;

public static class MeshLoader
{
    private static Assimp.AssimpContext ctx = new();

    public static Mesh[] Load(string path)
    {
        var scene = ctx.ImportFile(path, Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.OptimizeMeshes | Assimp.PostProcessSteps.OptimizeGraph);
        var dir = Path.GetDirectoryName(path) ?? string.Empty;
        if (scene.HasMeshes)
        {
            var meshes = new Mesh[scene.MeshCount];
            for (int p = 0; p < scene.Meshes.Count; p++)
            {
                var readMesh = scene.Meshes[p];
                var mat = new Material(Material.DefaultTextured);

                if (readMesh.MaterialIndex != -1)
                {
                    var readMat = scene.Materials[readMesh.MaterialIndex];
                    mat.DepthTested = true;
                    mat.BlendMode = readMat.BlendMode switch
                    {
                        Assimp.BlendMode.Additive => BlendMode.Addition,
                        _ => BlendMode.AlphaBlend,
                    };
                    if (readMat.HasTextureDiffuse)
                        mat.SetUniform("mainTex", Resources.Load<Texture>(Path.Combine(dir, readMat.TextureDiffuse.FilePath), true));
                }

                var mesh = meshes[p] = new Mesh(readMesh.Name, new Vertex[readMesh.VertexCount], readMesh.GetUnsignedIndices(), mat);

                var vertices = mesh.Vertices;

                for (int i = 0; i < readMesh.Vertices.Count; i++)
                {
                    var v = readMesh.Vertices[i];
                    var n = readMesh.Normals[i];

                    vertices[i].Position = new Vector3(v.X, v.Y, v.Z);
                    vertices[i].Normal = new Vector3(n.X, n.Y, n.Z);
                    vertices[i].Color = Colors.White;
                }

                var uv = readMesh.TextureCoordinateChannels.FirstOrDefault();
                if (uv != null)
                    for (int i = 0; i < uv.Count; i++)
                    {
                        var v = readMesh.TextureCoordinateChannels[0][i];
                        vertices[i].TexCoords = new Vector2(v.X, v.Y);
                    }

                meshes[p] = mesh;
            }
            return meshes;
        }
        return Array.Empty<Mesh>();
    }
}