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
        /// Initialise the system
        /// </summary>
        public abstract void Initialise();

        /// <summary>
        /// Run the logic
        /// </summary>
        public abstract void Execute();
    }
}
