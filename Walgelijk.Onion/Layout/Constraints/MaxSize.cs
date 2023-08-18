using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct MaxSize : IConstraint
{
    public readonly float? MaxWidth = null;
    public readonly float? MaxHeight = null;

    public MaxSize(float? maxWidth, float? maxHeight)
    {
        MaxWidth = maxWidth * Onion.GlobalScale;
        MaxHeight = maxHeight * Onion.GlobalScale;
    }

    public void Apply(in ControlParams p)
    {
        if (MaxWidth.HasValue && p.Instance.Rects.Intermediate.Width > MaxWidth.Value)
            p.Instance.Rects.Intermediate.MaxX = p.Instance.Rects.Intermediate.MinX + MaxWidth.Value;

        if (MaxHeight.HasValue && p.Instance.Rects.Intermediate.Height > MaxHeight.Value)
            p.Instance.Rects.Intermediate.MaxY = p.Instance.Rects.Intermediate.MinY + MaxHeight.Value;
    }
}
