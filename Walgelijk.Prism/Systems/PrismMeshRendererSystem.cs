namespace Walgelijk.Prism;

public class PrismMeshRendererSystem : Walgelijk.System
{
    public override void Render()
    {
        if (!Game.Compositor.Flags.HasFlag(RenderTextureFlags.Depth))
            Game.Compositor.Flags |= RenderTextureFlags.Depth;

        foreach (var item in Scene.GetAllComponentsOfType<PrismMeshComponent>())
        {
            var transform = Scene.GetComponentFrom<PrismTransformComponent>(item.Entity);

            item.RenderTask.VertexBuffer = item.VertexBuffer;
            item.RenderTask.Material = item.Material;
            item.RenderTask.ModelMatrix = transform.Transformation;

            RenderQueue.Add(item.RenderTask);
        }
    }
}
