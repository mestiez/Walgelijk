namespace Walgelijk.Prism.Pbr;

public class PbrVertexBuffer : VertexBuffer<PbrVertex>
{
    public PbrVertexBuffer()
    {
        Descriptor = new PbrVertex.Descriptor();
    }

    public PbrVertexBuffer(PbrVertex[] vertices) : base(vertices, new PbrVertex.Descriptor())
    {
    }

    public PbrVertexBuffer(PbrVertex[] vertices, uint[] indices, VertexAttributeArray[]? extraAttributes = null) : base(vertices, indices, new PbrVertex.Descriptor(), extraAttributes)
    {
    }
}
