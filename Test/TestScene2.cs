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

    public const int BufferCollectionSize = 3;
    public const int BufferSize = 1024;
    private static byte[] samples = new byte[BufferSize];
    private static float[] samplesCast = new float[BufferSize];

    private static float[] rawCollection = new float[BufferCollectionSize * BufferSize];
    private static float[] averaged = new float[BufferCollectionSize * BufferSize];
    private static float[] fft = new float[BufferCollectionSize * BufferSize];
    private static float[] visualiser = new float[BufferCollectionSize * BufferSize];
    private static int sampleSetCounter = 0;

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

        streamTest = new Sound(Resources.Load<StreamAudioData>("Party_Level_Theme_4.ogg"), false, false);
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

        public static void FrequencyWarp(ReadOnlySpan<float> input, Span<float> warped, float minFrequency, float maxFrequency)
        {
            for (int i = 0; i < input.Length; i++)
            {
                float frequency = minFrequency + (maxFrequency - minFrequency) * (float)i / (float)input.Length;
                warped[i] = input[i] * MathF.Log(frequency);
            }
        }

        public override void Update()
        {
            Draw.Reset();
            Draw.Order = new RenderOrder(350, 0);
            Draw.ScreenSpace = true;
            Draw.Colour = Colors.Purple;
            int length = Audio.GetCurrentSamples(streamTest, samples);
            for (int i = 0; i < length; i++)
                samplesCast[i] = Utilities.MapRange(0, byte.MaxValue, -0.5f, 0.5f, samples[i]);

            if (sampleSetCounter >= BufferCollectionSize)
            {
                for (int i = 0; i < length; i++)
                    averaged[i] = rawCollection[i];
                Array.Clear(samplesCast);
                for (int i = 0; i < 0; i++)
                    AudioAnalysis.BlurSignal(averaged);
                AudioAnalysis.Fft(averaged.AsSpan(0, length), fft.AsSpan(0, length));
                sampleSetCounter = 0;
                Array.Reverse(fft);
            }
            else
            {
                samplesCast.CopyTo(rawCollection, sampleSetCounter * BufferSize);
                sampleSetCounter++;
            }

            const float minFrequency = 20;
            const float maxFrequency = 2000;
            for (int i = 0; i < visualiser.Length; i++)
            {
                //float frequency = minFrequency + (maxFrequency - minFrequency) * (float)i / (float)visualiser.Length;
                //visualiser[i] = 
                if (Input.IsKeyHeld(Key.Space))
                    visualiser[i] = Utilities.SmoothApproach(visualiser[i], fft[i], 64, Time.DeltaTime);
                else
                    visualiser[i] = Utilities.SmoothApproach(
                        visualiser[i],
                        fft[(int)Utilities.Clamp(MathF.Pow(i / (float)visualiser.Length, 2f) * visualiser.Length, 0, visualiser.Length)],
                        64, Time.DeltaTime);
            }

            int index = 0;
            for (int i = 20; i < visualiser.Length / 2; i += 1)
            {
                var f = Utilities.Clamp(MathF.Abs(visualiser[i]) * 0.05f, 0, 150);// Utilities.MapRange(0, byte.MaxValue, -1, 1f, samples[i]);
                if (f > .01f)
                {
                    var a = new Vector2(15 + index * 1.6f, 250);
                    var b = new Vector2(15 + index * 1.6f, 250 - MathF.Log10(f + 1) * 50);
                    Draw.Colour = Color.FromHsv(index * 0.01f, 1, 1).WithAlpha(f * f * 5 + 0.2f);
                    Draw.Line(a, b, 3);
                }
                index++;
            }

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

            if (Input.IsKeyReleased(Key.Period))
                Audio.SetTime(streamTest, Audio.GetTime(streamTest) + 1f);
            if (Input.IsKeyReleased(Key.Comma))
                Audio.SetTime(streamTest, Audio.GetTime(streamTest) - 1f);

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