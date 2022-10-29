using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct Button
    {
        public static bool Process(string text, Vector2 topLeft, Vector2 size, out bool wasPressed, out bool wasReleased, HorizontalTextAlign halign = HorizontalTextAlign.Center, VerticalTextAlign valign = VerticalTextAlign.Middle, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(Button).GetHashCode(), text.GetHashCode(), site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            Gui.ProcessButtonLike(id, rect, out var result, out wasPressed, out wasReleased, true);

            State state = Gui.GetStateFor(id);
            Gui.PrepareDrawer();
            Draw.Colour = Gui.GetBackgroundColour(style, state);
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, state);
            Draw.OutlineColour = Gui.GetOutlineColour(style, state);
            Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, state));
            Draw.Font = Gui.GetFont(style);
            //Gui.TextBox(text, rect.BottomLeft, rect.Width, rect.Height, false, HorizontalTextAlign.Center, VerticalTextAlign.Middle, style, optionalId: id.Raw);
            Gui.DrawTextInRect(rect, text, Gui.GetTextColour(style, state), Gui.GetFontSize(style, state), halign, valign);

            Gui.Context.EndControl();
            return result;
        }
    }
}
