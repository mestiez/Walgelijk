﻿using System.Numerics;

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
        public float Speed { get; set; } = 1;
        /// <summary>
        /// Zoom speed
        /// </summary>
        public float ZoomFactor { get; set; } = 1.1f;

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
                transform.Position += Vector2.Normalize(delta) * zoom * Speed;

            if (Input.IsKeyHeld(Key.Plus))
                system.MainCameraComponent.OrthographicSize /= ZoomFactor;
            if (Input.IsKeyHeld(Key.Minus))
                system.MainCameraComponent.OrthographicSize *= ZoomFactor;
        }
    }
}
