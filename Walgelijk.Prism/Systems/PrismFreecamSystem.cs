using System.Numerics;

namespace Walgelijk.Prism;

public class PrismFreecamSystem : Walgelijk.System
{
    float x = 0;
    float y = 0;

    public override void Update()
    {
        var camera = Scene.GetAllComponentsOfType<PrismCameraComponent>().FirstOrDefault(static c => c.Active);
        if (camera != null)
        {
            var transform = Scene.GetComponentFrom<PrismTransformComponent>(camera.Entity);
            const float sensitivity = 0.01f;

            x += Input.WindowMouseDelta.X * -sensitivity;
            y += Input.WindowMouseDelta.Y * -sensitivity;

            var rot1 = Matrix4x4.Identity;

            rot1 *= Matrix4x4.CreateRotationX(y);
            rot1 *= Matrix4x4.CreateRotationY(x);

            transform.Rotation = Quaternion.CreateFromRotationMatrix(rot1);

            float speed = Time.DeltaTime * 4;

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
        }
    }
}
