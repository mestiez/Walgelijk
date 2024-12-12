using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Walgelijk.Audio.SoundCues;

namespace Walgelijk;

/// <summary>
/// The link between the scene and the window
/// </summary>
public class Game
{
    /// <summary>
    /// The last instance that was created
    /// </summary>
    public static Game Main { get; private set; } = null!;

    /// <summary>
    /// Path to the directory where the executable is
    /// </summary>
    public readonly string ExecutableDirectory;

    /// <summary>
    /// Path to where any state should be stored
    /// </summary>
    public readonly string AppDataDirectory;

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
            if (value != null && value.Disposed)
                throw new Exception("This scene has been disposed and can no longer be used");

            if (scene != null)
            {
                scene.Deactivate();
                switch (scene.ScenePersistence)
                {
                    default:
                    case ScenePersistence.Dispose:
                        scene.Dispose();
                        break;
                    case ScenePersistence.Persist:
                        if (!SceneCache.Has(scene.Id))
                            SceneCache.Add(scene);
                        break;
                }
            }

            State.Time.SecondsSinceSceneChange = 0;
            var previous = scene;
            scene = value;

            if (scene != null)
            {
                State.Time.DeltaTime = 0;
                scene.Game = this;
                scene.HasBeenLoadedAlready = true;

                scene.Activate();

                Logger.LogInformation("Scene changed");
            }

