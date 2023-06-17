using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public readonly struct EscapeScroll : IConstraint
{
    public void Apply(in ControlParams p)
    {
        p.Node.ContributeToParentScrollRect = false;
    }
}
