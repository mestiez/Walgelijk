using Walgelijk.Onion.Assets;
using Walgelijk.Onion.Controls;

namespace Walgelijk.Onion.Decorators;

/// <summary>
/// Provides an interface for decorators, processed by <see cref="DecoratorQueue"/>.
/// Decorators provide a way to add extra visuals to a control
/// </summary>
public interface IDecorator
{
    public void RenderBefore(in ControlParams p);
    public void RenderAfter (in ControlParams p);
}
