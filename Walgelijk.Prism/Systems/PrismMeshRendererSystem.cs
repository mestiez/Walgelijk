namespace Walgelijk.Prism;

public class PrismMeshRendererSystem : Walgelijk.System
{
    public override void Render()
    {
        foreach (var item in Scene.GetAllComponentsOfType<BasePrismMeshComponent>())
        {
            var r = item.GetRenderTask();
            if (r != null)
            {
                item.UpdateRenderTask(Scene);
                RenderQueue.Add(r);
            }
        }
    }
}
