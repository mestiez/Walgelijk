using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls;

public struct ArrayLayout
{
    //TODO deze code hier is een beetje dubbel he? fix het

    public static void VerticalLayout(Vector2 topLeft, Vector2 size, ArrayLayoutMode verticalLayout = ArrayLayoutMode.Start, ArrayScaleMode horizontalScaling = ArrayScaleMode.None, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
    {
        Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(VerticalLayout).GetHashCode(), site, optionalId));
        PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
        PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
        PositioningUtils.ResetLayoutControl(Gui.Context, id);

        if (id.ChildrenExtendOutOfBounds && verticalLayout != ArrayLayoutMode.Stretch)
            verticalLayout = ArrayLayoutMode.Start;

        Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

        var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
        //if (id.DrawBounds.Enabled)
        //    rect = PositioningUtils.GetRectIntersection(rect, id.DrawBounds.ToRect());

        PositioningUtils.ApplyRaycastRect(id, rect, false);

        var containsMouse = Gui.ContainsMouse(id, true);
        if (!Gui.Context.IsAnythingHot() && containsMouse)
            Gui.Context.Hot = id;
        if (!containsMouse && Gui.Context.IsHot(id))
            Gui.Context.Hot = null;

        float heightToFitTo = MathF.Max(id.ChildSizeSum.Y, size.Y);
        id.MaxLayoutCursorPosition = heightToFitTo;

        switch (verticalLayout)
        {
            case ArrayLayoutMode.Start:
                id.OffsetLayout = BasicLayoutFunctions.Vertical.StartOffset;
                break;
            case ArrayLayoutMode.Stretch:
                float padding = Gui.GetPadding(style, Gui.GetStateFor(id));
                id.MaxLayoutCursorPosition = size.Y - padding * 2;
                id.OffsetLayout = BasicLayoutFunctions.Vertical.StretchOffset;
                id.HeightLayout = BasicLayoutFunctions.Vertical.StretchHeight;
                break;
            case ArrayLayoutMode.Center:
                id.OffsetLayout = BasicLayoutFunctions.Vertical.CenterOffset;
                break;
            case ArrayLayoutMode.SpaceBetween:
                id.OffsetLayout = BasicLayoutFunctions.Vertical.SpaceBetweenOffset;
                break;
            case ArrayLayoutMode.End:
                id.OffsetLayout = BasicLayoutFunctions.Vertical.EndOffset;
                break;
        }

        switch (horizontalScaling)
        {
            case ArrayScaleMode.None:
                break;
            case ArrayScaleMode.Stretch:
                id.WidthLayout = BasicLayoutFunctions.Vertical.StretchScaling;
                break;
            case ArrayScaleMode.Contain:
                id.WidthLayout = BasicLayoutFunctions.Vertical.ContainScaling;
                break;
        }

        //if (Gui.ContainsMouse(id, true))
            PositioningUtils.ProcessScrolling(Gui.Context, id, style, 32);

        Gui.PrepareDrawer();
        Draw.Colour = Gui.GetBackgroundColour(style, State.Active);
        Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Inactive);
        Draw.OutlineColour = Gui.GetOutlineColour(style, State.Inactive);
        Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, State.Inactive));
    }

    public static void StopVerticalLayout()
    {
        var id = Gui.Context.ControlTree.CurrentParent;
        if (id == null)
            return;
        PseudoControls.Scrollbars(optionalId: id.Raw);
        Gui.Context.EndControl();
    }

    public static void HorizontalLayout(Vector2 topLeft, Vector2 size, ArrayLayoutMode horizontalLayout = ArrayLayoutMode.Start, ArrayScaleMode verticalScaling = ArrayScaleMode.None, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
    {
        Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(VerticalLayout).GetHashCode(), site, optionalId));
        PositioningUtils.ApplyCurrentLayout(Gui.Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyCurrentAnchors(Gui.Context, id, ref topLeft, ref size, style);
        PositioningUtils.ApplyAbsoluteTranslation(Gui.Context, id, ref topLeft);
        PositioningUtils.ApplyParentScrollOffset(Gui.Context, id, ref topLeft);
        PositioningUtils.ResetLayoutControl(Gui.Context, id);

        if (id.ChildrenExtendOutOfBounds && horizontalLayout != ArrayLayoutMode.Stretch)
            horizontalLayout = ArrayLayoutMode.Start;

        Draw.DrawBounds = PositioningUtils.ApplyDrawBounds(Gui.Context, id, new DrawBounds(size, topLeft));

        var rect = new Rect(topLeft.X, topLeft.Y, topLeft.X + size.X, topLeft.Y + size.Y);
        //if (id.DrawBounds.Enabled)
        //    rect = PositioningUtils.GetRectIntersection(rect, id.DrawBounds.ToRect());

        PositioningUtils.ApplyRaycastRect(id, rect, false);

        var containsMouse = Gui.ContainsMouse(id, true);
        if (!Gui.Context.IsAnythingHot() && containsMouse)
            Gui.Context.Hot = id;
        if (!containsMouse && Gui.Context.IsHot(id))
            Gui.Context.Hot = null;

        float heightToFitTo = MathF.Max(id.ChildSizeSum.X, size.X);
        id.MaxLayoutCursorPosition = heightToFitTo;

        switch (horizontalLayout)
        {
            case ArrayLayoutMode.Start:
                id.OffsetLayout = BasicLayoutFunctions.Horizontal.StartOffset;
                break;
            case ArrayLayoutMode.Stretch:
                float padding = Gui.GetPadding(style, Gui.GetStateFor(id));
                id.MaxLayoutCursorPosition = size.X - padding * 2;
                id.OffsetLayout = BasicLayoutFunctions.Horizontal.StretchOffset;
                id.WidthLayout = BasicLayoutFunctions.Horizontal.StretchWidth;
                break;
            case ArrayLayoutMode.Center:
                id.OffsetLayout = BasicLayoutFunctions.Horizontal.CenterOffset;
                break;
            case ArrayLayoutMode.SpaceBetween:
                id.OffsetLayout = BasicLayoutFunctions.Horizontal.SpaceBetweenOffset;
                break;
            case ArrayLayoutMode.End:
                id.OffsetLayout = BasicLayoutFunctions.Horizontal.EndOffset;
                break;
        }

        switch (verticalScaling)
        {
            case ArrayScaleMode.None:
                break;
            case ArrayScaleMode.Stretch:
                id.HeightLayout = BasicLayoutFunctions.Horizontal.StretchScaling;
                break;
            case ArrayScaleMode.Contain:
                id.HeightLayout = BasicLayoutFunctions.Horizontal.ContainScaling;
                break;
        }

        //if (Gui.ContainsMouse(id, true))
            PositioningUtils.ProcessScrolling(Gui.Context, id, style, 32, true);

        Gui.PrepareDrawer();
        Draw.Colour = Gui.GetBackgroundColour(style, State.Active);
        Draw.OutlineWidth = Gui.GetOutlineWidth(style, State.Inactive);
        Draw.OutlineColour = Gui.GetOutlineColour(style, State.Inactive);
        Draw.Quad(topLeft, size, roundness: Gui.GetRoundness(style, State.Inactive));
    }

    public static void StopHorizontalLayout()
    {
        var id = Gui.Context.ControlTree.CurrentParent;
        if (id == null)
            return;
        PseudoControls.Scrollbars(optionalId: id.Raw);
        Gui.Context.EndControl();
    }
}
