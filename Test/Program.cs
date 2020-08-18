using System;
using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rand = new Random();

            Game game = new Game(new OpenTKWindow("hallo daar", new Vector2(128, 128), new Vector2(512, 512)));
            game.Window.TargetFrameRate = 75;
            game.Window.TargetUpdateRate = 75;
            game.Window.VSync = false;
            game.Window.RenderTarget.ClearColour = new Color("#d42c5e");

            var scene = new Scene();

            //create rectangles
            for (int i = 0; i < 10; i++)
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

            game.Scene = scene;
            game.Start();
        }
    }
}
