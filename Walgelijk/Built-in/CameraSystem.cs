using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// System that manages the built in <see cref="CameraComponent"/>
    /// </summary>
    public class CameraSystem : System
    {
        private CameraRenderTask renderTask = new CameraRenderTask();
        private ClearRenderTask clearTask = new ClearRenderTask();

        /// <summary>
        /// Main camera entity
        /// </summary>
        public Entity MainCameraEntity { get; private set; }

        /// <summary>
        /// Main camera component
        /// </summary>
        public CameraComponent? MainCameraComponent { get; private set; }

        /// <summary>
        /// Main camera transform component
        /// </summary>
        public TransformComponent? MainCameraTransform { get; private set; }

        /// <summary>
        /// Set the correct matrices even before the RenderQueue is executed. This makes sure the Window point mapping functions don't lag behind 1 frame at the negligable cost of performance.
        /// </summary>
        public bool SetMatricesPreRender = true;

        private bool mainCameraSet;

        public override void Initialise()
        {
            if (MainCameraTransform == null)
                FallbackToFirstCamera();
        }

        private void FallbackToFirstCamera()
        {
            if (Scene.FindAnyComponent<CameraComponent>(out var camera))
                SetMainCamera(camera.Entity);
        }

        /// <summary>
        /// Set the main camera for this system
        /// </summary>
        public void SetMainCamera(Entity cameraEntity)
        {
            var camera = GetCameraFrom(cameraEntity);

            MainCameraEntity = cameraEntity;
            MainCameraComponent = camera;
            MainCameraTransform = Scene.GetComponentFrom<TransformComponent>(MainCameraEntity);

            mainCameraSet = true;
        }

        private CameraComponent GetCameraFrom(Entity cameraEntity)
        {
            if (Scene == null)
                throw new InvalidOperationException("System has not been added to a scene yet");

            if (!Scene.HasEntity(cameraEntity))
                throw new ArgumentException($"{cameraEntity} does not exist in the scene");

            if (!Scene.TryGetComponentFrom<CameraComponent>(cameraEntity, out var camera))
                throw new ArgumentException($"{cameraEntity} has no {nameof(CameraComponent)}");

            return camera ?? throw new Exception("Camera is null");
        }

        public override void PreRender()
        {
            if (!mainCameraSet || MainCameraComponent == null)
            {
                FallbackToFirstCamera();
                return;
            }

            SetRenderTask();

            if (MainCameraComponent.Clear)
            {
                clearTask.ClearColor = MainCameraComponent.ClearColour;
                RenderQueue.Add(clearTask, RenderOrder.CameraOperations);
            }

            RenderQueue.Add(renderTask, RenderOrder.CameraOperations);

            if (SetMatricesPreRender)
                renderTask.Execute(Graphics);
        }

        private void SetRenderTask()
        {
            if (MainCameraTransform == null || MainCameraComponent == null)
                return;

            var renderTarget = Scene.Game.Window.RenderTarget;

            renderTask.View = new Matrix4x4(MainCameraTransform.WorldToLocalMatrix);
            var size = renderTarget.Size / MainCameraComponent.PixelsPerUnit * MainCameraComponent.OrthographicSize;
            renderTask.Projection = Matrix4x4.CreateOrthographic(size.X, size.Y, 0, 1);
        }
    }
}
