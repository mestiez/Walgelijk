using System;

namespace Walgelijk
{
    /// <summary>
    /// Class that calculates the amount of times something needs to execute to adhere to a specific rate
    /// </summary>
    public class FixedIntervalDistributor
    {
        /// <summary>
        /// Preferred cycles rate
        /// </summary>
        public float Rate
        {
            get => updatesPerSecond;
            set
            {
                updatesPerSecond = value;
                Interval = 1f / value;
            }
        }

        /// <summary>
        /// Maximum ouput cycles rate that
        /// </summary>
        public int MaxRate { get; set; } = 256;

        private float timeStepAccumulator = 0;
        private float updatesPerSecond = 60;

        /// <summary>
        /// 1.0f / <see cref="Rate"/>
        /// </summary>
        public float Interval { get; private set; } = 1 / 60f;

        /// <summary>
        /// Calculate the amount of cycles to execute
        /// </summary>
        public int CalculateCycleCount(float realDeltaTime)
        {
            timeStepAccumulator += realDeltaTime;

            int requiredCycles = (int)MathF.Floor(timeStepAccumulator / Interval);

            if (requiredCycles > 0)
                timeStepAccumulator -= requiredCycles * Interval;

            return Math.Min(MaxRate, requiredCycles);
        }
    }
}
