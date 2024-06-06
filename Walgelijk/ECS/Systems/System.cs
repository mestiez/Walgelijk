using System;

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
        /// Should this be updated and rendered? 
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Containing scene
        /// </summary>
        public Scene Scene { get; internal set; }

        /// <summary>
        /// Current input state
        /// </summary>
        protected InputState Input => Scene.Game.State.Input;

        /// <summary>
        /// Current time information
        /// </summary>
        protected Time Time => Scene.Game.State.Time;

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
        /// The order of execution relative to other systems. The lower, the earlier it gets executed.
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
        /// Separate ordering value that is determined by the chronological order of this sytem within the collection
        /// </summary>
        internal int OrderOfAddition = 0;

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
        /// Run logic at <see cref="Game.UpdateRate"/> Hz
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Run logic at <see cref="Game.FixedUpdateRate"/> Hz
        /// </summary>
        public virtual void FixedUpdate() { }

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

        /// <summary>
        /// Invoked when the scene becomes active
        /// </summary>
        public virtual void OnActivate() { }  
        
        /// <summary>
        /// Invoked when the scene becomes inactive
        /// </summary>
        public virtual void OnDeactivate() { }
    }
}
