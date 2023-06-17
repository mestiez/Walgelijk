using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

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
