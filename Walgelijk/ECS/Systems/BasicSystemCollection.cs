using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;

namespace Walgelijk;

public class BasicSystemCollection : ISystemCollection
{
    private int systemCount;
    private readonly System[] systems;

    private readonly ConcurrentDictionary<Type, WeakReference<System>> systemsByType = new();

    private readonly ConcurrentBag<System> systemsToAdd = new();
    private readonly ConcurrentBag<System> systemsToDestroy = new();

    private readonly Mutex mut = new();

    /// <inheritdoc/>
    public int Count => systemCount;

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
    public T Add<T>(T s) where T : System
    {
        if (systemCount >= systems.Length)
            throw new Exception("Capacity exceeded");

        if (systemsByType.ContainsKey(typeof(T)))
            throw new DuplicateSystemException($"A system of type {typeof(T)} already exists");
        if (Scene != null)
            s.Scene = Scene;
        systemsToAdd.Add(s);
        return s;
    }

    /// <inheritdoc/>
    public T Get<T>() where T : System
    {
        if (systemsByType[typeof(T)].TryGetTarget(out var sys) && sys is T tsys)
            return tsys;
        throw new Exception("System does not exist");
    }

    /// <inheritdoc/>
    public System Get(Type t)
    {
        if (systemsByType[t].TryGetTarget(out var sys))
            return sys;
        throw new Exception("System does not exist");
    }

    /// <inheritdoc/>
    public int GetAll(System[] syss, int offset, int count)
    {
        int index = 0;
        for (int i = offset; i < Math.Min(Math.Min(systemCount, syss.Length), offset + count); i++)
            syss[i] = systems[index++];
        return index;
    }

    /// <inheritdoc/>
    public int GetAll(Span<System> syss)
    {
        int index = 0;
        for (int i = 0; i < Math.Min(systemCount, syss.Length); i++)
            syss[i] = systems[index++];
        return index;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<System> GetAll() => systems.AsSpan(0, systemCount);

    /// <inheritdoc/>
    public bool Has<T>() where T : System => Has(typeof(T));

    /// <inheritdoc/>
    public bool Has(Type t)
    {
        return systemsByType.ContainsKey(t);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        mut.Dispose();
    }

    /// <inheritdoc/>
    public void SyncBuffers()
    {
        bool shouldSort = systemsToAdd.Any() || systemsToDestroy.Any();

        foreach (var system in systemsToAdd)
        {
            OnSystemAdded?.Invoke(system);
            var type = system.GetType();
            systems[systemCount++] = system;
            systemsByType.TryAdd(type, new WeakReference<System>(system));
            system.ExecutionOrderChanged = false;
            system.Initialise();
        }
        systemsToAdd.Clear();

        foreach (var system in systemsToDestroy)
        {
            int index = Array.IndexOf(systems, system);
            if (index >= 0)
            {
                OnSystemRemoved?.Invoke(system);
                Array.Copy(systems, index + 1, systems, index, systems.Length - index - 1);
                systemsByType.Remove(system.GetType(), out _);
                systemCount--;
            }
        }
        systemsToDestroy.Clear();

        if (shouldSort)
            Sort();
    }

    /// <inheritdoc/>
    public void Sort() => Array.Sort(systems, 0, systemCount, new SystemComparer());

    /// <inheritdoc/>
    public bool Remove<T>() where T : System => Remove(typeof(T));

    /// <inheritdoc/>
    public bool Remove(Type t)
    {
        if (TryGet(t, out var sys))
        {
            systemsToDestroy.Add(sys);
            return true;
        }
        return false;
    }

    private struct SystemComparer : IComparer<System>
    {
        public int Compare(System? x, System? y) => (x?.ExecutionOrder ?? int.MaxValue) - (y?.ExecutionOrder ?? int.MaxValue);
    }
}
