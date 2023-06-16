using System;

namespace Walgelijk;

/// <summary>
/// Attribute that ensures no more than one instance of the component may exist in the scene at a time
/// </summary>
public sealed class SingleInstanceAttribute : ComponentAttribute
{
    public override void Assert<T>(T component, IComponentCollection components)
    {
        if (components.Contains<T>())
            throw new Exception($"{typeof(T)} has a \"single instance constraint\" but an attempt was made to add it to the scene more than once");
    }
}