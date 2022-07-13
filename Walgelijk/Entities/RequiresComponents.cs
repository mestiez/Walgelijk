using System;

namespace Walgelijk;

/// <summary>
/// Attribute that lets the <see cref="Scene"/> know that something needs something else.
/// </summary>
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

    /// <summary>
    /// Get a unique ID for the types in the (treated as unordered) collection
    /// </summary>
    /// <returns></returns>
    public int GetTypeCollectionGroupId()
    {
        if (Types == null || Types.Length == 0)
            return 0;

        var a = new HashCode();
        for (int i = 0; i < Types.Length; i++)
            a.Add(Types[i]);
        return a.ToHashCode();
    }
}
