using System.Collections.Generic;
/// <summary>
/// A container for <see cref="IComponentDisposer"/>s. It deals with disposing specific resources used by components.
/// </summary>
public class ComponentDisposalManager
{
    private List<IComponentDisposer> disposers = new();

    /// <summary>
    /// Add a disposer instance to the disposer collection
    /// </summary>
    /// <param name="disposer"></param>
    public void RegisterDisposer(IComponentDisposer disposer)
    {
        disposers.Add(disposer);
    }

    /// <summary>
    /// Dispose of the component object using all registered components
    /// </summary>
    public void DisposeOf(object obj)
    {
        foreach (var item in disposers)
            if (item.CanDisposeOf(obj))
                item.DisposeOf(obj);
    }
}
