using System;
using System.Numerics;

namespace Walgelijk.Imgui
{
    public struct PositioningUtils
    {
        public static float GetCombinedWidthOfChildren(Context guiContext, Identity parent)
        {
            float width = 0;
            foreach (var item in guiContext.ControlTree.GetDirectChildrenOf(parent))
                if (!item.AbsoluteLayout)
                    width += item.Size.X;
            return width;
        }

        public static float GetCombinedHeightOfChildren(Context guiContext, Identity parent)
        {
            float height = 0;
            foreach (var item in guiContext.ControlTree.GetDirectChildrenOf(parent))
                if (!item.AbsoluteLayout)
                    height += item.Size.Y;
            return height;
        }

        public static Rect GetRectIntersection(Rect a, Rect b)
        {
            float minx = MathF.Max(a.MinX, b.MinX);
            float maxx = MathF.Min(a.MaxX, b.MaxX);

            float miny = MathF.Max(a.MinY, b.MinY);
            float maxy = MathF.Min(a.MaxY, b.MaxY);

            if (minx > maxx)
                maxx = minx;

            if (miny > maxy)
                maxy = miny;

            var r = new Rect(minx, miny, maxx, maxy);

            return r;
        }

        public static DrawBounds ApplyDrawBounds(Context guiContext, Identity id, DrawBounds bounds)
        {
            if (bounds.Size.X < 0 || bounds.Size.Y < 0)
            {
                bounds.Size = Vector2.Zero;
                bounds.Enabled = false;
            }

            if (float.IsInfinity(bounds.Size.X) || float.IsInfinity(bounds.Size.Y) || float.IsNaN(bounds.Size.X) || float.IsNaN(bounds.Size.Y))
                bounds.Enabled = false;

            id.DrawBounds = bounds;
            if (id.AbsoluteLayout || !id.Parent.HasValue || !guiContext.Identities.TryGetValue(id.Parent.Value, out var parent))
                return bounds;

            var parentBounds = parent.DrawBounds;
            if (!bounds.Enabled) //als mijn drawbounds disabled moeten zijn dan moet ik gewoon de parents drawbounds returnen
            {
                id.DrawBounds = parentBounds;
                return parentBounds;
            }

            var intersection = GetRectIntersection(
                bounds.ToRect(),
                parentBounds.ToRect());

            var newBounds = new DrawBounds(intersection.GetSize(), intersection.BottomLeft, true);

            if (float.IsInfinity(newBounds.Size.X) || float.IsInfinity(newBounds.Size.Y) || float.IsNaN(newBounds.Size.X) || float.IsNaN(newBounds.Size.Y))
                newBounds.Enabled = false;

            id.DrawBounds = newBounds;
            return newBounds;
        }

        public static void GetVerticalScrollBounds(Identity id, float padding, out float lowerBound, out float upperBound)
        {
            var max = id.InnerContentBounds.MinY - id.TopLeft.Y + padding;
            var min = (id.TopLeft.Y + id.Size.Y) - id.InnerContentBounds.MaxY - padding;

            upperBound = MathF.Max(max, min);
            lowerBound = MathF.Min(max, min);

            //id.CalculatedScrollBounds.lowerY = lowerBound;
            //id.CalculatedScrollBounds.upperY = upperBound;
        }

        public static void GetHorizontalScrollBounds(Identity id, float padding, out float lowerBound, out float upperBound)
        {
            var max = id.InnerContentBounds.MinX - id.TopLeft.X + padding;
            var min = (id.TopLeft.X + id.Size.X) - id.InnerContentBounds.MaxX - padding;

            upperBound = MathF.Max(max, min);
            lowerBound = MathF.Min(max, min);

            //id.CalculatedScrollBounds.lowerX = lowerBound;
            //id.CalculatedScrollBounds.upperX = upperBound;
        }

        /// <summary>
        /// Scrolls the inner offset of the given control. Returns the actual amount scrolled.
        /// </summary>
        public static float ScrollVertical(Context guiContext, Identity id, float delta, float padding)
        {
            var old = id.InnerScrollOffset;
            GetVerticalScrollBounds(id, padding, out var lowerBound, out var upperBound);
            id.InnerScrollOffset.Y = Utilities.Clamp(id.InnerScrollOffset.Y + delta, lowerBound, upperBound);
            return id.InnerScrollOffset.Y - old.Y;
        }

        /// <summary>
        /// Scrolls the inner offset of the given control. Returns the actual amount scrolled.
        /// </summary>
        public static float ScrollHorizontal(Context guiContext, Identity id, float delta, float padding)
        {
            var old = id.InnerScrollOffset;
            GetHorizontalScrollBounds(id, padding, out var lowerBound, out var upperBound);
            id.InnerScrollOffset.X = Utilities.Clamp(id.InnerScrollOffset.X + delta, lowerBound, upperBound);
            return id.InnerScrollOffset.X - old.X;
        }

        public static void ProcessScrolling(Context guiContext, Identity id, Style? style, float scrollIntensity = 1, bool flipHorizontal = false)
        {
            if (!id.ChildrenExtendOutOfBounds)
                return;

            Gui.Context.RequestScrollInput(id);

            if (!id.LocalInputState.HasScrollFocus)
                return;

            var padding = Gui.GetPadding(style, Gui.GetStateFor(id));
            var d = id.LocalInputState.ScrollDelta * scrollIntensity;
            if (MathF.Abs(d) > float.Epsilon)
            {
                //PositioningUtils.ForceCalculateInnerBounds(Gui.Context, id, out _, out _);

                bool hor = id.LocalInputState.IsKeyHeld(Key.LeftShift);
                if (flipHorizontal)
                    hor = !hor;

                if (hor)
                    ScrollHorizontal(guiContext, id, d, padding);
                else
                    ScrollVertical(guiContext, id, d, padding);

                //if (MathF.Abs(actualScrolled) > float.Epsilon)
                //    Gui.Context.RequestScrollInput(id);
            }
        }

