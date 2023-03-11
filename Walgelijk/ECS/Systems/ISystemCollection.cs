using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk;

/// <summary>
/// Represents a thread safe collection of systems
/// </summary>
public interface ISystemCollection : IEnumerable<System>, IDisposable
{
    /// <summary>
    /// Scene that this collection belongs to
    /// </summary>
    public Scene? Scene { get; }

    /// <summary>
    /// Adds a system of type T to the collection.
    /// </summary>
    public T Add<T>(T s) where T : System;
    /// <summary>
    /// Remove a system of type T from the collection.
    /// </summary>
    public bool Remove<T>() where T : System;
    /// <summary>
    /// Remove a system of the given type from the collection.
    /// </summary>
    public bool Remove(Type t);

    /// <summary>
    /// Retrieves a system of type T from the collection.
    /// </summary>
    public T Get<T>() where T : System;
    /// <summary>
    /// Retrieves a system from the collection.
    /// </summary>
    public System Get(Type t);

    /// <summary>
    /// Attempts to retrieve a system of type T from the collection.
    /// </summary>
    public bool TryGet<T>([NotNullWhen(true)] out T? system) where T : System;
    /// <summary>
    /// Attempts to retrieve a system from the collection.
    /// </summary>
    public bool TryGet(Type t, [NotNullWhen(true)] out System? system);

    /// <summary>
    /// Determines whether the collection contains a system of the specified type.
    /// </summary>
    public bool Has<T>() where T : System;
    /// <summary>
    /// Determines whether the collection contains a system of the specified type.
    /// </summary>
    public bool Has(Type t);

    /// <summary>
    /// Gets the number of systems in the collection.
    /// </summary>
    public int Count { get; }
    /// <summary>
    /// Puts all systems in the collection into the specified array given an offset and maximum amount
    /// </summary>
    public int GetAll(System[] systems, int offset, int count);
    /// <summary>
    /// Puts all systems in the collection into the specified span
    /// </summary>
    public int GetAll(Span<System> systems);
    /// <summary>
    /// Returns a span with all systems
    /// </summary>
    public ReadOnlySpan<System> GetAll();

    /// <summary>
    /// Called after the collection is edited to sort it based on ExecutionOrder
    /// </summary>
    public void Sort();

    /// <summary>
    /// Initialise every new system, after which they are removed from the new system list
    /// </summary>
    public void InitialiseNewSystems();

    /// <summary>
    /// Fired right after a system is added
    /// </summary>
    public event Action<System>? OnSystemAdded;
    /// <summary>
    /// Fired right before a system is removed
    /// </summary>
    public event Action<System>? OnSystemRemoved;
}
