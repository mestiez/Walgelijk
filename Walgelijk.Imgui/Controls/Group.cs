using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{

    public struct Group
    {
        public static void StartGroup(Vector2 topLeft, Vector2 size, bool drawBackground = false, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            var id = Gui.Context.StartControl(IdGen.Hash(site, optionalId));
            PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
            PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
            PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
            Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

            var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
            if (id.DrawBounds.Enabled)
                rect = PositioningUtils.GetRectIntersection(rect, id.DrawBounds.ToRect());

            PositioningUtils.ApplyRaycastRect(id, rect, false);

            var containsMouse = Gui.ContainsMouse(id, false);
            if (!Gui.Context.IsAnythingHot() && containsMouse)
                Gui.Context.Hot = id;
            if (!containsMouse && Gui.Context.IsHot(id))
                Gui.Context.Hot = null;

            if (drawBackground)
            {
                Gui.PrepareDrawer();
                Draw.Colour = Gui.GetBackgroundColour(style, State.Active);
                Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Active);
                Draw.OutlineColour = Gui.GetOutlineColour(style, State.Active);
                Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, State.Inactive));
            }
        }

        public static void StopGroup()
        {
            var id = Gui.Context.ControlTree.CurrentParent;
            if (id == null)
                return;
            Gui.Context.EndControl();
        }
    }
}
