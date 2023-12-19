using System.Numerics;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct TransformTestScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new TransformTestSystem());
        scene.AddSystem(new ShapeRendererSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#cc5281")
        });

        game.UpdateRate = 0;
        game.FixedUpdateRate = 8;

        var interpolated = scene.CreateEntity();
        scene.AttachComponent(interpolated, new TransformComponent() { InterpolationFlags = InterpolationFlags.All });
        scene.AttachComponent(interpolated, new SpriteComponent(Texture.ErrorTexture)); 
        
        var normal = scene.CreateEntity();
        scene.AttachComponent(normal, new TransformComponent() { InterpolationFlags = InterpolationFlags.None });
        scene.AttachComponent(normal, new SpriteComponent(Texture.ErrorTexture));

        return scene;
    }

    public class TransformTestSystem : Walgelijk.System
    {
        public override void Initialise()
        {

        }

        public override void FixedUpdate()
        {
            int y = 0;
            var p = MathF.Sin(Time.SecondsSinceLoad) * 300;

            foreach (var sprite in Scene.GetAllComponentsOfType<SpriteComponent>())
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(sprite.Entity);
                transform.Scale = new Vector2(64);
                transform.Position = new Vector2(p, y);
                y -= 128;
            }
        }
    }
}
