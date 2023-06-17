using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct StickLeft : IConstraint
{
    public void Apply(in ControlParams p)
    {
        var offset = 0 - p.Instance.Rects.Intermediate.MinX + p.Theme.Padding;
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(offset, 0);
    }
}
