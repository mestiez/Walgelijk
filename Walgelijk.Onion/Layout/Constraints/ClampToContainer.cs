using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct ClampToContainer : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        //TODO dit werkt niet bij local draggables
        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity);
        var c = parent.Rects.Intermediate.Expand(-p.Theme.Padding);
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.ClampInside(c);
    }
}
