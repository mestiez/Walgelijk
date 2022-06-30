using System;
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

            Time.SecondsSinceSceneChange = 0;
            scene = value;
            if (scene != null)
            {
                Time.DeltaTime = 0;
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
    /// Returns the <see cref="Walgelijk.Time"/> information that belongs to <see cref="Window"/>
    /// </summary>
    public Time Time => Window.Time;

    /// <summary>
    /// When set to true, safety checks will be done at runtime. This will degrade performance and should be turned off in release. <b>True by default</b>
    /// </summary>
    public bool DevelopmentMode { get; set; } = true;

    /// <summary>
    /// Event dispatched when the scene is changed. The new scene is passed to the receivers
    /// </summary>
    public readonly Hook<Scene> OnSceneChange = new();

    /// <summary>
    /// Create a game with a window and an optional audio renderer. If the audio renderer is not set, the game won't be able to play any sounds
    /// </summary>
    public Game(Window window, AudioRenderer? audioRenderer = null)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
            throw new Exception("Could not gety entry assembly so a Game instance could not be created");
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
        if (Window == null) throw new InvalidOperationException("Window is null");
        Window.StartLoop();
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
        return $"{assemblyName?.Name ?? "null assembly"} v{assemblyName?.Version ?? (new global::System.Version(0, 0, 0))}\n";
    }
}

internal struct FallbackScene
{
    private static Scene? fallbackScene;

    public static Scene GetFallbackScene(Game game)
    {
        if (fallbackScene != null)
            return fallbackScene;

        fallbackScene = new Scene(game);

        var camera = fallbackScene.CreateEntity();
        fallbackScene.AttachComponent(camera, new TransformComponent());
        fallbackScene.AttachComponent(camera, new CameraComponent
        {
            Clear = true,
            ClearColour = Colors.Black,
            OrthographicSize = 1,
            PixelsPerUnit = 128,
        });

        var text = fallbackScene.CreateEntity();
        fallbackScene.AttachComponent(text, new TransformComponent());
        fallbackScene.AttachComponent(text, new TextComponent("No scene assigned"));

        fallbackScene.AddSystem(new TransformSystem());
        fallbackScene.AddSystem(new ShapeRendererSystem());
        fallbackScene.AddSystem(new CameraSystem());

        return fallbackScene;
    }
}
