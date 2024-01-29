namespace Walgelijk.Prism;

public class Mesh
{
    public readonly string Name;
    public readonly Vertex[] Vertices;
    public readonly uint[] Indices;
    public readonly Material? Material;

    public Mesh(string name, Vertex[] vertices, uint[] indices, Material? material)
    {
        Name = name;
        Vertices = vertices;
        Indices = indices;
        Material = material;
    }
}
