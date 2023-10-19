using System.Numerics;
using Walgelijk;
using Walgelijk.Onion;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public struct CompositorScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new CompositorTestSystem());
        scene.AddSystem(new OnionSystem());
        game.UpdateRate = 120;
        game.FixedUpdateRate = 30;

        game.Compositor.Enabled = true;

        game.Compositor.Clear();

        var mix = new MixNode();
        var source1 = new TextureValueNode(Resources.Load<Texture>("qoitest.qoi"));
        source1.Output.ConnectTo(mix.Inputs[0]);
        game.Compositor.SourceNode.Output.ConnectTo(mix.Inputs[1]);

        mix.Output.ConnectTo(game.Compositor.DestinationNode.Inputs[0]);

        return scene;
    }

    public class CompositorTestSystem : Walgelijk.System
    {
        public override void Render()
        {
            RenderQueue.Add(new ClearRenderTask(new Color(0.13f, 0.08f, 0.16f, 1)));
        }

        public override void Update()
        {
            Ui.Layout.Size(120, 32).Move(16);
            Ui.Checkbox(ref Game.Compositor.Enabled, "Enable compositor");


            if (Game.Compositor.DestinationNode.Inputs[0].Connected?.Node is MixNode mix)
                mix.Factor = MathF.Sin(Time.SecondsSinceSceneChange) * 0.5f + 0.5f;
        }
    }
}

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

        for (int i = 0; i < 1024; i++)
        {
            var e = scene.CreateEntity();
            scene.AttachComponent(e, new TransformComponent()
            {
                Scale = new Vector2(16)
            });
            scene.AttachComponent(e, new BatchedSpriteComponent(Resources.Load<Texture>("icon.png"))
            {
                SyncWithTransform = true
            });
        }

        return scene;
    }

    public class BatchRendererTestSystem : Walgelijk.System
    {
        public override void Update()
        {
            if (Input.IsKeyHeld(Key.Space))
                return;

            Draw.Reset();
            Draw.Order = new RenderOrder(0, 1);

            float seed = 0;
            var t = Time.SecondsSinceLoad * 0.05f;
            int x = 0, y = 0;
            Vector2 center = default;
            int i = 0;
            foreach (var item in Scene.GetAllComponentsOfType<TransformComponent>())
            {
                if (!Scene.TryGetComponentFrom<BatchedSpriteComponent>(item.Entity, out var sprite))
                    continue;

                seed += 542.534f;

                item.Position = 128 * new Vector2(
                    Noise.GetValue(0, t, -seed),
                    Noise.GetValue(seed, t, -t)
                    );

                item.Position += 25 * new Vector2(x, -y);
                item.Rotation += Noise.GetValue(30, -t * 7, seed * 2) * Time.DeltaTime * 500;
                item.Scale = Vector2.One * Utilities.MapRange(-1, 1, 4, 32, Noise.GetValue(-30, seed * 2, 5 * t));

                sprite.Texture = Noise.GetValue(t, -t, seed - 23) > 0
                    ? Resources.Load<Texture>("bee.png")
                    : Resources.Load<Texture>("icon.png");

                center += item.Position;
                i++;
                x++;
                if (x == 64)
                {
                    x = 0;
                    y++;
                }
            }

            if (Scene.FindAnyComponent<CameraComponent>(out var cam))
            {
                Scene.GetComponentFrom<TransformComponent>(cam.Entity).Position = center / i;
            }
        }
    }
}
