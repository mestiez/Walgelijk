
/// <summary>
/// A disposer for a component. Contains a method that is meant to free all resources used up by the given component. Register a custom component disposer using <see cref="ComponentDisposalManager"/>
/// </summary>
public interface IComponentDisposer
{
    /// <summary>
    /// Dispose of the given component
    /// </summary>
    public bool DisposeOf(object obj);

    /// <summary>
    /// Returns true if the disposer can deal with the given object
    /// </summary>
    public bool CanDisposeOf(object obj);
}
