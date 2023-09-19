using System.Numerics;

namespace Walgelijk.Prism;

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismMeshComponent : Component
{
    public readonly VertexBuffer VertexBuffer;
    public readonly PrismRenderTask RenderTask;

    public PrismMeshComponent(VertexBuffer vertexBuffer)
    {
        VertexBuffer = vertexBuffer;
        RenderTask = new PrismRenderTask(vertexBuffer);
    }

    public Material Material = Material.DefaultTextured;

    public PrismMeshComponent()
    {
        PrismPrimitives.GenerateCuboid(new Vector3(1, 1, 1), out var verts, out var indices);
        VertexBuffer = new VertexBuffer(verts, indices) { PrimitiveType = Primitive.Triangles };
        RenderTask = new PrismRenderTask(VertexBuffer);
    }
}
