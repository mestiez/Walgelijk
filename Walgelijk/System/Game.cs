using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Walgelijk;

namespace Walgelijk
{
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

        private Scene scene;

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
            get => scene;

            set
            {
                scene = value;
                if (value != null)
                {
                    scene.Game = this;
                    Logger.Log("Scene changed", nameof(Game));
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
        /// Create a game with a window and an optional audio renderer. If the audio renderer is not set, the game won't be able to play any sounds
        /// </summary>
        public Game(Window window, AudioRenderer audioRenderer = null)
        {
            ExecutableDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
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

            Window.Close();
        }

        [Command]
        private static string Version()
        {
            var assemblyName = Assembly.GetAssembly(typeof(Game)).GetName();
            return $"{assemblyName.Name} v{assemblyName.Version}\n";
        }
    }
}
