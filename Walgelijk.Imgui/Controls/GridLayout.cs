using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls;

public struct GridLayout
{
    public static void StartGridLayout(Vector2 topLeft, Vector2 size, int columns, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
    {
        Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(StartGridLayout).GetHashCode(), site, optionalId));
        PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
        PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
        PositioningUtils.ResetLayoutControl(Gui.Context, id);

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

        int rows = (int)MathF.Ceiling(id.ChildCount / (float)columns);
        float padding = Gui.GetPadding(style, State.Inactive);
        float widthPadSum = padding * (columns + 1);
        float heightPadSum = padding * rows;
        float gridCellSize = (size.X - widthPadSum) / columns;
        id.LayoutCursorPosition = 0;
        id.MaxLayoutCursorPosition = rows * gridCellSize + heightPadSum;

        id.WidthLayout = (parent, control, currentWidth, style) => gridCellSize;
        id.HeightLayout = (parent, control, currentHeight, style) => gridCellSize;
        id.OffsetLayout = (parent, control, currentPosition, style) =>
        {
            float x = parent.LayoutCursorPosition % (parent.Size.X - padding) + padding;
            float y = MathF.Floor(parent.LayoutCursorPosition / (parent.Size.X - padding)) * (gridCellSize + padding);
            var v = parent.TopLeft + new Vector2(x, y);
            v.X = (int)v.X;
            v.Y = (int)v.Y;
            parent.LayoutCursorPosition += gridCellSize + padding;
            return v;
        };

        //if (Gui.ContainsMouse(id, true))
        PositioningUtils.ProcessScrolling(Gui.Context, id, style, 32);

        // Gui.PrepareDrawer();
        //  Draw.Colour = Gui.GetBackgroundColour(style, State.Active);
        //  Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Inactive);
        //  Draw.OutlineColour = Gui.GetOutlineColour(style, State.Inactive);
        // Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, State.Inactive));
    }

    public static void StopGridLayout()
    {
        var id = Gui.Context.ControlTree.CurrentParent;
        if (id == null)
            return;
        PseudoControls.Scrollbars(optionalId: id.Raw);
        Gui.Context.EndControl();
    }
}
