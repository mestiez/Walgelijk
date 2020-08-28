namespace Walgelijk
{
    /// <summary>
    /// Holds game logic
    /// </summary>
    public abstract class System
    {
        /// <summary>
        /// Containing scene
        /// </summary>
        public Scene Scene { get; internal set; }

        /// <summary>
        /// Current input state
        /// </summary>
        public InputState Input => Scene.Game.Window.InputState;

        /// <summary>
        /// Current time information
        /// </summary>
        public Time Time => Scene.Game.Window.Time;

        /// <summary>
        /// Active render queue
        /// </summary>
        public RenderQueue RenderQueue => Scene.Game.RenderQueue;

        /// <summary>
        /// Active audio renderer
        /// </summary>
        public AudioRenderer Audio => Scene.Game.AudioRenderer;

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
