using System.Collections.Generic;
using System.Numerics;
using static Walgelijk.TextMeshGenerator;

namespace Walgelijk
{
    internal class DebugConsoleRenderer
    {
        private readonly DebugConsole debugConsole;
        private readonly TextComponent text;
        private readonly TextComponent overlay;
        private readonly TextComponent inputBox;
        private readonly TextComponent consoleInfo;
        private readonly RectangleShapeComponent background;
        private readonly RectangleShapeComponent inputBackground;

        private readonly DrawBoundsTask boundsTask;
        private readonly GroupRenderTask activeConsoleTask;

        private readonly Matrix4x4 textModel = Matrix4x4.CreateScale(1f, -1f, 1);
        private Matrix4x4 model = Matrix4x4.Identity;
        private bool dirtyLog;
        private float overlayLife;
        private float consoleSlideOffset;

        private List<ColourInstruction> textColors = new List<ColourInstruction>();

        public string InputString { get; set; } = string.Empty;
        public Rect TextBounds => text.LocalBoundingBox;

        public const float ConsoleNotificationDuration = 5;
        public const int TotalHeight = 350;
        public const int InputHeight = 20;
        public const int LogHeight = TotalHeight - InputHeight;
        public const float ConsoleSlideDuration = 0.2f;

        public static Color DefaultTextColour = new Color("#D42C5E");

        public struct LevelColours
        {

        }

        public DebugConsoleRenderer(DebugConsole debugConsole)
        {
            this.debugConsole = debugConsole;

            textColors.Clear();
            textColors.Add(new ColourInstruction { CharIndex = 0, Colour = DefaultTextColour });

            text = new TextComponent();
            text.TrackingMultiplier = 1f;
            //text.LineHeightMultiplier = .7f;
            text.RenderTask.ScreenSpace = true;
            text.Color = Colors.White;
            text.ColorInstructions = textColors;
            text.ParseRichText = false;

            inputBox = new TextComponent();
            inputBox.RenderTask.ScreenSpace = true;

            consoleInfo = new TextComponent();
            consoleInfo.RenderTask.ScreenSpace = true;
            consoleInfo.Color = Colors.White.WithAlpha(0.1f);
            consoleInfo.HorizontalAlignment = HorizontalTextAlign.Right;
            consoleInfo.String = "Filter disabled";

            overlay = new TextComponent();
            overlay.RenderTask.ScreenSpace = true;

            inputBackground = new RectangleShapeComponent();
            inputBackground.Color = new Color(15, 15, 15);
            inputBackground.Pivot = Vector2.Zero;
            inputBackground.RenderTask.ScreenSpace = true;

            background = new RectangleShapeComponent();
            background.Color = new Color(25, 25, 25);
            background.Pivot = Vector2.Zero;
            background.RenderTask.ScreenSpace = true;

            boundsTask = new DrawBoundsTask(new DrawBounds(new Vector2(0, LogHeight), Vector2.Zero));

            activeConsoleTask = new GroupRenderTask(
                background.RenderTask,
                inputBackground.RenderTask,
                inputBox.RenderTask,
                boundsTask,
                text.RenderTask,
                consoleInfo.RenderTask,
                DrawBoundsTask.DisableDrawBoundsTask
             );
        }

        public void Render()
        {
            if (consoleSlideOffset > float.Epsilon)
                RenderActiveConsole();

            float consoleSpeed = debugConsole.Game.Time.DeltaTime / ConsoleSlideDuration;
            consoleSlideOffset += debugConsole.IsActive ? consoleSpeed : -consoleSpeed;
            consoleSlideOffset = Utilities.Clamp(consoleSlideOffset);

            if (debugConsole.DrawConsoleNotification && !debugConsole.IsActive)
                RenderOverlayConsole();
        }

        private void RenderOverlayConsole()
        {
            overlayLife += debugConsole.Game.Time.DeltaTime;

            if (dirtyLog && debugConsole.Log.Count > 0)
            {
                overlayLife = 0;
                var e = debugConsole.Log[^1];
                overlay.Color = e.color;
                overlay.String = e.message;
                dirtyLog = false;
            }

            if (overlayLife < ConsoleNotificationDuration && overlay.RenderTask != null)
            {
                overlay.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(
                    debugConsole.Game.Window.Size.X - overlay.LocalBoundingBox.Width - 5,
                    5,
                    0);

                if (overlayLife > ConsoleNotificationDuration * .75f)
                    overlay.Color = Colors.White.WithAlpha(0.5f);
                else
                    overlay.Color = Colors.White;

                debugConsole.Game.RenderQueue.Add(overlay.RenderTask, RenderOrder.DebugUI.WithOrder(1));
            }
        }

        private void RenderActiveConsole()
        {
            if (dirtyLog)
            {
                string s = "";
                Color c = DefaultTextColour;
                textColors.Clear();
                textColors.Add(new ColourInstruction { CharIndex = 0, Colour = c });

                foreach (var entry in debugConsole.Log)
                {
                    if ((entry.type & debugConsole.Filter) != entry.type)
                        continue;

                    if (entry.color != c)
                    {
                        c = entry.color;
                        textColors.Add(new ColourInstruction { CharIndex = s.Length, Colour = entry.color });
                    }
                    s += entry.message + '\n';
                }

                text.String = s;

                dirtyLog = false;

                consoleInfo.String = $"Filter: {debugConsole.Filter}"; 
            }

            inputBox.String = InputString;

            float yPos = (1 - /*Easings.Cubic.Out*/(consoleSlideOffset)) * -TotalHeight;
            model = Matrix4x4.CreateTranslation(0, yPos, 0);

            boundsTask.DrawBounds = new DrawBounds(new Vector2(debugConsole.Game.Window.Size.X, Utilities.Clamp(LogHeight + yPos, float.Epsilon, LogHeight)), default);

            text.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(5, -text.LocalBoundingBox.Height + LogHeight + debugConsole.ScrollOffset - 5, 0) * model;

            inputBackground.RenderTask.ModelMatrix = Matrix4x4.CreateScale(debugConsole.Game.Window.Size.X, -InputHeight, 1) * Matrix4x4.CreateTranslation(0, -InputHeight + TotalHeight, 0) * model;
            inputBox.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateTranslation(5, -InputHeight + TotalHeight + 1, 0) * model;
            background.RenderTask.ModelMatrix = Matrix4x4.CreateScale(debugConsole.Game.Window.Size.X, -TotalHeight, 1) * model;
            consoleInfo.RenderTask.ModelMatrix = textModel * Matrix4x4.CreateScale(1.5f) * Matrix4x4.CreateTranslation(debugConsole.Game.Window.Size.X - 10, 10, 0);

            var queue = debugConsole.Game.RenderQueue;
            queue.Add(activeConsoleTask, RenderOrder.DebugUI.WithOrder(1));
        }

        public void SetDirty()
        {
            dirtyLog = true;
        }
    }
}
