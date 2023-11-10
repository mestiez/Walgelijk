using OpenTK.Graphics.ES11;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Numerics;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public struct BatchRendererScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new BatchRendererSystem());
        scene.AddSystem(new BatchRendererTestSystem());
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#2e515b")
        });

        game.UpdateRate = 120;
        game.FixedUpdateRate = 30;

        scene.AttachComponent(scene.CreateEntity(), new BatchRendererStorageComponent());

        game.Compositor.Enabled = false;
        int x = 0;
        int y = 0;

        for (int i = 0; i < 64; i++)
        {
            var e = scene.CreateEntity();
            scene.AttachComponent(e, new TransformComponent()
            {
                Scale = new Vector2(64)
            });
            scene.AttachComponent(e, new BatchedSpriteComponent(Resources.Load<Texture>("icon.png"))
            {
                SyncWithTransform = true
            });
            scene.AttachComponent(e, new GridPlacementComponent
            {
                Center = new Vector2(x, -y) * 25
            });

            x++;
            if (x == 64)
            {
                x = 0;
                y++;
            }
        }

        return scene;
    }

    public class GridPlacementComponent : Component
    {
        public Vector2 Center;
    }

    public class BatchRendererTestSystem : Walgelijk.System
    {
        public override void Update()
        {
            Draw.ScreenSpace = true;
            Draw.FontSize = 32;
            Draw.Font = Resources.Load<Font>("pt-serif-regular.wf");
            Draw.Text("Very simple batch rendering for a moderate amount of sprites", new Vector2(Window.Width / 2, Window.Height - 10), Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Bottom);

            if (Input.IsKeyHeld(Key.Space))
                return;

            Draw.Reset();
            Draw.Order = new RenderOrder(0, 1);

            var t = Time.SecondsSinceLoad * 0.005f;
            Vector2 center = default;

            var bee = Resources.Load<Texture>("bee.png");
            var icon = Resources.Load<Texture>("icon.png");
            int i = 0;
            object l = new();
            var mpos = Input.WorldMousePosition;

            Parallel.ForEach(Scene.GetAllComponentsOfType<TransformComponent>(), (item) =>
            {
                if (!Scene.TryGetComponentFrom<BatchedSpriteComponent>(item.Entity, out var sprite))
                    return;

                float seed = item.Entity * 0.0325f;

                item.Position = 256 * new Vector2(
                    Noise.GetValue(0, t, -seed),
                    Noise.GetValue(seed, t, -t)
                    ) + Scene.GetComponentFrom<GridPlacementComponent>(item.Entity).Center;

                sprite.Texture = MathF.Sin(item.Position.X * 0.02f + Time.SecondsSinceLoad) > 0
                    ? bee
                    : icon; ;

                var delta = item.Position - mpos;
                var dist = delta.Length();
                var dir = delta / dist;
                dist = Utilities.Clamp(dist * 0.01f, 1, float.MaxValue);
                var mouseRepulseOffset = dir / dist * 64;
                item.Position += mouseRepulseOffset;

                item.Rotation += Noise.GetValue(30, -t * 7, seed * 2) * Time.DeltaTime * 500;
                item.Scale = Vector2.One * Utilities.MapRange(-1, 1, 4, 128, Noise.GetValue(-30, seed * 2, 5 * t));

                sprite.Color = Utilities.Lerp(Colors.White, Colors.Purple, 1 - Utilities.Clamp(dist - 1));

                sprite.VerticalFlip = Time.SecondsSinceLoad % 2 > 1;

                lock (l)
                    center += item.Position;
                Interlocked.Increment(ref i);
            });

            if (Scene.FindAnyComponent<CameraComponent>(out var cam))
            {
                Scene.GetComponentFrom<TransformComponent>(cam.Entity).Position = center / i;
            }
        }
    }
}
