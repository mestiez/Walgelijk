namespace Walgelijk
{
    /// <summary>
    /// Holds game logic
    /// </summary>
    public abstract class System
    {
        private int executionOrder;
        internal bool ExecutionOrderChanged { get; set; }

        /// <summary>
        /// Containing scene
        /// </summary>
        public Scene Scene { get; internal set; }

        /// <summary>
        /// Current input state
        /// </summary>
        protected InputState Input => Scene.Game.Window.InputState;

        /// <summary>
        /// Current time information
        /// </summary>
        protected Time Time => Scene.Game.Window.Time;

        /// <summary>
        /// Active render queue
        /// </summary>
        protected RenderQueue RenderQueue => Scene.Game.RenderQueue;

        /// <summary>
        /// Active audio renderer
        /// </summary>
        protected AudioRenderer Audio => Scene.Game.AudioRenderer;

        /// <summary>
        /// Relevant game instance
        /// </summary>
        protected Game Game => Scene.Game;

        /// <summary>
        /// Relevant window instance
        /// </summary>
        protected Window Window => Game.Window;

        /// <summary>
        /// Relevant graphics instance
        /// </summary>
        protected IGraphics Graphics => Window.Graphics;

        /// <summary>
        /// The parallel group ID that this system belongs to. 0 means main thread.
        /// </summary>
        public int ParallelGroup { get; internal set; } = 0;

        /// <summary>
        /// The order of execution relative to other systems
        /// </summary>
        public int ExecutionOrder
        {
            get => executionOrder;
            set
            {
                if (executionOrder != value)
                    ExecutionOrderChanged = true;
                executionOrder = value;
            }
        }

        /// <summary>
        /// Is the game running in dev mode?
        /// </summary>
        protected bool DevelopmentMode => Scene.Game.DevelopmentMode;

        /// <summary>
        /// Debug drawing utilities
        /// </summary>
        protected DebugDraw DebugDraw => Scene.Game.DebugDraw;

        /// <summary>
        /// The active profiler
        /// </summary>
        protected Profiler Profiler => Scene.Game.Profiling;

        /// <summary>
        /// Initialise the system
        /// </summary>
        public virtual void Initialise() { }

        /// <summary>
        /// Run the logic
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Run pre rendering code
        /// </summary>
        public virtual void PreRender() { }

        /// <summary>
        /// Run rendering code
        /// </summary>
        public virtual void Render() { }

        /// <summary>
        /// Run post rendering code
        /// </summary>
        public virtual void PostRender() { }
    }
}
