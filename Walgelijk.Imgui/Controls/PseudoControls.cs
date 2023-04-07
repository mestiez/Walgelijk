using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.SimpleDrawing;

namespace Walgelijk.Imgui.Controls
{
    public struct PseudoControls
    {
        private enum ScrollAxis
        {
            Vertical,
            Horizontal
        }

        public static void Scrollbars(bool horizontal = true, bool vertical = true, Style? style = null, [CallerLineNumber] int site = 0, int optionalId = 0)
        {
            if (!(Gui.Context.ControlTree.CurrentParent?.ChildrenExtendOutOfBounds ?? false))
                return;

            if (vertical)
                processScrollbar(site, optionalId, ScrollAxis.Vertical, style);

            if (horizontal)
                processScrollbar(site, optionalId + 1, ScrollAxis.Horizontal, style);

            static void processScrollbar(int site, int optionalId, ScrollAxis axis, Style? style)
            {
                Gui.EscapeLayout();
                Identity id = Gui.Context.StartControl(IdGen.Hash(nameof(Scrollbars).GetHashCode(), site, optionalId));
                var parentId = id.Parent;
                if (parentId.HasValue &&
                    Gui.Context.Identities.TryGetValue(parentId.Value, out var parent) &&
                    (axis == ScrollAxis.Vertical ?
                        parent.InnerContentBounds.Height > parent.Size.Y :
                        parent.InnerContentBounds.Width > parent.Size.X))
                {
                    //PositioningUtils.ForceCalculateInnerBounds(Gui.Context, parent, out _, out _);

                    const float scrollbarThickness = 3;
                    var padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                    float scrollbarTotalSize = axis == ScrollAxis.Vertical ?
                        parent.Size.Y * (parent.Size.Y / parent.InnerContentBounds.Height) :
                        parent.Size.X * (parent.Size.X / parent.InnerContentBounds.Width);

                    float barPosition, lowerBound, upperBound;
                    Rect rect;
                    if (axis == ScrollAxis.Vertical)
                    {
                        PositioningUtils.GetVerticalScrollBounds(parent, padding, out lowerBound, out upperBound);
                        barPosition = Utilities.Lerp(
                            parent.TopLeft.Y, parent.TopLeft.Y + parent.Size.Y - scrollbarTotalSize,
                            Utilities.MapRange(lowerBound, upperBound, 1, 0, parent.InnerScrollOffset.Y));

                        rect = new Rect(parent.TopLeft.X + parent.Size.X - scrollbarThickness, parent.TopLeft.Y,
                                        parent.TopLeft.X + parent.Size.X, parent.TopLeft.Y + parent.Size.Y);
                    }
                    else
                    {
                        PositioningUtils.GetHorizontalScrollBounds(parent, padding, out lowerBound, out upperBound);
                        barPosition = Utilities.Lerp(parent.TopLeft.X, parent.TopLeft.X + parent.Size.X - scrollbarTotalSize,
                            Utilities.MapRange(lowerBound, upperBound, 1, 0, parent.InnerScrollOffset.X));

                        rect = new Rect(parent.TopLeft.X, parent.TopLeft.Y + parent.Size.Y - scrollbarThickness,
                                        parent.TopLeft.X + parent.Size.X, parent.TopLeft.Y + parent.Size.Y);
                    }

                    if (Gui.Context.IsActive(id))
                    {
                        if (Gui.Input.IsButtonReleased(Walgelijk.MouseButton.Left))
                            Gui.Context.Active = null;
                        else
                        {
                            if (axis == ScrollAxis.Vertical)
                            {
                                var delta = -Gui.Input.WindowMousePosDelta.Y * rect.Height / scrollbarTotalSize;
                                PositioningUtils.ScrollVertical(Gui.Context, parent, delta, padding);
                            }
                            else
                            {
                                var delta = -Gui.Input.WindowMousePosDelta.X * rect.Width / scrollbarTotalSize;
                                PositioningUtils.ScrollHorizontal(Gui.Context, parent, delta, padding);
                            }
                        }
                    }
                    else
                    {
                        //TODO eigenlijk moet je alle soorten rectangles en orders kunnen registreren bij de raycast dinges
                        if (rect.ContainsPoint(Gui.Input.WindowMousePos, 4))
                            Gui.Context.Hot = id;
                        else if (Gui.Context.IsHot(id))
                            Gui.Context.Hot = null;

                        if (Gui.Context.IsHot(id) && Gui.Input.IsButtonPressed(Walgelijk.MouseButton.Left))
                            Gui.Context.Active = id;
                    }

                    Gui.PrepareDrawer();
                    Draw.DrawBounds = parent.DrawBounds;
                    Draw.Colour = Gui.GetBackgroundColour(null, State.Active) * 0.5f;
                    Draw.Quad(rect.BottomLeft, rect.GetSize(), 0, 0);

                    Draw.Colour = Gui.GetForegroundColour(null, State.Hover);
                    if (axis == ScrollAxis.Vertical)
                        Draw.Quad(new Vector2(parent.TopLeft.X + parent.Size.X - scrollbarThickness, barPosition),
                            new Vector2(scrollbarThickness, scrollbarTotalSize), 0, 0);
                    else
                        Draw.Quad(new Vector2(barPosition, parent.TopLeft.Y + parent.Size.Y - scrollbarThickness),
                            new Vector2(scrollbarTotalSize, scrollbarThickness), 0, 0);
                }
                Gui.Context.EndControl();
            }
        }
    }
}
