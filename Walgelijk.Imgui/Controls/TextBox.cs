using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct TextBox
    {
        public static void Process(string text, Vector2 topLeft, float width, float? height = null, bool drawBackground = false, HorizontalTextAlign halign = HorizontalTextAlign.Left, VerticalTextAlign valign = VerticalTextAlign.Top, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(nameof(TextBox).GetHashCode(), halign.GetHashCode(), valign.GetHashCode(), site, optionalId));
            const float sizeOffset = -5;
            Gui.PrepareDrawer();

            Draw.FontSize = Gui.GetFontSize(style, State.Inactive);
            float padding = Gui.GetPadding(style, State.Inactive);

            Draw.Font = Gui.GetFont(style);
            var h = height ?? (Draw.CalculateTextHeight(text, width + sizeOffset - padding * 2) + padding * 2);
            var size = new Vector2(width, h);

            if (height == null)
                valign = VerticalTextAlign.Top;

            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);

            var rectTopLeft = topLeft;
            var rectSize = size;
            var textTopLeft = topLeft + new Vector2(padding);

            switch (halign)
            {
                case HorizontalTextAlign.Left:
                    break;
                case HorizontalTextAlign.Center:
                    textTopLeft.X += (width - padding + sizeOffset) / 2;
                    break;
                case HorizontalTextAlign.Right:
                    textTopLeft.X += width - padding + sizeOffset;
                    break;
            }

            switch (valign)
            {
                case VerticalTextAlign.Top:
                    break;
                case VerticalTextAlign.Middle:
                    textTopLeft.Y += (h - padding + sizeOffset) / 2;
                    break;
                case VerticalTextAlign.Bottom:
                    textTopLeft.Y += h - padding + sizeOffset;
                    break;
            }

            id.TopLeft = rectTopLeft;
            id.Size = rectSize;
            PositioningUtils.ApplyRaycastRect(id, new Rect(rectTopLeft.X, rectTopLeft.Y, rectTopLeft.X + rectSize.X, rectTopLeft.Y + rectSize.Y), true);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(rectSize, rectTopLeft));
            Gui.PrepareDrawer();

            if (drawBackground)
            {
                Draw.Colour = Gui.GetBackgroundColour(style, State.Active);
                Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Active);
                Draw.OutlineColour = Gui.GetOutlineColour(style, State.Active);
                Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, State.Inactive));
                Draw.OutlineWidth = 0;
            }

            Draw.Colour = Gui.GetTextColour(style, State.Inactive);
            Draw.FontSize = Gui.GetFontSize(style, State.Inactive);
            Draw.Font = Gui.GetFont(style);

            Draw.Text(text, textTopLeft, Vector2.One, halign, valign, width + sizeOffset - padding * 2);

            Gui.Context.EndControl();
        }
    }
}
