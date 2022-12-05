using System.Numerics;

namespace Walgelijk.Physics;

public class PhysicsDebugSystem : Walgelijk.System
{
    public override void Render()
    {
        if (!Scene.FindAnyComponent<PhysicsWorldComponent>(out var world))
        {
            Logger.Warn("PhysicsSystem without PhysicsWorld...");
            return;
        }

        foreach (var item in Scene.GetAllComponentsOfType<PhysicsBodyComponent>())
        {
            if (item.Collider == null)
                continue;

            var bounds = item.Collider.Bounds;
            DebugDraw.Rectangle(bounds, 0, Colors.Yellow);
        }

        var cSize = new Vector2(world.ChunkSize);
        var back = RenderOrder.Zero.WithOrder(-1);
        foreach (var item in world.ChunkDictionary)
        {
            var c = item.Value;
            var topleft = new Vector2(c.X, c.Y) * world.ChunkSize;
            DebugDraw.Rectangle(
                topleft + new Vector2(0, cSize.Y), cSize,
                0,
                c.IsEmpty ? Colors.Gray * .5f : Colors.Aqua * .5f,
                renderOrder: c.IsEmpty ? back : RenderOrder.Zero);
        }
    }
}
