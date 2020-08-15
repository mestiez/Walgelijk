namespace Walgelijk
{
    /// <summary>
    /// Holds game logic
    /// </summary>
    public interface ISystem
    {
        /// <summary>
        /// Containing scene
        /// </summary>
        public Scene Scene { get; set; }

        /// <summary>
        /// Initialise the system
        /// </summary>
        public void Initialise();

        /// <summary>
        /// Run the logic
        /// </summary>
        public void Execute();
    }
}
