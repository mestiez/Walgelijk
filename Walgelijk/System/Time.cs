namespace Walgelijk
{
    /// <summary>
    /// Structure that holds frame specific time data
    /// </summary>
    public class Time
    {
        /// <summary>
        /// Returns the amount of seconds that have passed since the last frame
        /// </summary>
        public float DeltaTime { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the game was launched
        /// </summary>
        public float SecondsSinceLoad { get; set; }
        /// <summary>
        /// Returns the amount of seconds that have passed since the last scene change
        /// </summary>
        public float SecondsSinceSceneChange { get; set; }

        /// <summary>
        /// Returns the amount of seconds that have passed since the last frame, unaffected by <see cref="TimeScale"/>
        /// </summary>
        public float DeltaTimeUnscaled { get; set; }
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

        /// <summary>
        /// Interpolation weight factor. Between fixed updates, this value will start at 0 and gradually increase until it ends at 1
        /// </summary>
        public float Interpolation { get; set; } = 0;

        /// <summary>
        /// Intended amount of time in seconds between fixed updates
        /// </summary>
        public float FixedInterval { get; set; } = 0;
    }
}
