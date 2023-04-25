using System.Diagnostics;
using System.Numerics;
using Walgelijk;
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
        Resources.SetBasePathForType<Texture>("textures");
        Resources.SetBasePathForType<Font>("fonts");

        game.Scene = new PhysicsTestScene().Load(game);

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

public interface ISceneCreator
{
    public Scene Load(Game game);
}
