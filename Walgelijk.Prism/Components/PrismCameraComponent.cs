namespace Walgelijk.Prism;

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismCameraComponent : Component
{
    public float FieldOfView = 80;
    public float NearClip = 0.1f;
    public float FarClip = 1000;
    public Color ClearColour;
    public bool Clear = true;
    public Frustum Frustum;

    public bool Active = true;

    public static void SetActive(Scene scene, PrismCameraComponent camera)
    {
        foreach (var item in scene.GetAllComponentsOfType<PrismCameraComponent>())
            item.Active = camera == item;
    }
}
