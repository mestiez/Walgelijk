using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct MinSize : IConstraint
{
    public readonly float? MinWidth = null;
    public readonly float? MinHeight = null;

    public MinSize(float? minWidth, float? minHeight)
    {
        MinWidth = minWidth * Onion.GlobalScale;
        MinHeight = minHeight * Onion.GlobalScale;
    }

    public void Apply(in ControlParams p)
    {
        if (MinWidth.HasValue && p.Instance.Rects.Intermediate.Width < MinWidth.Value)
            p.Instance.Rects.Intermediate.MaxX = p.Instance.Rects.Intermediate.MinX + MinWidth.Value;

        if (MinHeight.HasValue && p.Instance.Rects.Intermediate.Height < MinHeight.Value)
            p.Instance.Rects.Intermediate.MaxY = p.Instance.Rects.Intermediate.MinY + MinHeight.Value;
    }
}
