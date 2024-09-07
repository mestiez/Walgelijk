namespace Walgelijk.Prism.Pbr;

public class PbrMesh
{
    public readonly string Name;

    public readonly PbrVertex[] Vertices;

    public readonly uint[] Indices;

    public readonly Material? Material;

    public PbrMesh(string name, PbrVertex[] vertices, uint[] indices, Material? material)
    {
        Name = name;
        Vertices = vertices;
        Indices = indices;
        Material = material;
    }
}
