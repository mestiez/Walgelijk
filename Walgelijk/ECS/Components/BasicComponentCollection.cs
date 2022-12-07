using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Walgelijk;

public class FilterComponentCollection : IComponentCollection
{
    /* Het idee is dat er een dictionary is van "filters" wat eigenlijk gewoon een query is met 1 level
     * Als iemand iets opvraagt (by entity, by type, wat dan ook) dan pakt het deze filter uit de dict
     * als het nog niet bestaat dan maakt hij het
     * als er een component wordt toegevoegd of verwijdert dan kijkt het naar de filters en past alle relevante lijsten aan
     * IEnumerable<Filter> GetRelevantFilters(Component) of misschien dat het direct de lijsten uit de dict teruggeeft
     */ 

    public int Count => throw new NotImplementedException();

    public T Attach<T>(Entity entity, T component) where T : Component
    {
        throw new NotImplementedException();
    }

    public bool Contains<T>() where T : Component
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Component> GetAll()
    {
        throw new NotImplementedException();
    }

    public int GetAll(Span<Component> span)
    {
        throw new NotImplementedException();
    }

    public int GetAll(Component[] span, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Component> GetAllFrom(Entity entity)
    {
        throw new NotImplementedException();
    }

    public int GetAllFrom(Entity entity, Span<Component> span)
    {
        throw new NotImplementedException();
    }

    public int GetAllFrom(Entity entity, Component[] arr, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetAllOfType<T>() where T : Component
    {
        throw new NotImplementedException();
    }

    public int GetAllOfType<T>(Span<T> span) where T : Component
    {
        throw new NotImplementedException();
    }

    public int GetAllOfType<T>(T[] arr, int offset, int count) where T : Component
    {
        throw new NotImplementedException();
    }

    public T GetFrom<T>(Entity entity) where T : Component
    {
        throw new NotImplementedException();
    }

    public bool Has<T>(Entity entity) where T : Component
    {
        throw new NotImplementedException();
    }

    public bool HasEntity(Entity entity)
    {
        throw new NotImplementedException();
    }

    public bool Remove<T>(Entity entity) where T : Component
    {
        throw new NotImplementedException();
    }

    public bool Remove(Type type, Entity entity)
    {
        throw new NotImplementedException();
    }

    public bool Remove(Entity entity)
    {
        throw new NotImplementedException();
    }

    public void SyncBuffers()
    {
        throw new NotImplementedException();
    }

    public bool TryGetFrom<T>(Entity entity, [NotNullWhen(true)] out T? component) where T : Component
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Represents a thread safe collection of components and their entities
/// </summary>
public class BasicComponentCollection : IComponentCollection
{
    private readonly ConcurrentDictionary<Type, LinkedList<Component>> components;
    private readonly ConcurrentDictionary<Entity, LinkedList<Component>> byEntity = new();

    private readonly ConcurrentDictionary<Type, HashSet<Type>> typeUmbrella = new();

    private readonly ConcurrentBag<Component> componentsToAdd = new();
    private readonly ConcurrentBag<Component> componentsToDestroy = new();

    private int totalCount;

    /// <inheritdoc/>
    public int Count => totalCount;

    public BasicComponentCollection()
    {
        components = new ConcurrentDictionary<Type, LinkedList<Component>>();
    }

    /// <inheritdoc/>
    public T Attach<T>(Entity entity, T component) where T : Component
    {
        component.Entity = entity;
        componentsToAdd.Add(component);
        byEntity.Ensure(entity).AddLast(component);
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
        foreach (var comp in byEntity.Ensure(entity))
            yield return comp;
    }

    /// <inheritdoc/>
    public int GetAllFrom(Entity entity, Span<Component> span)
    {
        int i = 0;
        foreach (var comp in byEntity.Ensure(entity))
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
        foreach (var comp in byEntity.Ensure(entity))
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
        var t = typeof(T);
        if (typeUmbrella.ContainsKey(t))
            CheckAndUpdateFor(t);

        foreach (var types in typeUmbrella.Ensure(t))
            foreach (var item in components.Ensure(types))
                if (item is T typed)
                    yield return typed;
    }

    /// <inheritdoc/>
    public int GetAllOfType<T>(Span<T> span) where T : Component
    {
        var t = typeof(T);
        if (typeUmbrella.ContainsKey(t))
            CheckAndUpdateFor(t);

        int i = 0;
        foreach (var types in typeUmbrella.Ensure(t))
            foreach (var item in components.Ensure(types))
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
        var t = typeof(T);
        if (typeUmbrella.ContainsKey(t))
            CheckAndUpdateFor(t);

        int i = 0;
        foreach (var types in typeUmbrella.Ensure(t))
            foreach (var item in components.Ensure(types))
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
        foreach (var comp in byEntity.Ensure(entity))
            if (comp is T typed)
                return typed;

        throw new Exception($"There is no component of that type in entity {entity}");
    }

    /// <inheritdoc/>
    public bool TryGetFrom<T>(Entity entity, [NotNullWhen(true)] out T? component) where T : Component
    {
        foreach (var comp in byEntity.Ensure(entity))
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
        foreach (var comp in byEntity.Ensure(entity))
            if (comp is T)
                return true;
        return false;
    }

    /// <inheritdoc/>
    public bool HasEntity(Entity entity)
    {
        return byEntity.Ensure(entity).Any();
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
        foreach (var comp in byEntity.Ensure(entity))
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
            components.Ensure(type).AddLast(component);
            CheckAndUpdateFor(type);
            totalCount++;
        }

        while (componentsToDestroy.TryTake(out var component))
        {
            var type = component.GetType();

            if (byEntity.TryGetValue(component.Entity, out var coll))
                coll.Remove(component);

            if (components.Ensure(type).Remove(component))
            {
                if (component is IDisposable disp)
                    disp.Dispose();
                totalCount--;
            }
            else
                throw new Exception("Failed to remove component from list");
        }
    }

    private void CheckAndUpdateFor(Type type)
    {
        var list = typeUmbrella.Ensure(type);
        if (list.Count == 0)
            list.Add(type);
        foreach (var potentialDerivative in components.Keys)
            if (!list.Contains(potentialDerivative) && potentialDerivative.IsAssignableTo(type))
                list.Add(potentialDerivative);
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