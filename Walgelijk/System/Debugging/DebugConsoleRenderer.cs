using System.Numerics;

namespace Walgelijk
{
    internal class DebugConsoleRenderer
    {
        private readonly DebugConsole debugConsole;
        private readonly TextComponent text;
        private readonly TextComponent overlay;
        private readonly TextComponent inputBox;
        private readonly RectangleShapeComponent background;
        private readonly DrawBoundsTask boundsTask;
        private readonly GroupRenderTask activeConsoleTask;

        private readonly Matrix4x4 textModel = Matrix4x4.CreateScale(1f, -1f, 1);
        private Matrix4x4 model = Matrix4x4.Identity;
        private bool dirtyLog;
        private float overlayLife;
        private float consoleSlideOffset;

        public string InputString { get; set; }
        public Rect TextBounds => text.LocalBoundingBox;

        public const float ConsoleNotificationDuration = 5;
        public const int TotalHeight = 150;
        public const int InputHeight = 20;
        public const int LogHeight = TotalHeight - InputHeight;

        public DebugConsoleRenderer(DebugConsole debugConsole)
        {
            this.debugConsole = debugConsole;

            text = new TextComponent();
            text.TrackingMultiplier = .91f;
            text.LineHeightMultiplier = .7f;
            text.RenderTask.ScreenSpace = true;
            text.Color = new Color(222, 175, 213);

            inputBox = new TextComponent();
            inputBox.TrackingMultiplier = .91f;
            inputBox.LineHeightMultiplier = .7f;
            inputBox.RenderTask.ScreenSpace = true;

            overlay = new TextComponent();
            overlay.TrackingMultiplier = .91f;
            overlay.LineHeightMultiplier = .7f;
            overlay.RenderTask.ScreenSpace = true;

            background = new RectangleShapeComponent();
            background.Color = new Color(173, 85, 156, 230);
            background.Pivot = Vector2.Zero;
            background.RenderTask.ScreenSpace = true;

            boundsTask = new DrawBoundsTask(new DrawBounds(new Vector2(0, LogHeight), Vector2.Zero));

            activeConsoleTask = new GroupRenderTask(
                background.RenderTask,
                inputBox.RenderTask,
                boundsTask,
                text.RenderTask,
                DrawBoundsTask.DisableDrawBoundsTask
             );
        }

        public void Render()
        {
            if (consoleSlideOffset > float.Epsilon)
                RenderActiveConsole();

            float consoleSpeed = debugConsole.Game.Time.RenderDeltaTime * 4;
            consoleSlideOffset += debugConsole.IsActive ? consoleSpeed : -consoleSpeed;
            consoleSlideOffset = Utilities.Clamp(consoleSlideOffset);

            if (debugConsole.DrawConsoleNotification && !debugConsole.IsActive)
                RenderOverlayConsole();
        }

        private void RenderOverlayConsole()
        {
            overlayLife += debugConsole.Game.Time.RenderDeltaTime;

            if (dirtyLog && debugConsole.Log.Count > 0)
            {
                overlayLife = 0;
                overlay.String = debugConsole.Log[^1];
                dirtyLog = false;
            }

            if (overlayLife < ConsoleNotificationDuration)
            {
                overlay.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(
                    debugConsole.Game.Window.Size.X - overlay.LocalBoundingBox.Width - 5,
                    5,
                    0);

                if (overlayLife > ConsoleNotificationDuration * .75f)
                    overlay.Color = Colors.White.WithAlpha(0.5f);
                else
                    overlay.Color = Colors.White;

                debugConsole.Game.RenderQueue.Add(overlay.RenderTask, int.MaxValue);
            }
        }

        private void RenderActiveConsole()
        {
            if (dirtyLog)
            {
                string s = "";
                foreach (var entry in debugConsole.Log)
                    s += entry + '\n';

                text.String = s;

                dirtyLog = false;
            }

            inputBox.String = InputString;

            model = Matrix4x4.CreateTranslation(0, (1 - consoleSlideOffset) * -TotalHeight, 0);

            boundsTask.DrawBounds = new DrawBounds(new Vector2(debugConsole.Game.Window.Size.X, LogHeight), Vector2.Zero);
            text.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(0, -text.LocalBoundingBox.Height + LogHeight + debugConsole.ScrollOffset, 0) * model;
            inputBox.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(5, -InputHeight + TotalHeight, 0) * model;
            background.RenderTask.ModelMatrix = Matrix4x4.CreateScale(debugConsole.Game.Window.Size.X, -TotalHeight, 1) * model;

            var queue = debugConsole.Game.RenderQueue;
            queue.Add(activeConsoleTask, int.MaxValue);
        }

        public void SetDirtyLog()
        {
            dirtyLog = true;
        }
    }
}
