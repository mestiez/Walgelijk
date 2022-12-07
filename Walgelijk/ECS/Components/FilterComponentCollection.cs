using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Walgelijk;

public class FilterComponentCollection : IComponentCollection
{
    private readonly Dictionary<Filter, HashSet<Component>> components = new();
    private readonly Queue<Component> toAdd = new();
    private readonly Queue<Component> toDestroy = new();
    private readonly HashSet<Component> all = new();

    private bool isInLoop;

    public int Count => all.Count;

    public T Attach<T>(Entity entity, T component) where T : Component
    {
        component.Entity = entity;
        if (isInLoop)
            toAdd.Enqueue(component);
        else
            InternalAddComponent(component);

        return component;
    }

    public bool Contains<T>() where T : Component
    {
        return components.ContainsKey(new Filter(type: typeof(T)));
    }

    public IEnumerable<Component> GetAll()
    {
        return all;
    }

    public int GetAll(Span<Component> span)
    {
        int i = 0;
        foreach (var item in all)
        {
            span[i++] = item;
            if (i >= span.Length)
                break;
        }
        return i;
    }

    public int GetAll(Component[] arr, int offset, int count)
    {
        int i = 0;
        foreach (var item in all)
        {
            arr[offset + i++] = item;
            if (i >= count || offset + i >= arr.Length)
                break;
        }
        return i;
    }

    public IEnumerable<Component> GetAllFrom(Entity entity)
    {
        foreach (var item in components.Ensure(new Filter(entity)))
            yield return item;
    }

    public int GetAllFrom(Entity entity, Span<Component> span)
    {
        int i = 0;
        foreach (var item in components.Ensure(new Filter(entity)))
        {
            span[i++] = item;
            if (i >= span.Length)
                break;
        }
        return i;
    }

    public int GetAllFrom(Entity entity, Component[] arr, int offset, int count)
    {
        int i = 0;
        foreach (var item in components.Ensure(new Filter(entity)))
        {
            arr[offset + i++] = item;
            if (i >= count || offset + i >= arr.Length)
                break;
        }
        return i;
    }

    public IEnumerable<T> GetAllOfType<T>() where T : Component
    {
        using var marker = new LoopMarker(this);

        var list = components.Ensure(new Filter(type: typeof(T)), out var isnew);

        //if (isnew)
        //    foreach (var item in all)
        //        EnsureFilterSync(item);

        foreach (var item in list)
            if (item is T tt)
                yield return tt;

        marker.Dispose();
    }

    public int GetAllOfType<T>(Span<T> span) where T : Component
    {
        int i = 0;
        foreach (var item in GetAllOfType<T>())
            if (item is T typed)
            {
                span[i++] = typed;
                if (i >= span.Length)
                    break;
            }
        return i;
    }

    public int GetAllOfType<T>(T[] arr, int offset, int count) where T : Component
    {
        int i = 0;
        foreach (var item in GetAllOfType<T>())
            if (item is T typed)
            {
                arr[offset + i++] = typed;
                if (i >= count || offset + i >= arr.Length)
                    break;
            }
        return i;
    }

    public T GetFrom<T>(Entity entity) where T : Component
    {
        var list = components.Ensure(new Filter(entity, typeof(T)), out var isnew);
        //if (isnew)
        //    foreach (var item in all)
        //        EnsureFilterSync(item);
        foreach (var item in list)
            return item as T ?? throw new Exception(
                "The component that was found in the typed component list was of the incorrect type and this error is so severe that you should probably use a different game engine");
        throw new Exception("Entity has no component of that type");
    }

    public bool Has<T>(Entity entity) where T : Component
    {
        return components.ContainsKey(new Filter(type: typeof(T)));
    }

    public bool HasEntity(Entity entity)
    {
        return components.ContainsKey(new Filter(entity));
    }

    public bool Remove<T>(Entity entity) where T : Component
    {
        return Remove(typeof(T), entity);
    }

