using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
    /// The frame compositor. This is where post processing effects are applied
    /// </summary>
    public Compositor Compositor { get; }

    /// <summary>
    /// When set to true, safety checks will be done at runtime. 
    /// This will degrade performance and should be turned off in release. <b>True by default</b>
    /// </summary>
    public bool DevelopmentMode { get; set; } = true;

    /// <summary>
    /// Event dispatched when the scene is changed. The new scene is passed to the receivers
    /// </summary>
    public readonly Hook<Scene> OnSceneChange = new();

    /// <summary>
    /// Event dispatched when the game is about to close but hasn't yet done any cleanup
    /// </summary>
    public readonly Hook BeforeExit = new();

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
        Compositor = new Compositor(this);
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
                const int maxPerFrame = 10;
                fixedUpdateClock += dt * State.Time.TimeScale;
                accumulator += scaledDt;
                int iteration = 0;
                while (accumulator > fixedUpdateInterval)
                {
                    Scene?.FixedUpdateSystems();
                    fixedUpdateClock = 0;
                    accumulator -= fixedUpdateInterval;
                    iteration++;
                    if (iteration >= maxPerFrame)
                    {
                        Logger.Warn("Amount of FixedUpdates per frame has reached the limit of 10!");
                        break;
                    }
                }

                State.Time.FixedInterval = (float)fixedUpdateInterval;
                State.Time.Interpolation = (float)(accumulator / fixedUpdateInterval);
                //State.Time.Interpolation = (float)((fixedUpdateClock % fixedUpdateInterval) / fixedUpdateInterval);

                Scene?.UpdateSystems();
            }

            Profiling.Tick();
            Compositor.Render(Window.RenderQueue);
            Window.LoopCycle();

            if (!Window.IsOpen)
                break;

            if (UpdateRate != 0)
            {
                //var timeToSleep = TimeSpan.FromSeconds(1d / UpdateRate - clock.Elapsed.TotalSeconds);
                var expected = TimeSpan.FromSeconds(1d / UpdateRate);
                var msToSleep = expected.TotalMilliseconds - clock.Elapsed.TotalMilliseconds;
                //Logger.Debug($"TTS: {timeToSleep.TotalSeconds} s because we need to wait {1d/UpdateRate} s and the frame took {elapsed.TotalSeconds} s.");
                if (msToSleep > 1)
                    Thread.Sleep((int)msToSleep / 2); //Waarom deel ik door twee?
                while (clock.Elapsed < expected) 
                    Thread.Sleep(0); //Dit is niet echt slapen.. het gebruikt alsnog CPU maar het is nodig voor de laatste beetjes om de wachttijd perfect te maken
            }

            dt = clock.Elapsed.TotalSeconds;
            //Logger.Debug($"Frame duration: {dt} s.");
            clock.Restart();
        }
        Stop();

        clock.Stop();
        Window.Deinitialise();
        Scene?.Dispose();
    }

    /// <summary>
    /// Exit the game
    /// </summary>
    public void Stop()
    {
        BeforeExit.Dispatch();

        if (Window == null)
            throw new InvalidOperationException("Window is null");

        AudioRenderer?.Release();
        Window.Close();
        Logger.Dispose();
    }

    [Command(HelpString ="Prints the game and engine versions")]
    private static string Version()
    {
#if DEBUG
        const string config = "DEBUG mode";
#elif RELEASE
        const string config = "RELEASE mode";
#endif
        var walgelijk = Assembly.GetAssembly(typeof(Game)) ?? throw new Exception("Walgelijk assembly not found");
        var game = Assembly.GetEntryAssembly() ?? throw new Exception("Game assembly not found");

        var a = $"\tENGINE: {walgelijk.GetName()?.Name ?? "null assembly"} {walgelijk.GetName()?.Version ?? (new Version(0, 0, 0))}\n";
        var b = $"\tGAME: {game.GetName()?.Name ?? "null assembly"} {game.GetName()?.Version ?? (new Version(0, 0, 0))}\n";
        return $"{config} mode\n" + a + b;
    }
}
