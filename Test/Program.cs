using System;
using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;

namespace Test
{
    public class PlayerComponent
    {
        public float MovementSpeed = 0.01f;
    }

    public class PlayerSystem : Walgelijk.System
    {
        public override void Initialise() { }
        public override void Render() { }

        public override void Update()
        {
            var w = Input.IsKeyHeld(Key.W);
            var a = Input.IsKeyHeld(Key.A);
            var s = Input.IsKeyHeld(Key.S);
            var d = Input.IsKeyHeld(Key.D);

            if (!(w || a || s || d)) return;

            var delta = Vector2.Zero;

            if (w) delta.Y = 1;
            if (a) delta.X = -1;
            if (s) delta.Y -= 1;
            if (d) delta.X += 1;


            foreach (var pair in Scene.GetAllComponentsOfType<PlayerComponent>())
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(pair.Entity);
                PlayerComponent player = pair.Component;

                transform.Position += delta * player.MovementSpeed;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random();

            Game game = new Game(new OpenTKWindow("hallo daar", new Vector2(128, 128), new Vector2(512, 512)));
            game.Window.TargetFrameRate = 75;
            game.Window.TargetUpdateRate = 75;
            game.Window.VSync = true;
            game.Window.RenderTarget.ClearColour = new Color("#d42c5e");

            var scene = new Scene();

            //create rectangles
            for (int i = 0; i < 1; i++)
            {
                var entity = scene.CreateEntity();

                scene.AttachComponent(entity, new TransformComponent
                {
                    Position = new Vector2(
                        Utilities.RandomFloat(-.3f, .3f),
                        Utilities.RandomFloat(-.3f, .3f)
                        ),
                    Rotation = Utilities.RandomFloat(0, 360)
                });

                scene.AttachComponent(entity, new RectangleShapeComponent
                {
                    Size = new Vector2(
                        Utilities.RandomFloat(.1f, .2f),
                        Utilities.RandomFloat(.1f, .2f)
                        ),
                });

                if (i == 0)
                    scene.AttachComponent(entity, new PlayerComponent());
            }

            //create camera
            {
                var entity = scene.CreateEntity();
                scene.AttachComponent(entity, new CameraComponent());
                scene.AttachComponent(entity, new TransformComponent());

                var cameraSystem = new CameraSystem();
                scene.AddSystem(cameraSystem);
                cameraSystem.SetMainCamera(entity);
            }

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new BasicRendererSystem());
            scene.AddSystem(new PlayerSystem());

            game.Scene = scene;
            game.Start();
        }
    }
}
