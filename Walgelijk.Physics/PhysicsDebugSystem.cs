using System.Numerics;

namespace Walgelijk.Physics
{
    public class PhysicsDebugSystem : Walgelijk.System
    {
        public override void Render()
        {
            if (!Scene.FindAnyComponent<PhysicsWorldComponent>(out var world, out _))
            {
                Logger.Warn("PhysicsSystem without PhysicsWorld...");
                return;
            }

            var bodies = Scene.GetAllComponentsOfType<PhysicsBodyComponent>();
            foreach (var item in bodies)
            {
                if (item.Component.Collider == null)
                    continue;

                var bounds = item.Component.Collider.Bounds;
                DebugDraw.Rectangle(bounds, 0, Colors.Yellow);
            }

            var cSize = new Vector2(world.ChunkSize, world.ChunkSize);
            var back = RenderOrder.Zero.WithOrder(-1);
            foreach (var item in world.ChunkDictionary)
            {
                var c = item.Value;
                var topleft = new Vector2(c.X, c.Y) * world.ChunkSize;
                DebugDraw.Rectangle(
                    topleft - cSize, cSize,
                    0,
                    c.IsEmpty ? Colors.Gray * .5f : Colors.Aqua * .5f,
                    renderOrder: c.IsEmpty ? back : RenderOrder.Zero);
            }
        }
    }
}
