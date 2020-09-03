using System;
using System.Numerics;

namespace Walgelijk
{
    internal class QuickProfiler
    {
        private const float Padding = 5;

        private readonly Matrix4x4 textModel = Matrix4x4.CreateScale(1f, -1f, 1) * Matrix4x4.CreateTranslation(Padding, Padding, 0);
        private readonly TextComponent text;
        private readonly RectangleShapeComponent background;
        private readonly Profiler profiler;

        public QuickProfiler(Profiler profiler)
        {
            text = new TextComponent("?");
            text.TrackingMultiplier = .91f;
            text.RenderTask.ScreenSpace = true;

            background = new RectangleShapeComponent();
            background.Color = new Color(0, 0, 0, 0.2f);
            background.Pivot = Vector2.Zero;
            background.RenderTask.ScreenSpace = true;

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

            text.RenderTask.ModelMatrix = textModel;

            Rect textBounds = text.LocalBoundingBox;
            background.RenderTask.ModelMatrix = Matrix4x4.CreateScale(new Vector3(textBounds.Width + (2 * Padding), -textBounds.Height - (2 * Padding), 1));

            queue.Add(background.RenderTask, int.MaxValue);
            queue.Add(text.RenderTask, int.MaxValue);
        }
    }
}
