using System.Numerics;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct HeightConstraint : IConstraint
{
    public readonly float Height;

    public HeightConstraint(float h)
    {
        Height = h;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.MaxY = p.Instance.Rects.Intermediate.MinY + Height;
    }
}

public readonly struct ClampToContainer : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent == null)
            return;

        //TODO dit werkt niet bij local draggables
        var parent = p.Tree.EnsureInstance(p.Node.Parent.Identity);
        var c = parent.Rects.Intermediate.Expand(-Onion.Theme.Padding);
        p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.ClampInside(c);
    }
}