using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct Label
    {
        public static void Process(string text, Vector2 topLeft, HorizontalTextAlign halign = HorizontalTextAlign.Left, VerticalTextAlign valign = VerticalTextAlign.Top, float wrapWidth = float.PositiveInfinity, float? height = null, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(halign.GetHashCode(), valign.GetHashCode(), wrapWidth.GetHashCode(), site, optionalId));
            var size = new Vector2(wrapWidth, height ?? Draw.Font.LineHeight);
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);

            //TODO dit is niet correct denk ik
            var rectTopLeft = topLeft;
            var rectSize = size;
            //TODO je moet een functie hebben om heel goedkoop & snel de text bounds uit te rekenen
            switch (halign)
            {
                case HorizontalTextAlign.Left:
                    rectTopLeft.X = topLeft.X;
                    break;
                case HorizontalTextAlign.Center:
                    rectTopLeft.X = topLeft.X - size.X / 2;
                    break;
                case HorizontalTextAlign.Right:
                    rectTopLeft.X = topLeft.X + size.X;
                    break;
            }
            switch (valign)
            {
                case VerticalTextAlign.Top:
                    rectTopLeft.Y = topLeft.Y;
                    break;
                case VerticalTextAlign.Middle:
                    rectTopLeft.Y = topLeft.Y - size.Y / 2;
                    break;
                case VerticalTextAlign.Bottom:
                    rectTopLeft.Y = topLeft.Y + size.Y;
                    break;
            }

            id.TopLeft = rectTopLeft;
            id.Size = rectSize;

            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, DrawBounds.DisabledBounds);

            PositioningUtils.ApplyRaycastRect(id, new Rect(rectTopLeft.X, rectTopLeft.Y, rectTopLeft.X + rectSize.X, rectTopLeft.Y + rectSize.Y), true);

            Gui.PrepareDrawer();
            Draw.Colour = Gui.GetTextColour(style, State.Inactive);
            Draw.FontSize = Gui.GetFontSize(style, State.Inactive);
            Draw.Font = Gui.GetFont(style);
            Draw.Text(text, topLeft, Vector2.One, halign, valign, wrapWidth);

            Gui.Context.EndControl();
        }
    }
}
