using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct OffsetSize : IConstraint
{
    public readonly float Width;
    public readonly float Height;

    public OffsetSize(float w, float h)
    {
        Width = w * Onion.GlobalScale;
        Height = h * Onion.GlobalScale;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.Width += Width;
        p.Instance.Rects.Intermediate.Height += Height;
    }
}
