using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Walgelijk;

/// <summary>
/// The link between the scene and the window
/// </summary>
public class Game
{
    /// <summary>
    /// The last instance that was created
    /// </summary>
    public static Game Main { get; private set; }

    /// <summary>
    /// Path to the directory where the executable is
    /// </summary>
    public readonly string ExecutableDirectory;

    private Scene? scene;

    /// <summary>
    /// The developer console
    /// </summary>
    public DebugConsole Console { get; }

    /// <summary>
    /// Currently active window
    /// </summary>
    public Window Window { get; }

    /// <summary>
    /// Currently active scene
    /// </summary>
    public Scene Scene
    {
        get => scene ?? FallbackScene.GetFallbackScene(this);

        set
        {
            if (scene != null && scene.ShouldBeDisposedOnSceneChange)
                scene.Dispose();

            State.Time.SecondsSinceSceneChange = 0;
            scene = value;
            if (scene != null)
            {
                State.Time.DeltaTime = 0;
                scene.Game = this;
                scene.HasBeenLoadedAlready = true;
                Logger.Log("Scene changed", nameof(Game));
                OnSceneChange?.Dispatch(scene);
            }
            else Logger.Log("Scene set to null", nameof(Game));
        }
    }

    /// <summary>
    /// Returns the <see cref="Walgelijk.RenderQueue"/> that belongs to <see cref="Window"/>
    /// </summary>
    public RenderQueue RenderQueue => Window.RenderQueue;

    /// <summary>
    /// The main audio renderer
    /// </summary>
    public AudioRenderer AudioRenderer { get; }

    /// <summary>
    /// Debug drawing utilities
    /// </summary>
    public DebugDraw DebugDraw { get; }

    /// <summary>
    /// The game profiler
    /// </summary>
    public Profiler Profiling { get; }

    /// <summary>
    /// Game engine state information
    /// </summary>
    public GameState State { get; private set; } = new();

    /// <summary>
    /// When set to true, safety checks will be done at runtime. This will degrade performance and should be turned off in release. <b>True by default</b>
    /// </summary>
    public bool DevelopmentMode { get; set; } = true;

    /// <summary>
    /// Event dispatched when the scene is changed. The new scene is passed to the receivers
    /// </summary>
    public readonly Hook<Scene> OnSceneChange = new();

    /// <summary>
    /// The fixed update rate in Hz
    /// </summary>
    public int FixedUpdateRate = 60;
    /// <summary>
    /// The update/render rate in Hz. Uncapped if zero or smaller.
    /// </summary>
    public int UpdateRate = 0;

    private readonly Stopwatch clock = new();

    /// <summary>
    /// Create a game with a window and an optional audio renderer. If the audio renderer is not set, the game won't be able to play any sounds
    /// </summary>
    public Game(Window window, AudioRenderer? audioRenderer = null)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
            throw new Exception("Could not get entry assembly so a Game instance could not be created");
        ExecutableDirectory = Path.GetDirectoryName(entryAssembly.Location) + Path.DirectorySeparatorChar;
        global::System.Console.WriteLine(ExecutableDirectory);
        Window = window;
        window.Game = this;
        Main = this;
        Resources.Initialise();
        Console = new DebugConsole(this);
        AudioRenderer = audioRenderer ?? new EmptyAudioRenderer();
        Profiling = new Profiler(this);
        DebugDraw = new DebugDraw(this);
        Logger.Log(Version());
    }

    /// <summary>
    /// Start the game loop
    /// </summary>
    public void Start()
    {
        if (Window == null)
            throw new InvalidOperationException("Window is null");

        Window.Initialise();
        clock.Start();
        double dt = 0;
        double accumulator = 0;
        double fixedUpdateClock = 0;
        while (true)
        {
            var unscaledDt = (float)dt;
            var scaledDt = (float)dt * State.Time.TimeScale;

            State.Time.DeltaTimeUnscaled = unscaledDt;
            State.Time.DeltaTime = scaledDt;

            State.Time.SecondsSinceSceneChange += unscaledDt;
            State.Time.SecondsSinceSceneChangeUnscaled += scaledDt;

            State.Time.SecondsSinceLoad += scaledDt;
            State.Time.SecondsSinceLoadUnscaled += unscaledDt;

            AudioRenderer.UpdateTracks();
            Console.Update();
            AudioRenderer.Process(this);

            double fixedUpdateInterval = 1d / FixedUpdateRate;
            if (!Console.IsActive)
            {
                fixedUpdateClock += dt * State.Time.TimeScale;
                accumulator += scaledDt;
                while (accumulator > fixedUpdateInterval)
                {
                    Scene?.FixedUpdateSystems();
                    fixedUpdateClock = 0;
                    accumulator -= fixedUpdateInterval;
                }

                State.Time.Interpolation = (float)((fixedUpdateClock % fixedUpdateInterval) / fixedUpdateInterval);

                Scene?.UpdateSystems();
            }

            Profiling.Tick();

            Window.LoopCycle();

            if (!Window.IsOpen)
                break;

            dt = clock.Elapsed.TotalSeconds;
            clock.Restart();
        }
        clock.Stop();
        Window.Deinitialise();
    }

    /// <summary>
    /// Exit the game
    /// </summary>
    public void Stop()
    {
        if (Window == null)
            throw new InvalidOperationException("Window is null");

        AudioRenderer?.Release();
        Window.Close();
        Logger.Dispose();
    }

    [Command]
    private static string Version()
    {
        var assemblyName = Assembly.GetAssembly(typeof(Game))?.GetName() ?? null;
#if DEBUG
        const string config = "DEBUG mode";
#elif RELEASE
        const string config = "RELEASE mode";
#endif
        return $"{assemblyName?.Name ?? "null assembly"} ({config}) v{assemblyName?.Version ?? (new Version(0, 0, 0))}\n";
    }
}
