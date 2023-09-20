using System.Numerics;

namespace Walgelijk.Prism;

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismMeshComponent : Component
{
    public readonly VertexBuffer VertexBuffer;
    public readonly PrismRenderTask RenderTask;
    public Material Material = Material.DefaultTextured;

    public PrismMeshComponent(Vertex[] verts, uint[] indices)
    {
        VertexBuffer = new VertexBuffer(verts, indices);
        RenderTask = new PrismRenderTask(VertexBuffer);
    }

    public PrismMeshComponent(VertexBuffer vertexBuffer)
    {
        VertexBuffer = vertexBuffer;
        RenderTask = new PrismRenderTask(vertexBuffer);
    }
}
