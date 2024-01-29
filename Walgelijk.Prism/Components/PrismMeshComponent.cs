namespace Walgelijk.Prism;

public abstract class BasePrismMeshComponent : Component
{
    public abstract void UpdateRenderTask(Scene scene);

    public abstract IRenderTask? GetRenderTask();
}

[RequiresComponents(typeof(PrismTransformComponent))]
public abstract class PrismMeshComponent<TVertex> : BasePrismMeshComponent where TVertex : struct
{
    public readonly VertexBuffer<TVertex> VertexBuffer;
    public readonly PrismRenderTask<TVertex> RenderTask;
    public Material Material;

    public PrismMeshComponent(TVertex[] verts, uint[] indices, IVertexDescriptor descriptor, Material material)
    {
        VertexBuffer = new VertexBuffer<TVertex>(verts, indices, descriptor);
        RenderTask = new PrismRenderTask<TVertex>(VertexBuffer);
        Material = material;
    }

    public PrismMeshComponent(VertexBuffer<TVertex> vertexBuffer, Material material)
    {
        VertexBuffer = vertexBuffer;
        RenderTask = new PrismRenderTask<TVertex>(vertexBuffer);
        Material = material;
    }

    public override IRenderTask? GetRenderTask() => RenderTask;
}

/// <summary>
/// Mesh component using the default <see cref="Vertex"/>
/// </summary>
public class PrismMeshComponent : PrismMeshComponent<Vertex>
{
    public PrismMeshComponent(VertexBuffer<Vertex> vertexBuffer, Material material) : base(vertexBuffer, material)
    {
    }

    public PrismMeshComponent(Vertex[] verts, uint[] indices, Material material) : base(verts, indices, new Vertex.Descriptor(), material)
    {
    }

    public override void UpdateRenderTask(Scene scene)
    {
        var transform = scene.GetComponentFrom<PrismTransformComponent>(Entity);

        RenderTask.VertexBuffer = VertexBuffer;
        RenderTask.Material = Material;
        RenderTask.ModelMatrix = transform.Transformation;
    }
}