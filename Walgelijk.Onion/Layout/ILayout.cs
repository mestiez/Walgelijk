using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public interface ILayout
{
    public void Apply(in ControlParams p, int index);
}
