using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct WidthConstraint : IConstraint
{
    public readonly float Width;

    public WidthConstraint(float w)
    {
        Width = w * Onion.GlobalScale;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.MaxX = p.Instance.Rects.Intermediate.MinX + Width;
    }
}
