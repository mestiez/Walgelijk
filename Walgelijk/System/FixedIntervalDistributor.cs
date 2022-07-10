using System;

namespace Walgelijk
{
    /// <summary>
    /// Class that calculates the amount of times something needs to execute to adhere to a specific rate
    /// </summary>
    public class FixedIntervalDistributor
    {
        /// <summary>
        /// Preferred rate in Hz
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
        /// Maximum allowed ouput rate per frame
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

            int requiredCycles = 0;
            float interval = Interval;

            while (timeStepAccumulator > interval)
            {
                timeStepAccumulator -= interval;
                requiredCycles++;
            }

            return Math.Min(MaxRate, requiredCycles);
        }
    }
}
