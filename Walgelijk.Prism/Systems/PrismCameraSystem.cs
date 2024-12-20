﻿using System.Numerics;

namespace Walgelijk.Prism;

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

        if (Matrix4x4.Invert(transform.Transformation, out var i))
            renderTask.View = i;
        renderTask.Projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.Max(cam.FieldOfView, 1) * Utilities.DegToRad, (float)Window.Width / Window.Height, cam.NearClip, cam.FarClip);

        cam.Frustum = new Frustum(i * renderTask.Projection);

        if (cam.Clear)
        {
            clearTask.ClearColor = cam.ClearColour;
            RenderQueue.Add(clearTask, RenderOrder.CameraOperations);
        }

        RenderQueue.Add(renderTask, RenderOrder.CameraOperations);
    }
}
