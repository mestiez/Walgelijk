namespace Walgelijk.Imgui
{
    public struct BasicAnchorFunctions
    {
        public static float Left(Identity parent, Identity current, float currentX, Style? style = null)
        {
            return parent.TopLeft.X + Gui.GetPadding(style, Gui.GetStateFor(current));
        }

        public static float Right(Identity parent, Identity current, float currentX, Style? style = null)
        {
            return parent.TopLeft.X + parent.Size.X - current.Size.X - Gui.GetPadding(style, Gui.GetStateFor(current));
        }

        public static float Top(Identity parent, Identity current, float currentY, Style? style = null)
        {
            return parent.TopLeft.Y + Gui.GetPadding(style, Gui.GetStateFor(current));
        }

        public static float Bottom(Identity parent, Identity current, float currentY, Style? style = null)
        {
            return parent.TopLeft.Y + parent.Size.Y - current.Size.Y - Gui.GetPadding(style, Gui.GetStateFor(current));
        }

        public static float HorizontalCenter(Identity parent, Identity current, float currentX, Style? style = null)
        {
            return parent.TopLeft.X + parent.Size.X / 2 - current.Size.X / 2;
        }

        public static float VerticalCenter(Identity parent, Identity current, float currentX, Style? style = null)
        {
            return parent.TopLeft.Y + parent.Size.Y / 2 - current.Size.Y / 2;
        }

        public static float StretchWidth(Identity parent, Identity current, float currentWidth, Style? style = null)
        {
            return parent.Size.X - Gui.GetPadding(style, Gui.GetStateFor(current)) * 2;
        }

        public static float StretchHeight(Identity parent, Identity current, float currentHeight, Style? style = null)
        {
            return parent.Size.Y - Gui.GetPadding(style, Gui.GetStateFor(current)) * 2;
        }

        public static float ContainWidth(Identity parent, Identity current, float currentWidth, Style? style = null)
        {
            var min = parent.Size.X - Gui.GetPadding(style, Gui.GetStateFor(current)) * 2;
            if (currentWidth > min)
                return min;
            return currentWidth;
        }

        public static float ContainHeight(Identity parent, Identity current, float currentHeight, Style? style = null)
        {
            var min = parent.Size.Y - Gui.GetPadding(style, Gui.GetStateFor(current)) * 2;
            if (currentHeight > min)
                return min;
            return currentHeight;
        }
    }
}
