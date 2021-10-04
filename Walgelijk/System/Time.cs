namespace Walgelijk
{
    /// <summary>
    /// Structure that holds frame specific time data
    /// </summary>
    public class Time
    {
        /// <summary>
        /// Returns the amount of seconds that have passed since the last update frame
        /// </summary>
        public float UpdateDeltaTime { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the last rendered frame, unaffected by <see cref="TimeScale"/>
        /// </summary>
        public float RenderDeltaTime { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the game was launched
        /// </summary>
        public float SecondsSinceLoad { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the last scene change
        /// </summary>
        public float SecondsSinceSceneChange { get; set; }

        /// <summary>
        /// Returns the amount of seconds that have passed since the last update frame, unaffected by <see cref="TimeScale"/>
        /// </summary>
        public float UpdateDeltaTimeUnscaled { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the game was launched, unaffected by <see cref="TimeScale"/>
        /// </summary>
        public float SecondsSinceLoadUnscaled { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the last scene change, unaffected by <see cref="TimeScale"/>
        /// </summary>
        public float SecondsSinceSceneChangeUnscaled { get; set; }

        /// <summary>
        /// Factor by which the time is multiplied
        /// </summary>
        public float TimeScale { get; set; } = 1;
}
}
