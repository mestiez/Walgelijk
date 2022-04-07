using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Provides basic WASD + - camera controls
    /// </summary>
    public class DebugCameraSystem : System
    {
        /// <summary>
        /// Movement speed
        /// </summary>
        public float Speed { get; set; } = 500;
        /// <summary>
        /// Zoom speed
        /// </summary>
        public float ZoomFactor { get; set; } = 4f;

        public override void Update()
        {
            var system = Scene.GetSystem<CameraSystem>();
            if (system.MainCameraComponent != null)
            {
                ControlCamera(system);
            }
        }

        private void ControlCamera(CameraSystem system)
        {
            if (system.MainCameraComponent == null || system.MainCameraTransform == null)
                return;

            var transform = system.MainCameraTransform;
            var delta = Vector2.Zero;
            var zoom = system.MainCameraComponent.OrthographicSize;

            if (Input.IsKeyHeld(Key.W))
                delta += new Vector2(0, 1);
            if (Input.IsKeyHeld(Key.A))
                delta += new Vector2(-1, 0);
            if (Input.IsKeyHeld(Key.S))
                delta += new Vector2(0, -1);
            if (Input.IsKeyHeld(Key.D))
                delta += new Vector2(1, 0);

            if (delta != Vector2.Zero)
                transform.Position += Vector2.Normalize(delta) * zoom * Speed * Time.DeltaTime;

            float zoomFac = MathF.Pow(ZoomFactor, Time.DeltaTime);
            if (Input.IsKeyHeld(Key.Equal))
                system.MainCameraComponent.OrthographicSize /= zoomFac;
            if (Input.IsKeyHeld(Key.Minus))
                system.MainCameraComponent.OrthographicSize *= zoomFac;
        }
    }
}
