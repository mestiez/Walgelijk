namespace Walgelijk.Prism;

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismMeshComponent : Component
{
    public readonly VertexBuffer VertexBuffer;
    public readonly PrismRenderTask RenderTask;
    public Material Material;

    public PrismMeshComponent(Vertex[] verts, uint[] indices, Material material)
    {
        VertexBuffer = new VertexBuffer(verts, indices);
        RenderTask = new PrismRenderTask(VertexBuffer);
        Material = material;
    }

    public PrismMeshComponent(VertexBuffer vertexBuffer, Material material)
    {
        VertexBuffer = vertexBuffer;
        RenderTask = new PrismRenderTask(vertexBuffer);
        Material = material;
    }
}
