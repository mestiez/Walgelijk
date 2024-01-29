namespace Walgelijk.Prism;

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismMeshComponent : Component
{
    public readonly VertexBuffer VertexBuffer;
    public readonly PrismRenderTask<Vertex> RenderTask;
    public Material Material;

    public PrismMeshComponent(Vertex[] verts, uint[] indices, Material material)
    {
        VertexBuffer = new VertexBuffer(verts, indices);
        RenderTask = new(VertexBuffer);
        Material = material;
    }

    public PrismMeshComponent(VertexBuffer vertexBuffer, Material material)
    {
        VertexBuffer = vertexBuffer;
        RenderTask = new(vertexBuffer);
        Material = material;
    }
}

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismMeshComponent<TVertex> : Component where TVertex : struct
{
    public readonly VertexBuffer<TVertex> VertexBuffer;
    public readonly PrismRenderTask<TVertex> RenderTask;
    public Material Material;

    public PrismMeshComponent(VertexBuffer<TVertex> vertexBuffer, Material material)
    {
        VertexBuffer = vertexBuffer;
        RenderTask = new(vertexBuffer);
        Material = material;
    }
}
