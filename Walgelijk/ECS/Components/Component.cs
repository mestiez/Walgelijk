namespace Walgelijk;

/// <summary>
/// Component base class. Contains a reference to the entity that is attached.
/// </summary>
public abstract class Component
{
    /// <summary>
    /// The entity that this component is attached to
    /// </summary>
    public Entity Entity { get; internal set; }
}