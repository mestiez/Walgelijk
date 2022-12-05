using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Walgelijk;

/// <summary>
/// Represents a thread safe collection of components and their entities
/// </summary>
public class BasicComponentCollection : IComponentCollection
{
    private readonly ConcurrentDictionary<Type, LinkedList<Component>> components;
    private readonly ConcurrentDictionary<Entity, LinkedList<Component>> byEntity = new();

    private readonly ConcurrentBag<Component> componentsToAdd = new();
    private readonly ConcurrentBag<Component> componentsToDestroy = new();

    public readonly int CapacityPerType;
    public readonly int TypeCapacity;

    private int totalCount;

    /// <inheritdoc/>
    public int Count => totalCount;

    public BasicComponentCollection(int capacityPerType = 4096, int typeCapacity = 1024)
    {
        CapacityPerType = capacityPerType;
        TypeCapacity = typeCapacity;
        components = new ConcurrentDictionary<Type, LinkedList<Component>>(2, typeCapacity);
    }

    /// <inheritdoc/>
    public T Attach<T>(Entity entity, T component) where T : Component
    {
        component.Entity = entity;
        componentsToAdd.Add(component);
        if (!byEntity.TryGetValue(entity, out var coll))
        {
            coll = new LinkedList<Component>();
            if (!byEntity.TryAdd(entity, coll))
                throw new Exception("Failed to create component list for entity");
        }
        coll.AddLast(component);
        return component;
    }

    /// <inheritdoc/>
    public bool Contains<T>() where T : Component
    {
        if (components.TryGetValue(typeof(T), out var coll))
            return coll.Count > 0;
        return false;
    }

    /// <inheritdoc/>
    public IEnumerable<Component> GetAll()
    {
        foreach (var components in components.Values)
            foreach (var c in components)
                yield return c;
    }

    /// <inheritdoc/>
    public int GetAll(Span<Component> span)
    {
        int i = 0;
        foreach (var item in GetAll())
        {
            span[i++] = item;
            if (i >= span.Length)
                break;
        }
        return i;
    }

    /// <inheritdoc/>
    public int GetAll(Component[] arr, int offset, int count)
    {
        int i = 0;
        foreach (var item in GetAll())
        {
            arr[offset + i++] = item;
            if (i + offset >= arr.Length || i >= count)
                break;
        }
        return i;
    }

    /// <inheritdoc/>
    public IEnumerable<Component> GetAllFrom(Entity entity)
    {
        if (byEntity.TryGetValue(entity, out var coll))
            foreach (var comp in coll)
                yield return comp;
    }

    /// <inheritdoc/>
    public int GetAllFrom(Entity entity, Span<Component> span)
    {
        int i = 0;
        if (byEntity.TryGetValue(entity, out var coll))
            foreach (var comp in coll)
            {
                span[i++] = comp;
                if (i >= span.Length)
                    break;
            }
        return i;
    }

    /// <inheritdoc/>
    public int GetAllFrom(Entity entity, Component[] arr, int offset, int count)
    {
        int i = 0;
        if (byEntity.TryGetValue(entity, out var coll))
            foreach (var comp in coll)
            {
                arr[offset + i++] = comp;
                if (i >= count || offset + i >= arr.Length)
                    break;
            }
        return i;
    }

    /// <inheritdoc/>
    public IEnumerable<T> GetAllOfType<T>() where T : Component
    {
        if (components.TryGetValue(typeof(T), out var coll))
            foreach (var item in coll)
                if (item is T typed)
                    yield return typed;
    }

    /// <inheritdoc/>
    public int GetAllOfType<T>(Span<T> span) where T : Component
    {
        int i = 0;
        if (components.TryGetValue(typeof(T), out var coll))
            foreach (var item in coll)
                if (item is T typed)
                {
                    span[i++] = typed;
                    if (i >= span.Length)
                        break;
                }
        return i;
    }

    /// <inheritdoc/>
    public int GetAllOfType<T>(T[] arr, int offset, int count) where T : Component
    {
        int i = 0;
        if (components.TryGetValue(typeof(T), out var coll))
            foreach (var item in coll)
                if (item is T typed)
                {
                    arr[offset + i++] = typed;
                    if (i >= count || offset + i >= arr.Length)
                        break;
                }
        return i;
    }

    /// <inheritdoc/>
    public T GetFrom<T>(Entity entity) where T : Component
    {
        if (byEntity.TryGetValue(entity, out var coll))
            foreach (var comp in coll)
                if (comp is T typed)
                    return typed;

        throw new Exception($"There is no component of that type in entity {entity}");
    }

    /// <inheritdoc/>
    public bool TryGetFrom<T>(Entity entity, [NotNullWhen(true)] out T? component) where T : Component
    {
        if (byEntity.TryGetValue(entity, out var coll))
            foreach (var comp in coll)
                if (comp is T typed)
                {
                    component = typed;
                    return true;
                }

        component = null;
        return false;
    }

    /// <inheritdoc/>
    public bool Has<T>(Entity entity) where T : Component
    {
        if (byEntity.TryGetValue(entity, out var coll))
            foreach (var comp in coll)
                if (comp is T)
                    return true;
        return false;
    }

    /// <inheritdoc/>
    public bool HasEntity(Entity entity)
    {
        if (byEntity.TryGetValue(entity, out var list))
            return list.Any();
        return false;
    }

    /// <inheritdoc/>
    public bool Remove<T>(Entity entity) where T : Component
    {
        if (TryGetFrom<T>(entity, out var comp))
        {
            componentsToDestroy.Add(comp);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public bool Remove(Type type, Entity entity)
    {
        if (byEntity.TryGetValue(entity, out var coll))
            foreach (var comp in coll)
                if (comp.GetType().IsAssignableTo(type))
                {
                    componentsToDestroy.Add(comp);
                    return true;
                }

        return false;
    }

    /// <inheritdoc/>
    public void SyncBuffers()
    {
        while (componentsToAdd.TryTake(out var component))
        {
            var type = component.GetType();

            LinkedList<Component>? list;

            if (!components.TryGetValue(type, out list))
            {
                list = new LinkedList<Component>();
                if (!components.TryAdd(type, list))
                    throw new Exception("Failed to add component list to dictionary");
            }

            list.AddLast(component);
            totalCount++;
        }

        while (componentsToDestroy.TryTake(out var component))
        {
            var type = component.GetType();

            if (byEntity.TryGetValue(component.Entity, out var coll))
                coll.Remove(component);

            if (components.TryGetValue(type, out var list))
            {
                if (list.Remove(component))
                {
                    if (component is IDisposable disp)
                        disp.Dispose();
                    totalCount--;
                }
                else
                    throw new Exception("Failed to remove component from list");
            }
        }
    }

    public bool Remove(Entity entity)
    {
        byEntity.TryRemove(entity, out var coll);
        coll?.Clear();

        foreach (var item in GetAllFrom(entity))
            componentsToDestroy.Add(item);
        return true;
    }

    public void Dispose()
    {
        foreach (var list in components.Values)
        {
            foreach (var c in list)
                if (c is IDisposable d)
                    d.Dispose();
            list.Clear();
        }
        components.Clear();
        byEntity.Clear();
        componentsToAdd.Clear();
        componentsToDestroy.Clear();
    }
}