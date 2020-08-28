namespace Walgelijk
{
    /// <summary>
    /// Provides performance information
    /// </summary>
    public sealed class Profiler
    {
        /// <summary>
        /// Amount of updates in the last second
        /// </summary>
        public float UpdatesPerSecond => upsCounter.Frequency;
        /// <summary>
        /// Amount of frames rendered in the last second
        /// </summary>
        public float FramesPerSecond => fpsCounter.Frequency;

        private readonly Game game;
        private readonly VertexBuffer profilerText;

        private readonly TickRateCounter upsCounter = new TickRateCounter();
        private readonly TickRateCounter fpsCounter = new TickRateCounter();

        /// <summary>
        /// Create a profiler for the given game
        /// </summary>
        /// <param name="game"></param>
        public Profiler(Game game)
        {
            this.game = game;

            profilerText = new VertexBuffer();
        }

        /// <summary>
        /// Force the profiler to update. Should be handled by the window.
        /// </summary>
        public void Update()
        {
            CalculateUPS();
        }

        /// <summary>
        /// Force the profiler to calculate render information. Should be handled by the window.
        /// </summary>
        public void Render()
        {
            CalculateFPS();

        }

        private void CalculateUPS()
        {
            upsCounter.Tick(game.Time.SecondsSinceStart);
        }

        private void CalculateFPS()
        {
            fpsCounter.Tick(game.Time.SecondsSinceStart);
        }
    }

    internal class TickRateCounter
    {
        public float Frequency { get; private set; }

        public float MeasureInterval { get; set; } = 1f;

        private int counter;
        private float lastMeasurementTime;

        public void Tick(float currentTime)
        {
            counter++;
            if ((currentTime - lastMeasurementTime) > MeasureInterval)
            {
                lastMeasurementTime = currentTime;
                Frequency = counter / MeasureInterval;
                counter = 0;
            }
        }
    }
}
