using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// System that manages the built in <see cref="CameraComponent"/>
    /// </summary>
    public class CameraSystem : System
    {
        private CameraRenderTask renderTask;
        private Entity mainCameraEntity;
        private CameraComponent mainCameraComponent;
        private TransformComponent mainCameraTransform;

        private bool mainCameraSet;

        public override void Initialise()
        {
            renderTask = new CameraRenderTask();
        }

        /// <summary>
        /// Set the main camera for this system
        /// </summary>
        /// <param name="cameraEntity"></param>
        public void SetMainCamera(Entity cameraEntity)
        {
            if (Scene == null)
                throw new InvalidOperationException("System has not been added to a scene yet");

            if (!Scene.HasEntity(cameraEntity))
                throw new ArgumentException($"{cameraEntity} does not exist in the scene");            
            
            if (!Scene.TryGetComponentFrom<CameraComponent>(cameraEntity, out var camera))
                throw new ArgumentException($"{cameraEntity} has no {nameof(CameraComponent)}");

            mainCameraEntity = cameraEntity;
            mainCameraComponent = camera;
            mainCameraTransform = Scene.GetComponentFrom<TransformComponent>(mainCameraEntity);

            mainCameraSet = true;
        }

        public override void Execute()
        {
            if (!mainCameraSet) return;
            SetRenderTask();
            Scene.Game.Window.RenderQueue.Enqueue(renderTask);
        }

        private void SetRenderTask()
        {
            renderTask.View = mainCameraTransform.WorldToLocalMatrix;
            renderTask.Projection = Matrix4x4.CreateOrthographic(mainCameraComponent.OrthographicSize, mainCameraComponent.OrthographicSize, 0, 1);
        }
    }
}
