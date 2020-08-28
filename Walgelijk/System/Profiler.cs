﻿using System.Numerics;

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
        /// <summary>
        /// Enables or disables a small debug performance information display
        /// </summary>
        public bool DrawQuickProfiler { get; set; }
#if DEBUG 
            = true;
#endif

        private readonly Game game;
        private readonly Matrix4x4 quickProfilerModel = Matrix4x4.CreateScale(0.7f) * Matrix4x4.CreateTranslation(5, 5, 0);
        private readonly TextComponent quickProfiler;

        private readonly TickRateCounter upsCounter = new TickRateCounter();
        private readonly TickRateCounter fpsCounter = new TickRateCounter();

        /// <summary>
        /// Create a profiler for the given game
        /// </summary>
        /// <param name="game"></param>
        public Profiler(Game game)
        {
            this.game = game;
            quickProfiler = new TextComponent("?");
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
            if (DrawQuickProfiler)
                RenderQuickProfiler();
        }

        private void RenderQuickProfiler()
        {
            quickProfiler.String = $"{FramesPerSecond} fps\n{UpdatesPerSecond} ups";

            var task = quickProfiler.RenderTask;
            task.ScreenSpace = true;
            task.ModelMatrix = quickProfilerModel;
            game.RenderQueue.Enqueue(task);
        }

        private void CalculateUPS()
        {
            upsCounter.Tick(game.Time.SecondsSinceLoad);
        }

        private void CalculateFPS()
        {
            fpsCounter.Tick(game.Time.SecondsSinceLoad);
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