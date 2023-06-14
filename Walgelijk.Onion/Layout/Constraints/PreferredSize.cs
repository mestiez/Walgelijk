using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct PreferredSize : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (p.Instance.PreferredWidth.HasValue)
            p.Instance.Rects.Intermediate.Width = p.Instance.PreferredWidth.Value;

        if (p.Instance.PreferredHeight.HasValue)
            p.Instance.Rects.Intermediate.Height = p.Instance.PreferredHeight.Value;
    }
}
