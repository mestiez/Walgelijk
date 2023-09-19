namespace Walgelijk.Prism;

public class PrismTransformSystem : Walgelijk.System
{
    public override void Update()
    {
        foreach (var item in Scene.GetAllComponentsOfType<PrismTransformComponent>())
        {
            item.RecalculateMatrix();
        }
    }
}
