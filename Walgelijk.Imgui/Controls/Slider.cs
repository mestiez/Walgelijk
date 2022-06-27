using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct Slider
    {
        public static bool ProcessHorizontal(string format, Vector2 topLeft, Vector2 size, ref float value, float min, float max, int decimals = 2, float step = float.NaN, Func<float, float>? displayTransformer = null, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(nameof(ProcessHorizontal).GetHashCode(), format.GetHashCode(), decimals.GetHashCode(), step.GetHashCode(), site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);

            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));
            bool interacted = false;

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            Gui.ProcessButtonLike(id, rect, out var held, out _, out _);

            if (held)
            {
                float mousePosRatio = Utilities.Clamp(Utilities.MapRange(rect.MinX, rect.MaxX, 0, 1, Gui.Input.WindowMousePos.X));
                var target = Utilities.Lerp(min, max, mousePosRatio);
                if (float.IsNormal(step))
                    target = Utilities.Snap(target, step);
                if (value != target)
                {
                    value = target;
                    interacted = true;
                }

                if (float.IsNormal(step))
                    value = Utilities.Snap(value, step);
            }

            float progressRatio = Utilities.Clamp(Utilities.MapRange(min, max, 0, 1, value));

            State state = Gui.GetStateFor(id);
            float roundness = Gui.GetRoundness(style, state);

            Gui.PrepareDrawer();
            Draw.Colour = Gui.GetBackgroundColour(style, state);
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, state);
            Draw.OutlineColour = Gui.GetOutlineColour(style, state);

            Draw.Quad(topLeft, size, roundness: roundness);

            Draw.Colour = Gui.GetForegroundColour(style, state);
            Draw.Quad(topLeft, new Vector2(MathF.Max(roundness * 2, size.X * progressRatio), size.Y), roundness: roundness);

            float toShow = displayTransformer != null ? displayTransformer(value) : value;

            Gui.DrawTextInRect(rect, string.Format(format, MathF.Round(toShow, decimals)), Gui.GetTextColour(style, state), Gui.GetFontSize(style, state), HorizontalTextAlign.Center, VerticalTextAlign.Middle);

            Gui.Context.EndControl();

            return interacted;
        }

        public static bool ProcessVertical(string format, Vector2 topLeft, Vector2 size, ref float value, float min, float max, int decimals = 2, float step = float.NaN, Func<float, float>? displayTransformer = null, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(nameof(ProcessVertical).GetHashCode(), format.GetHashCode(), decimals.GetHashCode(), step.GetHashCode(), site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);

            bool interacted = false;

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            Gui.ProcessButtonLike(id, rect, out var held, out _, out _);

            if (held)
            {
                float mousePosRatio = Utilities.Clamp(Utilities.MapRange(rect.MaxY, rect.MinY, 0, 1, Gui.Input.WindowMousePos.Y));
                var target = Utilities.Lerp(min, max, mousePosRatio);
                if (float.IsNormal(step))
                    target = Utilities.Snap(target, step);
                if (value != target)
                {
                    value = target;
                    interacted = true;
                }

                if (float.IsNormal(step))
                    value = Utilities.Snap(value, step);
            }

            float progressRatio = Utilities.Clamp(Utilities.MapRange(min, max, 0, 1, value));

            State state = Gui.GetStateFor(id);
            float roundness = Gui.GetRoundness(style, state);

            Gui.PrepareDrawer();
            Draw.Colour = Gui.GetBackgroundColour(style, state);
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, state);
            Draw.OutlineColour = Gui.GetOutlineColour(style, state);
            Draw.Quad(topLeft, size, roundness: roundness);

            Draw.Colour = Gui.GetForegroundColour(style, state);
            var tSize = MathF.Max(size.Y * progressRatio, roundness * 2);
            Draw.Quad(new Vector2(topLeft.X, topLeft.Y + (size.Y - tSize)),
                new Vector2(size.X, tSize),
                roundness: roundness);

            float toShow = displayTransformer != null ? displayTransformer(value) : value;

            Gui.Label(string.Format(format, MathF.Round(toShow, decimals)), topLeft + size / 2, HorizontalTextAlign.Center, VerticalTextAlign.Middle, size.X, null, style);

            Gui.Context.EndControl();
            return interacted;
        }

        public static bool IntegerSlider(string format, Vector2 topLeft, Vector2 size, ref int value, int min, int max, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            float f = value;
            if (Gui.SliderHorizontal(format, topLeft, size, ref f, min, max, 0, 1, style, site, optionalId))
            {
                value = Utilities.Clamp((int)MathF.Round(f), min, max);
                return true;
            }
            return false;
        }
    }
}