            OnSceneChange?.Dispatch((previous, scene));
        }
    }

    /// <summary>
    /// If the scene ID is found in the cache, it is made active. Returns true if found, false if not.
    /// </summary>
    public bool TrySetCachedScene(in SceneId id)
    {
        if (SceneCache.TryGet(id, out var scene))
        {
            Scene = scene;
            return true;
        }
        return false;
    }

    /// <summary>
    /// The scene cache keeps <see cref="ScenePersistence.Persist"/> scenes in memory
    /// </summary>
    public SceneCache SceneCache { get; } = new();

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
    public readonly Hook<(Scene? Old, Scene? New)> OnSceneChange = new();

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

    /// <summary>
    /// The maximum amount of fixed updates per frame
    /// </summary>
    public int MaxFixedUpdatesPerFrame = 10;

    private readonly Stopwatch clock = new();

    /// <summary>
    /// The logger for the game, also used by <see cref="Logger"/>
    /// </summary>
    public readonly ILogger Logger;

    /// <summary>
    /// Additional events to invoke for every game loop cycle
    /// </summary>
    public List<IGameLoopEvent> AdditionalLoopEvents = [];

    /// <summary>
    /// Processes and manages all <see cref="ISoundCue"/>s. This is a layer above the <see cref="AudioRenderer"/> that simplifies audio into playable "cues".
    /// </summary>
    public SoundCueManager SoundCueManager;

    /// <summary>
    /// Create a game with a window and an optional audio renderer. If the audio renderer is not set, the game won't be able to play any sounds
    /// </summary>
    public Game(Window window, AudioRenderer? audioRenderer = null, ILogger? logger = null)
    {
        var entryAssembly = Assembly.GetEntryAssembly() ?? throw new Exception("Could not get entry assembly so a Game instance could not be created");

        ExecutableDirectory = Path.GetDirectoryName(entryAssembly.Location) + Path.DirectorySeparatorChar;
        AppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(AppDataDirectory))
            throw new Exception("SpecialFolder.LocalApplicationData does not return a valid location. Where are we?");
        AppDataDirectory = Path.Combine(AppDataDirectory, entryAssembly.GetName().Name ?? (Random.Shared.Next().ToString("X2")));
        var d = Directory.CreateDirectory(AppDataDirectory);
        if (!d.Exists)
            throw new Exception($"AppDataDirectory could not be created at \"{AppDataDirectory}\"");

        var diskLogDir = Directory.CreateDirectory(Path.Combine(AppDataDirectory, "logs"));

        if (logger == null)
        {
            using ILoggerFactory factory = LoggerFactory.Create(b =>
            {
                b.AddConsole();
                b.AddProvider(new GameConsoleLoggingProvider(this));
                b.AddProvider(new DiskLoggingProvider(diskLogDir));
            });
            logger = factory.CreateLogger(entryAssembly!.GetName()!.Name!);
        }
        Logger = logger;

        Window = window;
        window.Game = this;
        Main = this;
        Resources.Initialise();
        Console = new DebugConsole(this);
        AudioRenderer = audioRenderer ?? new EmptyAudioRenderer();
        Profiling = new Profiler(this);
        DebugDraw = new DebugDraw(this);
        Compositor = new Compositor(this);

        Logger.LogInformation("Executable directory is \"{ExecutableDirectory}\"", ExecutableDirectory);
        Logger.LogInformation("App data directory is \"{AppDataDirectory}\"", AppDataDirectory);
        Logger.LogInformation("{Version}", Version());
        Logger.LogInformation("Display DPI: {DPI}", window.DPI);
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
            AudioRenderer.Process(unscaledDt);
            foreach (var e in AdditionalLoopEvents)
                e.Update(this, unscaledDt);

            double fixedUpdateInterval = 1d / FixedUpdateRate;
            if (!Console.IsActive)
            {
                fixedUpdateClock += dt * State.Time.TimeScale;
                accumulator += scaledDt;
                int iteration = 0;
                while (accumulator > fixedUpdateInterval)
                {
                    Scene?.FixedUpdateSystems();
                    foreach (var e in AdditionalLoopEvents)
                        e.FixedUpdate(this, unscaledDt);
                    fixedUpdateClock = 0;
                    accumulator -= fixedUpdateInterval;
                    iteration++;
                    if (iteration >= MaxFixedUpdatesPerFrame)
                        break;
                }

                State.Time.FixedInterval = (float)fixedUpdateInterval;
                State.Time.Interpolation = (float)(accumulator / fixedUpdateInterval);

                Scene?.UpdateSystems();
                RoutineScheduler.StepRoutines(scaledDt);
                Profiling.Tick();
            }

            SetWindowWorldBounds();

            Scene?.RenderSystems();
            if (DevelopmentMode)
                DebugDraw.Render();

            Compositor.Prepare();
            RenderQueue.RenderAndReset(Window.Graphics);
            Compositor.Render(Window.Graphics);

            Window.Graphics.CurrentTarget = Window.RenderTarget;
            Console.Render();
            Profiling.Render();

            Window.LoopCycle();

            if (!Window.IsOpen)
                break;

            if (UpdateRate != 0)
            {
                var expected = TimeSpan.FromSeconds(1d / UpdateRate);
                while (clock.Elapsed < expected) { }
            }

            dt = clock.Elapsed.TotalSeconds;
            clock.Restart();
        }
        Stop();

        Compositor.Dispose();
        clock.Stop();
        Window.Deinitialise();
        Scene?.Dispose();
    }

    private void SetWindowWorldBounds()
    {
        var topLeft = Window.WindowToWorldPoint(default);
        var bottomRight = Window.WindowToWorldPoint(Window.Size);
        Window.WorldBounds = new Rect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y).SortComponents();
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
    }

    [Command(HelpString = "Provides some control over the compositor at runtime", Alias = "Compositor")]
    private static CommandResult CompositorCmd(string cmd)
    {
        var dict = new Dictionary<string, Func<string>>()
        {
            { "enable", () => { Main.Compositor.Enabled = true; return "Compositor enabled"; } },
            { "disable", () => { Main.Compositor.Enabled = false; return "Compositor disabled"; } },
        };

        if (dict.TryGetValue(cmd, out var action))
            return action();

        return CommandResult.Error("Invalid compositor action. The following are available: " + string.Join(", ", dict.Keys));
    }

    [Command(HelpString = "Prints the game and engine versions")]
    private static string Version()
    {
#if DEBUG
        const string config = "Engine is in DEBUG mode";
#elif RELEASE
        const string config = "Engine is in RELEASE mode";
#endif
        var walgelijk = Assembly.GetAssembly(typeof(Game)) ?? throw new Exception("Walgelijk assembly not found");
        var game = Assembly.GetEntryAssembly() ?? throw new Exception("Game assembly not found");

        var a = $"\tENGINE: {walgelijk.GetName()?.Name ?? "null assembly"} {walgelijk.GetName()?.Version ?? (new Version(0, 0, 0))}\n";
        var b = $"\tGAME: {game.GetName()?.Name ?? "null assembly"} {game.GetName()?.Version ?? (new Version(0, 0, 0))}\n";
        return $"{config}\n" + a + b;
    }
}
