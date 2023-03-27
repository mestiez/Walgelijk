using System.Numerics;
using Walgelijk;
using Walgelijk.OpenTK;

namespace Walgelijk.Template;

public class Program
{
    public static Game Game = null!;

    public static void Main(string[] args)
    {
        Game = new Game(
            new OpenTKWindow("Walgelijk.Template", new Vector2(-1), new Vector2(1280, 720)),
            new OpenALAudioRenderer()
        );

        Game.UpdateRate = 120;
        Game.FixedUpdateRate = 60;
        Game.Window.VSync = false;

        TextureLoader.Settings.FilterMode = FilterMode.Linear;

        Resources.SetBasePathForType<FixedAudioData>("audio");
        Resources.SetBasePathForType<StreamAudioData>("audio");
        Resources.SetBasePathForType<Texture>("textures");
        Resources.SetBasePathForType<Font>("fonts");

        var scene = Game.Scene = new Scene(Game);
        scene.AddSystem(new CameraSystem());
        scene.AddSystem(new TransformSystem());

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent()
            {
                PixelsPerUnit = 1,
                OrthographicSize = 1,
                ClearColour = new Color("#2c2436")
            }
        );

#if DEBUG
        Game.DevelopmentMode = true;
        Game.Console.DrawConsoleNotification = true;
#else
        Game.DevelopmentMode = false;
        Game.Console.DrawConsoleNotification = false;
#endif

        Game.Window.SetIcon(Resources.Load<Texture>("icon.png"));
        Game.Profiling.DrawQuickProfiler = false;

        Game.Start();
    }
}
