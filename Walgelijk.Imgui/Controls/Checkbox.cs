using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct Checkbox
    {
        public static bool Process(string label, Vector2 topLeft, Vector2 size, ref bool value, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(nameof(Checkbox).GetHashCode(), label.GetHashCode(), site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            Gui.ProcessButtonLike(id, rect, out var held, out var pressed, out var released);

            if (pressed)
                value = !value;

            State state = Gui.GetStateFor(id);
            Gui.PrepareDrawer();
            Draw.Colour = Gui.GetBackgroundColour(style, state);
            Draw.OutlineWidth = Gui.GetOutlineWidth(style, state);
            Draw.OutlineColour = Gui.GetOutlineColour(style, state);
            Draw.Quad(topLeft, new Vector2(size.Y), roundness: Gui.GetRoundness(style, state));

            const float sizeDifference = 5;
            if (value)
            {
                Draw.Colour = Gui.GetForegroundColour(style, state);
                Draw.Quad(topLeft + new Vector2(sizeDifference), new Vector2(size.Y - sizeDifference * 2), roundness: Gui.GetRoundness(style, state));
            }

            Draw.Font = Gui.GetFont(style);
            Draw.FontSize = Gui.GetFontSize(style, state);
            Draw.Colour = Gui.GetTextColour(style, state);

            Draw.Text(label, topLeft + new Vector2(size.Y + 5, size.Y / 2), Vector2.One, HorizontalTextAlign.Left, VerticalTextAlign.Middle, size.X);

            Gui.Context.EndControl();
            return pressed;
        }
    }
}
