using System;

namespace Walgelijk;

/// <summary>
/// Attribute that lets the <see cref="Scene"/> know that something needs something else.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequiresComponents : ComponentRelationAttribute
{
    /// <summary>
    /// Construct a <see cref="RequiresComponents"/> with the given types
    /// </summary>
    public RequiresComponents(params global::System.Type[] componentTypes)
    {
        Types = componentTypes;
        Array.Sort(Types, static (a, b) => a.GetHashCode() - b.GetHashCode()); // makes sure the same types are always in the same place
    }
}
