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

public struct ImageMemoryLeak : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new ImageMemoryLeakSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color(0x87CCB4)
        });

        game.UpdateRate = 120;
        game.FixedUpdateRate = 8;

        return scene;
    }

    public class ImageMemoryLeakSystem : Walgelijk.System
    {
        const int r = 256;

        public List<Texture> Cycle = new();

        public ImageMemoryLeakSystem()
        {
            for (int i = 0; i < 10; i++)
                Cycle.Add(NewTexture());
        }

        private Texture NewTexture() => TexGen.Checkerboard(r, r, Random.Shared.Next(5, 16), Utilities.RandomColour(), Utilities.RandomColour());

        public override void Update()
        {
            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Text("Press space", new Vector2(10), Vector2.One);

            int i = 0;
            int w = Window.Width / Cycle.Count;
            foreach (var tex in Cycle)
            {
                Draw.Image(tex, new Rect(0, 0, w, Window.Height).Translate(w * i++,0), ImageContainmentMode.Contain);
            }

            if (Input.IsKeyHeld(Key.Space))
            {
                Cycle[^1].Dispose();
                Cycle.RemoveAt(Cycle.Count - 1);
                Cycle.Insert(0, NewTexture());
            }
        }
    }
}