        public static void ForceCalculateInnerBounds(Context guiContext, Identity id, out float widthSum, out float heightSum)
        {
            var innerBounds = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
            widthSum = 0;
            heightSum = 0;
            foreach (var item in guiContext.ControlTree.EnumerateChildren(id))
            {
                if (item.AbsoluteLayout)
                    continue;

                widthSum += item.Size.X;
                heightSum += item.Size.Y;

                var left = item.TopLeft.X - id.InnerScrollOffset.X;
                var right = item.TopLeft.X + item.Size.X - id.InnerScrollOffset.X;
                var top = item.TopLeft.Y - id.InnerScrollOffset.Y;
                var bottom = item.TopLeft.Y + item.Size.Y - id.InnerScrollOffset.Y;

                innerBounds.MinX = MathF.Min(left, innerBounds.MinX);
                innerBounds.MinY = MathF.Min(top, innerBounds.MinY);

                innerBounds.MaxX = MathF.Max(right, innerBounds.MaxX);
                innerBounds.MaxY = MathF.Max(bottom, innerBounds.MaxY);
            }

            id.InnerContentBounds = innerBounds;
        }

        public static void ResetLayoutControl(Context guiContext, Identity id)
        {
            ForceCalculateInnerBounds(guiContext, id, out var width, out var height);
            id.LayoutCursorPosition = 0;
            id.ChildSizeSum = new Vector2(width, height);

            if (!id.ChildrenExtendOutOfBounds)
                id.InnerScrollOffset = default;

            id.OffsetLayout = null;
            id.WidthLayout = null;
            id.HeightLayout = null;
        }

        public static void ApplyCurrentLayout(Context guiContext, Identity id, ref Vector2 topLeft, ref Vector2 size, Style? style)
        {
            if (guiContext.ControlTree.PreviousParent == null || id.AbsoluteLayout)
            {
                id.Size = size = ImguiUtils.NormaliseSize(size);
                id.TopLeft = topLeft = ImguiUtils.NormalisePosition(topLeft);
                return;
            }

            topLeft = Gui.Context.CurrentOffsetLayout(guiContext.ControlTree.PreviousParent, id, topLeft, style);

            if (!id.AbsoluteWidth)
                size.X = Gui.Context.CurrentWidthLayout(guiContext.ControlTree.PreviousParent, id, size.X, style);

            if (!id.AbsoluteHeight)
                size.Y = Gui.Context.CurrentHeightLayout(guiContext.ControlTree.PreviousParent, id, size.Y, style);

            id.Size = size = ImguiUtils.NormaliseSize(size);
            id.TopLeft = topLeft = ImguiUtils.NormalisePosition(topLeft);
        }

        public static void ApplyCurrentAnchors(Context guiContext, Identity id, ref Vector2 topLeft, ref Vector2 size, Style? style)
        {
            if (guiContext.ControlTree.PreviousParent == null || id.AbsoluteLayout)
            {
                id.Size = size = ImguiUtils.NormaliseSize(size);
                id.TopLeft = topLeft = ImguiUtils.NormalisePosition(topLeft);
                return;
            }

            if (Gui.Context.CurrentHorizontalAnchor != null)
                topLeft.X = Gui.Context.CurrentHorizontalAnchor(guiContext.ControlTree.PreviousParent, id, topLeft.X, style);

            if (Gui.Context.CurrentVerticalAnchor != null)
                topLeft.Y = Gui.Context.CurrentVerticalAnchor(guiContext.ControlTree.PreviousParent, id, topLeft.Y, style);

            if (!id.AbsoluteWidth && Gui.Context.CurrentWidthAnchor != null)
                size.X = Gui.Context.CurrentWidthAnchor(guiContext.ControlTree.PreviousParent, id, size.X, style);

            if (!id.AbsoluteHeight && Gui.Context.CurrentHeightAnchor != null)
                size.Y = Gui.Context.CurrentHeightAnchor(guiContext.ControlTree.PreviousParent, id, size.Y, style);

            id.Size = size = ImguiUtils.NormaliseSize(size);
            id.TopLeft = topLeft = ImguiUtils.NormalisePosition(topLeft);
        }

        public static void ApplyParentScrollOffset(Context guiContext, Identity id, ref Vector2 topLeft)
        {
            if (id.AbsoluteLayout || !id.Parent.HasValue || !guiContext.Identities.TryGetValue(id.Parent.Value, out var parentId))
            {
                id.TopLeft = topLeft = ImguiUtils.NormalisePosition(topLeft);
                return;
            }

            topLeft += ImguiUtils.NormalisePosition(parentId.InnerScrollOffset);
            id.TopLeft = topLeft = ImguiUtils.NormalisePosition(topLeft);
        }

        public static void ApplyAbsoluteTranslation(Context guiContext, Identity id, ref Vector2 topLeft)
        {
            topLeft += id.AbsoluteTranslation;
            id.TopLeft = topLeft = ImguiUtils.NormalisePosition(topLeft);
        }

        public static void ApplyRaycastRect(Identity id, Rect interactionRect, bool considerDrawBounds = true)
        {
            if (id.DrawBounds.Enabled && considerDrawBounds)
            {
                interactionRect = GetRectIntersection(interactionRect, id.DrawBounds.ToRect());
                id.RaycastRectangle = interactionRect;
            }
            else
                id.RaycastRectangle = interactionRect;
        }

        public static void DisableRaycastRect(Identity id)
        {
            id.RaycastRectangle = null;
        }
    }
}
