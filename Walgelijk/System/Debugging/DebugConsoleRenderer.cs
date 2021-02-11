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
        private readonly RectangleShapeComponent inputBackground;

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
        public const int TotalHeight = 350;
        public const int InputHeight = 20;
        public const int LogHeight = TotalHeight - InputHeight;
        public const float ConsoleSlideDuration = 0.3f;

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

            inputBackground = new RectangleShapeComponent();
            inputBackground.Color = new Color(173, 85, 156, 230);
            inputBackground.Pivot = Vector2.Zero;
            inputBackground.RenderTask.ScreenSpace = true;

            background = new RectangleShapeComponent();
            background.Color = (0.9f * inputBackground.Color).WithAlpha(0.9f); 
            background.Pivot = Vector2.Zero;
            background.RenderTask.ScreenSpace = true;

            boundsTask = new DrawBoundsTask(new DrawBounds(new Vector2(0, LogHeight), Vector2.Zero));

            activeConsoleTask = new GroupRenderTask(
                background.RenderTask,
                inputBackground.RenderTask,
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

            float consoleSpeed = debugConsole.Game.Time.RenderDeltaTime / ConsoleSlideDuration;
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

                debugConsole.Game.RenderQueue.Add(overlay.RenderTask, DefaultLayers.DebugUI.WithOrder(1));
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

            float yPos = (1 - consoleSlideOffset) * -TotalHeight;
            model = Matrix4x4.CreateTranslation(0, yPos, 0);

            boundsTask.DrawBounds = new DrawBounds(new Vector2(debugConsole.Game.Window.Size.X, Utilities.Clamp(LogHeight + yPos, float.Epsilon, LogHeight)), default);

            text.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(5, -text.LocalBoundingBox.Height + LogHeight + debugConsole.ScrollOffset - 5, 0) * model;

            inputBackground.RenderTask.ModelMatrix = Matrix4x4.CreateScale(debugConsole.Game.Window.Size.X, -InputHeight, 1) * Matrix4x4.CreateTranslation(0, -InputHeight + TotalHeight, 0) * model;
            inputBox.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(5, -InputHeight + TotalHeight + 1, 0) * model;
            background.RenderTask.ModelMatrix = Matrix4x4.CreateScale(debugConsole.Game.Window.Size.X, -TotalHeight, 1) * model;

            var queue = debugConsole.Game.RenderQueue;
            queue.Add(activeConsoleTask, DefaultLayers.DebugUI.WithOrder(1));
        }

        public void SetDirtyLog()
        {
            dirtyLog = true;
        }
    }
}
