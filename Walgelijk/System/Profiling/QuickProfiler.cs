using System;
using System.Numerics;

namespace Walgelijk
{
    internal class QuickProfiler
    {
        private readonly Matrix4x4 model = Matrix4x4.CreateScale(1f, -1f, 1) * Matrix4x4.CreateTranslation(5, 5, 0);
        private readonly TextComponent text;
        private readonly Profiler profiler;

        public QuickProfiler(Profiler profiler)
        {
            text = new TextComponent("?");
            text.TrackingMultiplier = .91f;
            text.RenderTask.ScreenSpace = true;
            this.profiler = profiler;
        }

        public void Render(RenderQueue queue)
        {
            float frameTime = MathF.Round(1000 / profiler.UpdatesPerSecond, 3);
            float renderTime = MathF.Round(1000 / profiler.FramesPerSecond, 3);

            text.String = 
                $"frame time {frameTime}ms\n" +
                $"render time {renderTime}ms\n" +
                $"{queue.Length} render tasks";

            text.RenderTask.ModelMatrix = model;
            queue.Add(text.RenderTask, int.MaxValue);
        }

    }
}
