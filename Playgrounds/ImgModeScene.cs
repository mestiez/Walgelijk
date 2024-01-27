using System.Numerics;
using Walgelijk;
using Walgelijk.SimpleDrawing;

namespace Playgrounds;

public struct ImgModeScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new ImgModeSystem());
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

    public class ImgModeSystem : Walgelijk.System
    {
        float w = 512;
        float h = 512;

        public override void Render()
        {
            float s = Time.DeltaTime * 512;

            if (Input.IsKeyHeld(Key.Right))
                w += s;

            if (Input.IsKeyHeld(Key.Left))
                w -= s;

            if (Input.IsKeyHeld(Key.Up))
                h += s;

            if (Input.IsKeyHeld(Key.Down))
                h -= s;

            Draw.Reset();
            Draw.Font = Resources.Load<Font>("Amarante-Regular.wf");
            Draw.FontSize = 32;
            Draw.ScreenSpace = true;
            Draw.Texture = Resources.Load<Texture>("texture_test.png");
                
            Draw.ImageMode = ImageMode.Stretch;
            var r = new Rect(new Vector2(Window.Width / 4f, Window.Height / 2f), new Vector2(w, h));
            Draw.Quad(r);
            Draw.Text("Are you sure?", r.GetCenter(), Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Middle, r.Width);

            Draw.ImageMode = ImageMode.Slice;
            r = new Rect(new Vector2(Window.Width / 2f + Window.Width / 4f, Window.Height / 2f), new Vector2(w, h));
            Draw.Quad(r);
            Draw.Text("Are you sure?", r.GetCenter(), Vector2.One, HorizontalTextAlign.Center, VerticalTextAlign.Middle, r.Width);

            Draw.ImageMode = ImageMode.Tiled;
            r = new Rect(new Vector2(Window.Width / 2f, Window.Height / 2f), new Vector2(w, h));
            Draw.Quad(r);
        }
    }
}