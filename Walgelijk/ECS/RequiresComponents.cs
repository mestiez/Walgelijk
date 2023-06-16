using System;

namespace Walgelijk;

/// <summary>
/// Attribute that lets the <see cref="Scene"/> know that something needs something else.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequiresComponents : ComponentAttribute
{
    /// <summary>
    /// Relevant component types
    /// </summary>
    public readonly global::System.Type[]? Types;

    /// <summary>
    /// Construct a <see cref="RequiresComponents"/> with the given types
    /// </summary>
    public RequiresComponents(params global::System.Type[] componentTypes)
    {
        Types = componentTypes;
        Array.Sort(Types, static (a, b) => a.GetHashCode() - b.GetHashCode()); // makes sure the same types are always in the same place
    }

    public override void Assert<T>(T component, IComponentCollection components)
    {
        if (Types != null)
            foreach (var type in Types)
            {
                HasComponentType(component, components, type);
                return;
            }
    }

    private static void HasComponentType<T>(T component, IComponentCollection components, Type type) where T : Component
    {
        foreach (var e in components.GetAllFrom(component.Entity))
            if (type.IsAssignableFrom(e.GetType()))
                return;
        throw new Exception($"{component.GetType()} requires a {type}");
    }
}
