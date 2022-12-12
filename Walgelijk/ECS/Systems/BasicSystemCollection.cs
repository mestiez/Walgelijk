using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading;

namespace Walgelijk;

/// <summary>
/// Represents a thread safe collection of systems
/// </summary>
public class BasicSystemCollection : ISystemCollection
{
    private int systemCount;
    private readonly System[] systems;
    private readonly ConcurrentDictionary<Type, WeakReference<System>> systemsByType = new();
    private readonly SystemComparer systemComparer = new SystemComparer();
    private readonly ConcurrentBag<WeakReference<System>> toInitialise = new();
    private int incrementalIdentity = 0;

    /// <inheritdoc/>
    public int Count => systemCount;

    /// <summary>
    /// Maximum system amount. Can only be set on creation
    /// </summary>
    public readonly int Capacity;

    /// <inheritdoc/>
    public Scene? Scene { get; }

    /// <inheritdoc/>
    public event Action<System>? OnSystemAdded;
    /// <inheritdoc/>
    public event Action<System>? OnSystemRemoved;

    /// <summary>
    /// Create a new <see cref="BasicSystemCollection"/> with the given scene
    /// </summary>
    public BasicSystemCollection(Scene scene, int capacity = ushort.MaxValue)
    {
        Scene = scene;
        this.Capacity = capacity;
        systemCount = 0;
        systems = new System[capacity];
    }

    /// <summary>
    /// Create a new <see cref="BasicSystemCollection"/>
    /// </summary>
    public BasicSystemCollection(int capacity = ushort.MaxValue)
    {
        systemCount = 0;
        systems = new System[capacity];
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Add<T>(T s) where T : System
    {
        if (systemCount >= systems.Length)
            throw new Exception("Capacity exceeded");
        if (systemsByType.ContainsKey(typeof(T)))
            throw new DuplicateSystemException($"A system of type {typeof(T)} already exists");
        if (Scene != null)
            s.Scene = Scene;

        s.OrderOfAddition = incrementalIdentity++;
        InternalAddSystem(s);

        return s;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove<T>() where T : System => Remove(typeof(T));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(Type t)
    {
        if (TryGet(t, out var sys))
        {
            InternalRemoveSystem(sys);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get<T>() where T : System
    {
        if (systemsByType[typeof(T)].TryGetTarget(out var sys) && sys is T tsys)
            return tsys;
        throw new Exception("System does not exist");
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public System Get(Type t)
    {
        if (systemsByType[t].TryGetTarget(out var sys))
            return sys;
        throw new Exception("System does not exist");
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAll(System[] syss, int offset, int count)
    {
        int index = 0;
        for (int i = offset; i < Math.Min(Math.Min(systemCount, syss.Length), offset + count); i++)
            syss[i] = systems[index++];
        return index;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAll(Span<System> syss)
    {
        int index = 0;
        for (int i = 0; i < Math.Min(systemCount, syss.Length); i++)
            syss[i] = systems[index++];
        return index;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<System> GetAll() => systems.AsSpan(0, systemCount);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : System => Has(typeof(T));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has(Type t)
    {
        return systemsByType.ContainsKey(t);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet<T>([NotNullWhen(true)] out T? system) where T : System
    {
        if (systemsByType.TryGetValue(typeof(T), out var reference) && reference.TryGetTarget(out var sys) && sys is T tsys)
        {
            system = tsys;
            return true;
        }
        system = null;
        return false;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGet(Type t, [NotNullWhen(true)] out System? system)
    {
        if (systemsByType.TryGetValue(t, out var reference) && reference.TryGetTarget(out var sys))
        {
            system = sys;
            return true;
        }
        system = null;
        return false;
    }

    /// <inheritdoc/>
    public IEnumerator<System> GetEnumerator()
    {
        for (int i = 0; i < systemCount; i++)
            yield return systems[i];
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < systemCount; i++)
            yield return systems[i];
    }


    private void InternalRemoveSystem(System system)
    {
        int index = Array.IndexOf(systems, system);
        if (index >= 0)
        {
            OnSystemRemoved?.Invoke(system);
            Array.Copy(systems, index + 1, systems, index, systems.Length - index - 1);
            systemsByType.Remove(system.GetType(), out _);
            systemCount--;
        }

        Sort();
    }

    private void InternalAddSystem(System system)
    {
        OnSystemAdded?.Invoke(system);
        var type = system.GetType();
        systems[systemCount++] = system;
        var wr = new WeakReference<System>(system);
        systemsByType.TryAdd(type, wr);
        toInitialise.Add(wr);

        Sort();
    }

    /// <inheritdoc/>
    public void Sort()
    {
        Array.Sort(systems, 0, systemCount, systemComparer);
        foreach (var item in GetAll())
            item.ExecutionOrderChanged = false;
    }

    public void Dispose() { }

    public void InitialiseNewSystems()
    {
        while(toInitialise.TryTake(out var wr))
            if (wr.TryGetTarget(out var system))
                system.Initialise();
    }

    private struct SystemComparer : IComparer<System>
    {
        public int Compare(System? x, System? y)
        {
            int d = (x?.ExecutionOrder ?? int.MaxValue) - (y?.ExecutionOrder ?? int.MaxValue);
            if (d == 0)
                return (x?.OrderOfAddition ?? 0) - (y?.OrderOfAddition ?? 0);
            return d;
        }
    }
}
