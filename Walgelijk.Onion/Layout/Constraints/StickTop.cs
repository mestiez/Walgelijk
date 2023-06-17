using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct StickTop : IConstraint
{
    public void Apply(in ControlParams p)
    {
        var offset = 0 - p.Instance.Rects.Intermediate.MinY + p.Theme.Padding;
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(0, offset);
    }
}
   