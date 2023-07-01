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
        if (Width)
            p.Instance.Rects.Intermediate.Width = p.Instance.Rects.ComputedChildContent.Width;
        if (Height)
            p.Instance.Rects.Intermediate.Height = p.Instance.Rects.ComputedChildContent.Height;
    }
}
