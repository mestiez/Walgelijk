using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct StickRight : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity).Rects.GetInnerContentRect();
        var offset = parent.Width - p.Instance.Rects.Intermediate.MaxX - p.Theme.Padding;
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(offset, 0);
    }
}
