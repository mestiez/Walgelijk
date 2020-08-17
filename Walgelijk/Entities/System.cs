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
        /// Initialise the system
        /// </summary>
        public abstract void Initialise();

        /// <summary>
        /// Run the logic
        /// </summary>
        public abstract void Execute();
    }
}
