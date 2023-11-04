using System.Numerics;

namespace Walgelijk.Prism;

public class PrismFreecamSystem : Walgelijk.System
{
    private float x = 0;
    private float y = 0;
    private float currentSpeed = 4;
    private bool locked = false;

    public float BaseSpeed = 4;
    public float TopSpeed = 64;

    public override void Update()
    {
        if (Window.HasFocus && !locked && Input.AnyMouseButton)
        {
            Window.IsCursorLocked = true;
            locked = true;
        }

        if (!Window.HasFocus || Input.IsKeyPressed(Key.Escape))
        {
            Window.IsCursorLocked = false;
            locked = false;
        }

        var camera = Scene.GetAllComponentsOfType<PrismCameraComponent>().FirstOrDefault(static c => c.Active);
        if (camera != null && locked)
        {
            var transform = Scene.GetComponentFrom<PrismTransformComponent>(camera.Entity);
            const float sensitivity = 0.01f;

            if (Window.HasFocus)
            {
                x += Input.WindowMouseDelta.X * -sensitivity;
                y += Input.WindowMouseDelta.Y * -sensitivity;
            }

            var rot1 = Matrix4x4.Identity;

            y = Math.Clamp(y, -MathF.PI / 2, MathF.PI / 2);

            rot1 *= Matrix4x4.CreateRotationX(y);
            rot1 *= Matrix4x4.CreateRotationY(x);

            transform.Rotation = Quaternion.CreateFromRotationMatrix(rot1);

            float speed = Time.DeltaTime * currentSpeed;

            if (Input.IsKeyHeld(Key.W))
                transform.Position += transform.Forwards * speed;

            if (Input.IsKeyHeld(Key.D))
                transform.Position += transform.Right * speed;

            if (Input.IsKeyHeld(Key.A))
                transform.Position += transform.Left * speed;

            if (Input.IsKeyHeld(Key.S))
                transform.Position += transform.Backwards * speed;

            if (Input.IsKeyHeld(Key.LeftShift))
                transform.Position += transform.Up * speed;

            if (Input.IsKeyHeld(Key.LeftControl))
                transform.Position += transform.Down * speed;

            if (Input.IsKeyHeld(Key.Space))
                currentSpeed = Utilities.SmoothApproach(currentSpeed, TopSpeed, 2, Time.DeltaTime);
            else
                currentSpeed = Utilities.SmoothApproach(currentSpeed, BaseSpeed, 2, Time.DeltaTime);
        }
    }
}
