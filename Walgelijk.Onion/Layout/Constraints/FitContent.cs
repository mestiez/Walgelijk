using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct FitContent : IConstraint
{
    public readonly bool Width, Height;

    public FitContent(bool width, bool height)
    {
        Width = width;
        Height = height;
    }

    public void Apply(in ControlParams p)
    {
        var adjustmentWidth = p.Instance.Rects.InnerContentRectAdjustment.Z - p.Instance.Rects.InnerContentRectAdjustment.X;
        var adjustmentHeight = p.Instance.Rects.InnerContentRectAdjustment.W - p.Instance.Rects.InnerContentRectAdjustment.Y;

        if (Width)
            p.Instance.Rects.Intermediate.Width = p.Instance.Rects.ComputedChildContentSize.X - adjustmentWidth;
        if (Height)
            p.Instance.Rects.Intermediate.Height = p.Instance.Rects.ComputedChildContentSize.Y - adjustmentHeight;
    }
}
