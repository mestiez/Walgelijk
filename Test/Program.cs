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
            game.Window.RenderTarget.ClearColour = new Color("#d42c5e");

            var scene = new Scene();

            for (int i = 0; i < 10; i++)
            {
                var entity = scene.CreateEntity();

                scene.AttachComponent(entity, new TransformComponent
                {
                    Position = new Vector2(
                        Utilities.RandomFloat(-10f, 10f), 
                        Utilities.RandomFloat(-10f, 10f)
                        )
                });

                scene.AttachComponent(entity, new RectangleRendererComponent
                {
                    Size = new Vector2(
                        Utilities.RandomFloat(.5f, 1f), 
                        Utilities.RandomFloat(.5f, 1f)
                        )
                });
            }

            var rendererSystem = new RectangleRendererSystem();
            scene.AddSystem(rendererSystem);

            game.Scene = scene;
            game.Start();
        }
    }
}
