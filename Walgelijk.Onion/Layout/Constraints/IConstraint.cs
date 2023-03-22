using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public interface IConstraint
{
    public void Apply(in ControlParams p);
}
