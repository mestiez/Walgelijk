using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public interface IGroupLayout
{
    public void Calculate(in ControlParams p, int index);
}