    public bool Remove(Type type, Entity entity)
    {
        var list = components.Ensure(new Filter(entity, type));
        foreach (var item in list)
            if (item.GetType().IsAssignableTo(type))
            {
                toDestroy.Enqueue(item);
                return true;
            }
        return false;
    }

    public bool Remove(Entity entity)
    {
        foreach (var item in components.Ensure(new Filter(entity)))
            toDestroy.Enqueue(item);
        return true;
    }

    public bool TryGetFrom<T>(Entity entity, [NotNullWhen(true)] out T? component) where T : Component
    {
        var list = components.Ensure(new Filter(entity, typeof(T)));
        foreach (var item in list)
            if (item is T t)
            {
                component = t;
                return true;
            }
        component = null;
        return false;
    }

    public void SyncBuffers()
    {
        while (toAdd.TryDequeue(out var component))
            InternalAddComponent(component);

        while (toDestroy.TryDequeue(out var component))
            InternalRemoveComponent(component);
    }

    private void InternalRemoveComponent(Component component)
    {
        bool success = false;
        foreach (var filter in GetFiltersFor(component))
            success |= components.Ensure(filter).Remove(component);

        if (success)
        {
            if (component is IDisposable disp)
                disp.Dispose();
            all.Remove(component);

        }
        else
            throw new Exception("Failed to remove component from list");
    }

    private void InternalAddComponent(Component component)
    {
        all.Add(component);
        foreach (var filter in GetFiltersFor(component))
        {
            //Console.WriteLine("{0} for {1}", filter, component);
            components.Ensure(filter).Add(component);
        }
    }

    public void Dispose()
    {
        foreach (var c in all)
            if (c is IDisposable d)
                d.Dispose();
        all.Clear();
        components.Clear();
        toAdd.Clear();
        toDestroy.Clear();
    }

    //private void EnsureFilterSync<T>(T component) where T : Component
    //{
    //    foreach (var filter in GetFiltersFor(component))
    //    {
    //        var list = components.Ensure(filter);
    //        if (list.Contains(component))
    //            continue;

    //        list.Add(component);
    //    }
    //}

    private IEnumerable<Filter> GetFiltersFor<T>(T component) where T : Component
    {
        var type = component.GetType();

        var base1 = new Filter(entity: component.Entity);
        var base2 = new Filter(type: type);
        var base3 = new Filter(component.Entity, type);

        yield return base1;
        yield return base2;
        yield return base3;

        foreach (var baseType in ReflectionCache.GetAllBaseTypes(type))
        {
            if (!baseType.IsAssignableTo(typeof(Component)))
                continue;

            yield return new Filter(type: baseType);
            yield return new Filter(component.Entity, baseType);
        }
    }

    public readonly struct Filter : IEquatable<Filter>
    {
        public readonly Entity? ByEntity;
        public readonly Type? ByType;

        public Filter(Entity? entity = null, Type? type = null)
        {
            ByEntity = entity;
            ByType = type;
        }

        public override bool Equals(object? obj) => obj is Filter filter && Equals(filter);

        public bool Equals(Filter other) => ByEntity == other.ByEntity && ByType == other.ByType;

        public override int GetHashCode() => HashCode.Combine(ByEntity, ByType);

        public static bool operator ==(Filter left, Filter right) => left.Equals(right);

        public static bool operator !=(Filter left, Filter right) => !(left == right);

        public readonly bool Passes<T>(T component) where T : Component
        {
            if (ByEntity.HasValue && ByEntity.Value != component.Entity)
                return false;
            if (ByType != null && !component.GetType().IsAssignableTo(ByType))
                return false;
            return true;
        }

        public override string ToString() => $"{ByType?.ToString() ?? "any type"} on {ByEntity?.ToString() ?? "any entity"}";
    }

    private readonly struct LoopMarker : IDisposable
    {
        public readonly FilterComponentCollection coll;

        public LoopMarker(FilterComponentCollection coll)
        {
            this.coll = coll;
            coll.isInLoop = true;
        }

        public void Dispose()
        {
            coll.isInLoop = false;
        }
    }
}
