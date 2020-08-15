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

            Game game = new Game(new OpenTKWindow("hallo daar", new Vector2(128, 128), new Vector2(800, 600)));
            game.Window.RenderTarget.ClearColour = new Color("0f3f00");

            var scene = new Scene();

            for (int i = 0; i < 10; i++)
            {
                var entity = scene.CreateEntity();
                var transform = new TransformComponent();
                scene.AttachComponent(entity, transform);
                scene.AttachComponent(entity, new RectangleRendererComponent
                {
                    offset = (float)rand.NextDouble() * 1000,
                    speed = (float)rand.NextDouble() * 1.5f + 0.5f,
                    Size = new Vector2((float)rand.NextDouble() * .1f + 0.1f, (float)rand.NextDouble() * .1f + 0.1f)
                });
            }

            var rendererSystem = new RectangleRendererSystem();
            scene.AddSystem(rendererSystem);

            game.Scene = scene;
            game.Start();
        }
    }
}
