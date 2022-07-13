using System;

namespace Walgelijk;

/// <summary>
/// Attribute that lets the scene know that a System can be ran in parallel to others. A <see cref="RequiresComponents"/> attribute can be used alongside this to make sure two systems that edit the same data are ran sequentially, atomically.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ParallelAttribute : Attribute
{
}