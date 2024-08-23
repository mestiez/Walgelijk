namespace Walgelijk;

/// <summary>
/// Base class for attributes that indicate a component constraint
/// </summary>
public abstract class ComponentAttribute : global::System.Attribute
{
    public abstract void Assert<T>(T component, IComponentCollection components) where T : Component;
}
