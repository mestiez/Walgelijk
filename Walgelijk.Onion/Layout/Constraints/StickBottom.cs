using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct StickBottom : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity);
        var offset = parent.Rects.Intermediate.Height - p.Instance.Rects.Intermediate.MaxY - Onion.Theme.Padding;
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(0,offset);
    }
}