using System;
using System.Numerics;

namespace Walgelijk.Imgui
{
    public struct BasicLayoutFunctions
    {
        public struct Vertical
        {
            public static Vector2 StartOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                var v = parent.TopLeft + new Vector2(padding, parent.LayoutCursorPosition + padding);
                parent.LayoutCursorPosition += identity.Size.Y + padding;
                return v;
            }

            public static Vector2 StretchOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                var v = parent.TopLeft + new Vector2(padding, parent.LayoutCursorPosition + padding);
                parent.LayoutCursorPosition += identity.Size.Y + padding;
                return v;
            }

            public static float StretchHeight(Identity parent, Identity identity, float currentHeight, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float targetHeight = (parent.MaxLayoutCursorPosition - padding * Math.Max(1, parent.PreviousChildCount - 1)) / parent.PreviousChildCount;
                return targetHeight;
            }

            public static Vector2 SpaceBetweenOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float step = (parent.MaxLayoutCursorPosition - padding * 2 - parent.ChildSizeSum.Y) / Math.Max(1, parent.PreviousChildCount - 1);
                var v = parent.TopLeft + new Vector2(padding, parent.LayoutCursorPosition + padding);
                parent.LayoutCursorPosition += identity.Size.Y;
                parent.LayoutCursorPosition += step;
                return v;
            }

            public static Vector2 CenterOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                var v = parent.TopLeft + new Vector2(padding, parent.LayoutCursorPosition + parent.Size.Y / 2 - (parent.ChildSizeSum.Y + padding * parent.PreviousChildCount) / 2 + padding * 2);
                parent.LayoutCursorPosition += identity.Size.Y + padding;
                return v;
            }

            public static Vector2 EndOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float offset = parent.MaxLayoutCursorPosition - parent.ChildSizeSum.Y - padding * Math.Max(1, parent.PreviousChildCount - 1);
                var v = parent.TopLeft + new Vector2(padding, parent.LayoutCursorPosition + offset - padding);
                parent.LayoutCursorPosition += identity.Size.Y + padding;
                return v;
            }

            public static float ContainScaling(Identity parent, Identity current, float existingWidth, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float targetWidth = parent.Size.X - padding * 2;
                return MathF.Min(targetWidth, existingWidth);
            }

            public static float StretchScaling(Identity parent, Identity current, float existingWidth, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float targetWidth = parent.Size.X - padding * 2;
                return targetWidth;
            }
        }

        public struct Horizontal
        {
            public static Vector2 StartOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                var v = parent.TopLeft + new Vector2(parent.LayoutCursorPosition + padding, padding);
                parent.LayoutCursorPosition += identity.Size.X + padding;
                return v;
            }

            public static Vector2 StretchOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                var v = parent.TopLeft + new Vector2(parent.LayoutCursorPosition + padding, padding);
                parent.LayoutCursorPosition += identity.Size.X + padding;
                return v;
            }

            public static float StretchWidth(Identity parent, Identity identity, float currentHeight, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float targetWidth = (parent.MaxLayoutCursorPosition - padding * Math.Max(1, parent.PreviousChildCount - 1)) / parent.PreviousChildCount;
                return targetWidth;
            }

            public static Vector2 SpaceBetweenOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                //return StretchOffset(parent, identity, currentPosition, style);
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float step = (parent.MaxLayoutCursorPosition - padding * 2 - parent.ChildSizeSum.X) / Math.Max(1, parent.PreviousChildCount - 1);
                var v = parent.TopLeft + new Vector2(parent.LayoutCursorPosition + padding, padding);
                parent.LayoutCursorPosition += identity.Size.X;
                parent.LayoutCursorPosition += step;
                return v;
            }

            public static Vector2 CenterOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                var v = parent.TopLeft + new Vector2(
                    parent.LayoutCursorPosition + parent.Size.X / 2 - (parent.ChildSizeSum.X + padding * parent.PreviousChildCount) / 2 + padding * 2f, 
                    padding
                    );
                parent.LayoutCursorPosition += identity.Size.X + padding;
                return v;
            }

            public static Vector2 EndOffset(Identity parent, Identity identity, Vector2 currentPosition, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float offset = parent.MaxLayoutCursorPosition - parent.ChildSizeSum.X - padding * Math.Max(1, parent.PreviousChildCount - 1);
                var v = parent.TopLeft + new Vector2(parent.LayoutCursorPosition + offset, padding);
                parent.LayoutCursorPosition += identity.Size.X + padding;
                return v;
            }

            public static float ContainScaling(Identity parent, Identity current, float existingHeight, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                float targetHeight = parent.Size.Y - padding * 2;
                return MathF.Min(targetHeight, existingHeight);
            }

            public static float StretchScaling(Identity parent, Identity current, float existingHeight, Style? style = null)
            {
                float padding = Gui.GetPadding(style, Gui.GetStateFor(parent));
                return parent.Size.Y - padding * 2;
            }
        }
    }
}
