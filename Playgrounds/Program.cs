using System.Diagnostics;
using System.Numerics;
using Walgelijk;
using Walgelijk.Onion.Controls;
using Walgelijk.OpenTK;

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
        Resources.SetBasePathForType<FixedAudioData>("audio");
        Resources.SetBasePathForType<StreamAudioData>("audio");
        Resources.SetBasePathForType<Texture>("textures");
        Resources.SetBasePathForType<Font>("fonts");

        game.Scene = new AudioVisScene().Load(game);

#if DEBUG
        game.DevelopmentMode = true;
#else
        game.DevelopmentMode = false;
#endif
        game.Window.SetIcon(Resources.Load<Texture>("icon.png"));
        game.Profiling.DrawQuickProfiler = false;

        game.Start();
    }

    [Command(HelpString = "Set the scene. Pass ?? as an argument to list all available scenes")]
    public static CommandResult SetScene(string name)
    {
        var scenes = typeof(ISceneCreator).Assembly.GetTypes().Where(a => !a.IsAbstract && !a.IsInterface && a.IsAssignableTo(typeof(ISceneCreator)));

        switch (name)
        {
            case "??":
                return string.Join(", ", scenes.Select(t => t.Name));
            default:
                {
                    foreach (var type in scenes)
                        if (type.Name.Equals(name.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            var act = Activator.CreateInstance(type) as ISceneCreator ?? throw new Exception("Invalid scene: not an ISceneCreator");
                            Game.Main.Scene = act.Load(Game.Main);
                            return "Scene set to " + type.Name;
                        }
                    throw new Exception("Invalid scene name");
                }
        }
    }
}

public interface ISceneCreator
{
    public Scene Load(Game game);
}
