using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct Overflow(bool horizontalScroll, bool verticalScroll) : IConstraint
{
    public void Apply(in ControlParams p)
    {
        if (verticalScroll)
            p.Instance.OverflowBehaviour |= OverflowBehaviour.ScrollVertical;
        else
            p.Instance.OverflowBehaviour &= ~OverflowBehaviour.ScrollVertical;

        if (horizontalScroll)
            p.Instance.OverflowBehaviour |= OverflowBehaviour.ScrollHorizontal;
        else
            p.Instance.OverflowBehaviour &= ~OverflowBehaviour.ScrollHorizontal;
    }
}
