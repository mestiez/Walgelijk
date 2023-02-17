using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Layout;

public interface ILayout : IGroupLayout, ISingleLayout
{
    void IGroupLayout.Calculate(in ControlParams p, int index) => CalculateEither(p);

    void ISingleLayout.Calculate(in ControlParams p) => CalculateEither(p);

    public void CalculateEither(in ControlParams p);
}
