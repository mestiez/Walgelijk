namespace Walgelijk.Prism.Pbr;

public class PbrMeshComponent : PrismMeshComponent<PbrVertex>
{
    public RenderOrder RenderOrder;

    public PbrMeshComponent(VertexBuffer<PbrVertex> vertexBuffer, Material material) : base(vertexBuffer, material)
    {
    }

    public PbrMeshComponent(PbrVertex[] verts, uint[] indices, Material material) : base(verts, indices, new PbrVertex.Descriptor(), material)
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
