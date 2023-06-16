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
        Transformation = Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateTranslation(Position);
    }
}

[RequiresComponents(typeof(PrismTransformComponent))]
public class PrismCameraComponent : Component
{
    public float FieldOfView = 90;
    public float NearClip = 0.1f;
    public float FarClip = 1000;
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
    private CameraRenderTask renderTask = new CameraRenderTask();
    private ClearRenderTask clearTask = new ClearRenderTask();

    public override void PreRender()
    {
        foreach (var item in Scene.GetAllComponentsOfType<PrismCameraComponent>())
            if (item.Active)
            {
                ProcessCamera(item);
                return;
            }
    }

    private void ProcessCamera(PrismCameraComponent cam)
    {
        var transform = Scene.GetComponentFrom<PrismTransformComponent>(cam.Entity);
        var renderTarget = Window.RenderTarget;

        renderTask.View = transform.Transformation;
        renderTask.Projection = Matrix4x4.CreatePerspectiveFieldOfView(cam.FieldOfView * Utilities.DegToRad, Window.RenderTarget.AspectRatio, cam.NearClip, cam.FarClip);

        if (cam.Clear)
        {
            clearTask.ClearColor = cam.ClearColour;
            RenderQueue.Add(clearTask, RenderOrder.CameraOperations);
        }

        RenderQueue.Add(renderTask, RenderOrder.CameraOperations);
    }
}

public class PrismTransformSystem : Walgelijk.System
{
    public override void Update()
    {
        foreach (var item in Scene.GetAllComponentsOfType<PrismTransformComponent>())
            item.RecalculateMatrix();
    }
}
