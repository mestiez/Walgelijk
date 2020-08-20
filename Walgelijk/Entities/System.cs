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
        /// Initialise the system
        /// </summary>
        public abstract void Initialise();

        /// <summary>
        /// Run the logic
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Run rendering code
        /// </summary>
        public abstract void Render();
    }
}
