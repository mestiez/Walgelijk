using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Walgelijk;
using Walgelijk.Imgui;
using Walgelijk.Localisation;
using Walgelijk.OpenTK.MotionTK;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public struct TestScene2
{
    private static Video videos;
    private static Sound streamTest;

    public static Scene Load(Game game)
    {
        var scene = new Scene(game);

        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        scene.AddSystem(new TestSystem());
        scene.AddSystem(new VideoSystem());
        scene.AddSystem(new GuiSystem());

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#a8a3c1")
        });

        var audio = game.AudioRenderer.LoadStream("resources/audio/dexter.ogg");
        streamTest = new Sound(audio, true, false);
        game.AudioRenderer.Play(streamTest);

        return scene;
    }

    public class TestSystem : Walgelijk.System
    {
        Stopwatch sw = new();
        int fixedPerSecond = 0;
        Vector2 last;
        Vector2 target;
        private readonly (string, Language)[] langs = Languages.All.Select(static l => (l.DisplayName, l)).ToArray();

        public override void Initialise()
        {
            sw.Start();
        }

        public override void Update()
        {
            if (Input.IsKeyReleased(Key.O))
                Window.IsCursorLocked = !Window.IsCursorLocked;

            if (Input.IsKeyReleased(Key.D1))
                Window.CursorAppearance = DefaultCursor.Default;
            if (Input.IsKeyReleased(Key.D2))
                Window.CursorAppearance = DefaultCursor.Pointer;
            if (Input.IsKeyReleased(Key.D3))
                Window.CursorAppearance = DefaultCursor.Crosshair;

            if (Input.IsKeyReleased(Key.D4))
                Window.CustomCursor = TexGen.Noise(64, 64, 0.7f, 2.354f, Colors.Magenta, Colors.Cyan);

            if (Input.IsKeyReleased(Key.K))
            {
                if (Audio.IsPlaying(streamTest))
                    Audio.Stop(streamTest);
                else
                    Audio.Play(streamTest);
            }

            if (Input.IsKeyReleased(Key.M))
            {
                Audio.Pause(streamTest);
            }

            Draw.Reset();
            Draw.Order = new RenderOrder(50, 0);
            Draw.ScreenSpace = true;
            Draw.Colour = Colors.Purple;
            Draw.Circle(Vector2.Lerp(last, target, Time.Interpolation), new Vector2(25));

            Draw.Colour = Colors.Aqua;
            Draw.Circle(target, new Vector2(15));

            Draw.Order = new RenderOrder(25, 0);
            Draw.Colour = Colors.Blue;
            Draw.Circle(new Vector2((Time.SecondsSinceLoad * 45) % 512), new Vector2(32));

            if (sw.Elapsed.TotalSeconds >= 1d)
            {
                Logger.Debug($"{fixedPerSecond} fixed updates per second");
                fixedPerSecond = 0;
                sw.Restart();
            }

            if (Input.IsKeyHeld(Key.Space))
                Gui.Label("test", new Vector2(64), style: new Style() { Text = Colors.WhiteSmoke });

            Draw.Order = new RenderOrder(120, 0);

            Draw.Colour = Colors.White;
            Draw.Text(Localisation.Get("greeting"), new Vector2(256, 256), Vector2.One);
            Draw.Text(Localisation.Get("solitary"), new Vector2(256, 276), Vector2.One);
            Draw.Image(Localisation.CurrentLanguage.Flag, new Rect(new Vector2(256 - 32, 266), new Vector2(48, 32)), ImageContainmentMode.Stretch);

            Draw.Image(Resources.Load<Texture>(Localisation.Get("advert")), new Rect(new Vector2(612, 256), new Vector2(256)), ImageContainmentMode.Stretch);

            Gui.Dropdown<Language>(new Vector2(256, 32), new Vector2(256, 32), langs, ref Localisation.CurrentLanguage);

            Draw.Reset();
            Draw.Order = new RenderOrder(1000, 0);
            Draw.ScreenSpace = true;
            Draw.Colour = Colors.White;
            if (videos != null)
                Draw.Image(videos.Texture, new Rect(64, 64, 500, 500), ImageContainmentMode.Contain);

            if (Input.IsButtonReleased(Button.Middle))
            {
                if (videos == null)
                {
                    videos = new Video("resources/video/new york.mp4");
                    Scene.AttachComponent(Scene.CreateEntity(), new VideoComponent(videos));
                    videos.Play();
                }
                else
                    videos.Restart();
            }
        }

        public override void FixedUpdate()
        {
            last = target;
            target = Input.WindowMousePosition;
            fixedPerSecond++;
        }
    }
}