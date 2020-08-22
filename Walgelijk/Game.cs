using System;
using System.Threading;
using Walgelijk;

namespace Walgelijk
{
    /// <summary>
    /// The link between the scene and the window
    /// </summary>
    public class Game
    {
        // TODO moet dit wel? singleton game? echt???? fix dit
        /// <summary>
        /// The last instance that was created
        /// </summary>
        public static Game Main { get; private set; }

        private Scene scene;

        /// <summary>
        /// Currently active window
        /// </summary>
        public Window Window { get; }

        /// <summary>
        /// Currently active scene
        /// </summary>
        public Scene Scene
        {
            get => scene; set
            {
                scene = value;
                scene.Game = this;
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
        /// Returns the <see cref="Walgelijk.Time"/> information that belongs to <see cref="Window"/>
        /// </summary>
        public Time Time => Window.Time;

        /// <summary>
        /// Create a game with a window and an optional audio renderer. If the audio renderer is not set, the game won't be able to play any sounds
        /// </summary>
        public Game(Window window, AudioRenderer audioRenderer = null)
        {
            Window = window;
            window.Game = this;
            Main = this;
            AudioRenderer = audioRenderer ?? new EmptyAudioRenderer();
        }

        /// <summary>
        /// Start the game loop
        /// </summary>
        public void Start()
        {
            if (Window == null) throw new InvalidOperationException("Window is null");
            Window.StartLoop();
        }
    }
}
