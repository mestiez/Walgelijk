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
        public float DeltaTime { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the game was launched
        /// </summary>
        public float SecondsSinceStart { get; set; }
    }
}
