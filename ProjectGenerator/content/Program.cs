using System.Numerics;
using System.Runtime.Versioning;
using Walgelijk;
using Walgelijk.Localisation;
using Walgelijk.OpenTK;

namespace $name$;

public class Program
{
    private static Game game;

    static void Main(string[] args)
    {
        game = new Game(
            new OpenTKWindow("$name$", new Vector2(-1, -1), new Vector2(1280, 720)),
            new OpenALAudioRenderer()
            );

        game.UpdateRate = 120;
        game.FixedUpdateRate = 60;
        game.Window.VSync = false;

        TextureLoader.Settings.FilterMode = FilterMode.Linear;

        Resources.SetBasePathForType<FixedAudioData>("audio");
        Resources.SetBasePathForType<StreamAudioData>("audio");
        Resources.SetBasePathForType<Texture>("textures");
        Resources.SetBasePathForType<Font>("fonts");
        Resources.SetBasePathForType<Language>("locale");

        game.Scene = MainScene.Load(game);

#if DEBUG
        game.DevelopmentMode = true;
        game.Console.DrawConsoleNotification = true;
#else
        game.DevelopmentMode = false;
        game.Console.DrawConsoleNotification = false;
#endif
        game.Window.SetIcon(Resources.Load<Texture>("icon.png"));
        game.Profiling.DrawQuickProfiler = false;

        game.Start();
    }
}

public readonly struct MainScene
{
    public static Scene Load(Game game)
    {
        var scene = new Scene(game);
        scene.AddSystem(new TransformSystem());
        scene.AddSystem(new ShapeRendererSystem());
        scene.AddSystem(new CameraSystem() { ExecutionOrder = -1 });

        var camera = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new CameraComponent
        {
            PixelsPerUnit = 1,
            OrthographicSize = 1,
            ClearColour = new Color("#272830")
        });
        game.UpdateRate = 120;

        var text = scene.CreateEntity();
        scene.AttachComponent(camera, new TransformComponent());
        scene.AttachComponent(camera, new TextComponent("Hallo Wereld :)", Resources.Load<Font>("inter.fnt")));

        return scene;
    }
}