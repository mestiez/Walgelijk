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

        private readonly Matrix3x2 textModel = Matrix3x2.CreateScale(1f, -1f);
        private Matrix3x2 model = Matrix3x2.Identity;
        private bool dirtyLog;
        private float overlayLife;
        private float consoleSlideOffset;

        private List<ColourInstruction> textColors = new();

        public string InputString { get; set; } = string.Empty;
        public Rect TextBounds => text.LocalBoundingBox;

        public const float ConsoleNotificationDuration = 5;
        public const int TotalHeight = 350;
        public const int InputHeight = 20;
        public const int LogHeight = TotalHeight - InputHeight;
        public const float ConsoleSlideDuration = 0.2f;

        public static Color DefaultTextColour = new Color("#D42C5E");

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
            text.RenderOrder = RenderOrder.Top.WithOrder(int.MaxValue);
            text.ColorInstructions = textColors;
            text.ParseRichText = false;

            inputBox = new TextComponent();
            inputBox.RenderTask.ScreenSpace = true;

            consoleInfo = new TextComponent();
            consoleInfo.RenderTask.ScreenSpace = true;
            consoleInfo.Color = Colors.White.WithAlpha(0.1f);
            consoleInfo.HorizontalAlignment = HorizontalTextAlign.Right;
            consoleInfo.String = "Filter disabled";
            consoleInfo.RenderOrder = RenderOrder.Top.WithOrder(int.MaxValue);

            overlay = new TextComponent();
            overlay.RenderTask.ScreenSpace = true;
            overlay.RenderOrder = RenderOrder.Top.WithOrder(int.MaxValue);

            inputBackground = new RectangleShapeComponent();
            inputBackground.Color = new Color(15, 15, 15);
            inputBackground.Pivot = Vector2.Zero;
            inputBackground.RenderOrder = RenderOrder.Top.WithOrder(int.MaxValue);
            inputBackground.RenderTask.ScreenSpace = true;

            background = new RectangleShapeComponent();
            background.Color = new Color(25, 25, 25);
            background.Pivot = Vector2.Zero;
            background.RenderOrder = RenderOrder.Top.WithOrder(int.MaxValue);
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

            float consoleSpeed = debugConsole.Game.State.Time.DeltaTime / ConsoleSlideDuration;
            consoleSlideOffset += debugConsole.IsActive ? consoleSpeed : -consoleSpeed;
            consoleSlideOffset = Utilities.Clamp(consoleSlideOffset);

            if (debugConsole.DrawConsoleNotification && !debugConsole.IsActive)
                RenderOverlayConsole();
        }

        private void RenderOverlayConsole()
        {
            overlayLife += debugConsole.Game.State.Time.DeltaTime;

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
                overlay.RenderTask.ModelMatrix = textModel *
                    Matrix3x2.CreateTranslation(debugConsole.Game.Window.Size.X - overlay.LocalBoundingBox.Width - 5, 5);

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
            model = Matrix3x2.CreateTranslation(0, yPos);

            boundsTask.DrawBounds = new DrawBounds(new Vector2(debugConsole.Game.Window.Size.X, Utilities.Clamp(LogHeight + yPos, float.Epsilon, LogHeight)), default);

            text.RenderTask.ModelMatrix = textModel * Matrix3x2.CreateTranslation(5, -text.LocalBoundingBox.Height + LogHeight + debugConsole.ScrollOffset - 5) * model;

            inputBackground.RenderTask.ModelMatrix = Matrix3x2.CreateScale(debugConsole.Game.Window.Size.X, -InputHeight) * Matrix3x2.CreateTranslation(0, -InputHeight + TotalHeight) * model;
            inputBox.RenderTask.ModelMatrix = textModel * Matrix3x2.CreateTranslation(5, -InputHeight + TotalHeight + 1) * model;
            background.RenderTask.ModelMatrix = Matrix3x2.CreateScale(debugConsole.Game.Window.Size.X, -TotalHeight) * model;
            consoleInfo.RenderTask.ModelMatrix = textModel * Matrix3x2.CreateScale(1.5f) * Matrix3x2.CreateTranslation(debugConsole.Game.Window.Size.X - 10, 10);

            var queue = debugConsole.Game.RenderQueue;
            queue.Add(activeConsoleTask, RenderOrder.Top.WithOrder(int.MaxValue));
        }

        public void SetDirty()
        {
            dirtyLog = true;
        }
    }
}
