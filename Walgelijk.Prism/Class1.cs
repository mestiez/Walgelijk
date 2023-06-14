using System.Numerics;

namespace Walgelijk.Prism;

public class PrismTransformComponent : Component
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale = Vector3.One;

    public Matrix4x4 Transformation = Matrix4x4.Identity;

    public void RecalculateMatrix()
    {
        Transformation = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Position) ;
    }
}

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismCameraComponent : Component
{
    public float FieldOfView = 90;
    public Color ClearColour;
    public bool Clear = true;

    public bool Active = true;

    public static void SetActive(Scene scene, PrismCameraComponent camera)
    {
        foreach (var item in scene.GetAllComponentsOfType<PrismCameraComponent>())
            item.Active = camera == item;
    }
}

public class PrismCameraSystem : Walgelijk.System
{

}

public class PrismTransformSystem : Walgelijk.System
{
    public override void Update()
    {
        
    }
}
