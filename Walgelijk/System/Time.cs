namespace Walgelijk
{
    /// <summary>
    /// Structure that holds frame specific time data
    /// </summary>
    public struct Time
    {
        /// <summary>
        /// Returns the amount of seconds that have passed since the last update frame
        /// </summary>
        public float UpdateDeltaTime { get; set; }        
        /// <summary>
        /// Returns the amount of seconds that have passed since the last rendered frame
        /// </summary>
        public float RenderDeltaTime { get; set; }

        /// <summary>
        /// Returns the amount of seconds that have passed since the game was launched
        /// </summary>
        public float SecondsSinceLoad { get; set; }
    }
}
