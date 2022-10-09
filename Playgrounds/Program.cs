using System.Diagnostics;
using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;
using Walgelijk.SimpleDrawing;

namespace TestWorld;

public class Program
{
    private static Game game = new Game(
            new OpenTKWindow("playground", new Vector2(-1, -1), new Vector2(800, 600)),
            new OpenALAudioRenderer()
            );

    static void Main(string[] args)
    {
        try
        {
            var p = Process.GetCurrentProcess();
            p.PriorityBoostEnabled = true;
            p.PriorityClass = ProcessPriorityClass.High;
        }
        catch (global::System.Exception e)
        {
            Logger.Warn($"Failed to set process priority: {e}");
        }

        game.UpdateRate = 0;
        game.FixedUpdateRate = 60;
        game.Console.DrawConsoleNotification = true;
        game.Window.VSync = false;

        TextureLoader.Settings.FilterMode = FilterMode.Linear;

        Resources.SetBasePathForType<AudioData>("audio");
        Resources.SetBasePathForType<Texture>("textures");
        Resources.SetBasePathForType<Font>("fonts");

        //game.Scene = SplashScreen.CreateScene(Resources.Load<Texture>("opening_bg.png"), new[]
        //{
        //    new SplashScreen.Logo(Resources.Load<Texture>("splash1.png"), new Vector2(180, 0), 5, new Sound(Resources.Load<AudioData>("opening.wav"), false, false)),
        //    new SplashScreen.Logo(Resources.Load<Texture>("splash2.png"), new Vector2(180, 0), 3f),
        //    new SplashScreen.Logo(Resources.Load<Texture>("splash3.png"), new Vector2(180, 0), 3f),
        //
        //}, () => game.Scene = new AudioWaveScene().Load(game), SplashScreen.Transition.FadeInOut);

        game.Scene = new TextureGeneratorScene().Load(game);

#if DEBUG
        game.DevelopmentMode = true;
#else
        game.DevelopmentMode = false;
#endif
        game.Window.SetIcon(Resources.Load<Texture>("icon.png"));
        game.Profiling.DrawQuickProfiler = false;

        game.Start();
    }
}

public struct TextureGeneratorScene : ISceneCreator
{
    public Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new TextureGeneratorTestSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });
        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#a8a3c1")
        });
        return scene;
    }

    public class TextureGeneratorTestSystem : Walgelijk.System
    {
        private static Texture[] textures =
        {
            TexGen.Colour(128, 128, Color.Red),
            TexGen.Checkerboard(128, 128, 8, Colors.Orange, Colors.Gray),
            TexGen.Noise(128, 128, 0.2123f, 134.2134f, Colors.Red, Colors.Black),
            TexGen.Gradient(128, 128, TexGen.GradientType.Horizontal, Colors.Purple, Colors.GreenYellow),
            TexGen.Gradient(128, 128, TexGen.GradientType.Vertical, Colors.Blue, Colors.Cyan),
            TexGen.Gradient(128, 128, TexGen.GradientType.Diagonal, Colors.Cyan, Colors.Magenta),
            TexGen.Gradient(128, 128, TexGen.GradientType.Spherical, Colors.Yellow, Colors.Red),
            TexGen.Gradient(128, 128, TexGen.GradientType.Radial, Colors.White, Colors.Black),
            TexGen.Gradient(128, 128, TexGen.GradientType.Manhattan, Colors.Blue, Colors.Red),
        };

        public override void Initialise()
        {

        }

        public override void Render()
        {
            Draw.ResetTexture();
            Draw.ScreenSpace = true;

            int maxPerRow = (int)MathF.Min(textures.Length, Window.Size.X / 128);

            if (textures.Length < maxPerRow)
                row(textures, 0, 1);
            else
            {
                var rowCount = (int)MathF.Ceiling(textures.Length / (float)maxPerRow);
                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    row(textures.AsSpan(rowIndex * maxPerRow, Math.Min(maxPerRow, textures.Length - rowIndex * maxPerRow)), rowIndex, rowCount);
                }
            }

            void row(Span<Texture> span, int rowIndex, int rowCount)
            {
                float step = Window.Size.X / span.Length;
                float h = Window.Size.Y / rowCount;
                for (int i = 0; i < span.Length; i++)
                    Draw.Image(span[i], new Rect(i * step, h * (rowIndex), (i + 1) * step, h * (rowIndex + 1)), ImageContainmentMode.Center);
            }
        }
    }
}

public interface ISceneCreator
{
    public Scene Load(Game game);
}
