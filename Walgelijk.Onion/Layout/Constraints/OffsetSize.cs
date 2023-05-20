using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct OffsetSize : IConstraint
{
    public readonly float Width;
    public readonly float Height;

    public OffsetSize(float w, float h)
    {
        Width = w;
        Height = h;
    }

    public void Apply(in ControlParams p)
    {
        p.Instance.Rects.Intermediate.Width += Width;
        p.Instance.Rects.Intermediate.Height += Height;
    }
}

public readonly struct Order : IConstraint
{
    public readonly int LocalOrder;

    public Order(int localOrder)
    {
        LocalOrder = localOrder;
    }

    public void Apply(in ControlParams p)
        => p.Node.RequestedLocalOrder = LocalOrder;
}

public readonly struct EscapeScroll : IConstraint
{
    public void Apply(in ControlParams p)
    {
        p.Node.ContributeToParentScrollRect = false;
    }
}

public readonly struct IgnoreScroll : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (p.Node.Parent != null && p.Tree.Instances.TryGetValue(p.Node.Parent.Identity, out var parent))
            p.Instance.Rects.Intermediate = p.Instance.Rects.Intermediate.Translate(-parent.InnerScrollOffset);
    }
}