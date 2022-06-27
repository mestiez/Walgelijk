using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct ProgressBar
    {
        public static void Process(Vector2 topLeft, Vector2 size, float progress0to1, bool showPercentage = false, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(nameof(ProgressBar).GetHashCode(), showPercentage.GetHashCode(), site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            PositioningUtils.ApplyRaycastRect(id, id.DrawBounds.ToRect(), false);

            Gui.PrepareDrawer();
            var t = Easings.Cubic.Out(Gui.Context.UnscaledTime % 1f);

            var bg = Gui.GetBackgroundColour(style, State.Inactive);
            var fg = Draw.Colour = Utilities.Lerp(Gui.GetForegroundColour(style, State.Inactive), Colors.White, (1 - t) * 0.2f);
            var roundness = Gui.GetRoundness(style, State.Inactive);

            Draw.Colour = bg;
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Inactive);
            Draw.OutlineColour = Gui.GetOutlineColour(style, State.Inactive);
            Draw.Quad(topLeft, size, 0, roundness);

            Draw.Colour = fg;
            float w = Utilities.Clamp(size.X * progress0to1, roundness * 2, size.X);
            Draw.Quad(topLeft, new Vector2(w, size.Y), 0, roundness);

            if (w > roundness * 2)
            {
                fg = Colors.White.WithAlpha(0.5f);
                var min = new Vector2(topLeft.X + roundness, topLeft.Y);
                var max = new Vector2(topLeft.X + w - roundness, topLeft.Y);
                Draw.Colour = fg.WithAlpha((1 - t));
                Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Inactive);
                Draw.OutlineColour = Gui.GetOutlineColour(style, State.Inactive);
                Draw.Quad(Utilities.Lerp(min, max, t), new Vector2(2, size.Y), 0, roundness);
            }

            if (showPercentage)
            {
                var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
                Draw.Font = Gui.GetFont(style);
                Gui.DrawTextInRect(rect, string.Format("{0}%", MathF.Round(progress0to1 * 100)), Gui.GetTextColour(style, State.Inactive), Gui.GetFontSize(style, State.Inactive), HorizontalTextAlign.Center, VerticalTextAlign.Middle);
            }

            Gui.Context.EndControl();
        }
    }
}
