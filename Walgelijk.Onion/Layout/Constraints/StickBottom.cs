using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct StickBottom : IConstraint
{
    public readonly bool Pad;

    public StickBottom(bool pad)
    {
        this.Pad = pad;
    }

    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity).Rects.GetInnerContentRect();
        var offset = parent.Height - p.Instance.Rects.Intermediate.MaxY - (Pad ? p.Theme.Padding : 0);
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(0,offset);
    }
}