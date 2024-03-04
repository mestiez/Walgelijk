using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct IgnoreScroll : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent != null && p.Tree.Instances.TryGetValue(p.Node.Parent.Identity, out var parent))
            p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(-parent.InnerScrollOffset);
    }
}
