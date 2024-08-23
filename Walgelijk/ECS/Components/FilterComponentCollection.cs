using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk;

public class FilterComponentCollection : IComponentCollection
{
    private readonly Dictionary<Filter, HashSet<Component>> components = new();
    private readonly LinkedList<Component> toAdd = new();
    private readonly LinkedList<Component> toDestroy = new();
    private readonly HashSet<Component> all = new();

    public int Count => all.Count + toAdd.Count;

    public T Attach<T>(Entity entity, T component) where T : Component
    {
        component.Entity = entity;
        toAdd.AddLast(component);

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
                return i;
        }
        foreach (var item in toAdd)
        {
            span[i++] = item;
            if (i >= span.Length)
                return i;
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
                return i;
        }

        foreach (var item in toAdd)
        {
            arr[offset + i++] = item;
            if (i >= count || offset + i >= arr.Length)
                return i;
        }

        return i;
    }

    public IEnumerable<Component> GetAllFrom(Entity entity)
    {
        foreach (var item in components.Ensure(new Filter(entity)))
            yield return item;

        foreach (var item in toAdd)
            if (item.Entity == entity)
                yield return item;
    }

    public int GetAllFrom(Entity entity, Span<Component> span)
    {
        int i = 0;

        foreach (var item in components.Ensure(new Filter(entity)))
        {
            span[i++] = item;
            if (i >= span.Length)
                return i;
        }

        foreach (var item in toAdd)
        {
            if (item.Entity != entity)
                continue;
            span[i++] = item;
            if (i >= span.Length)
                return i;
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
                return i;
        }

        foreach (var item in toAdd)
        {
            if (item.Entity != entity)
                continue;
            arr[offset + i++] = item;
            if (i >= count || offset + i >= arr.Length)
                return i;
        }
        return i;
    }

    public IEnumerable<T> GetAllOfType<T>() where T : Component
    {
        var list = components.Ensure(new Filter(type: typeof(T)));

        foreach (var item in list)
            yield return (T)item;

        foreach (var item in toAdd)
            if (item is T tt)
                yield return tt;
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
        var list = components.Ensure(new Filter(entity, typeof(T)));
        foreach (var item in list)
            return item as T ?? throw new Exception("The component that was found in the typed component list was of the incorrect type and this error is so severe that you should probably use a different game engine");

        foreach (var item in toAdd)
            if (item.Entity == entity && item is T tt)
                return tt;

        throw new Exception($"Entity has no component of type {typeof(T)}");
    }

    public bool Has<T>(Entity entity) where T : Component
    {
        var list = components.Ensure(new Filter(entity, typeof(T)));
        foreach (var item in list)
            if (item is T)
                return true;

        foreach (var item in toAdd)
            if (item.Entity == entity && item is T)
                return true;

        return false;
    }

    public bool HasEntity(Entity entity)
    {
        foreach (var item in toAdd)
            if (item.Entity == entity)
                return true;
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
            if (!toDestroy.Contains(item) && item.GetType().IsAssignableTo(type))
            {
                toDestroy.AddLast(item);
                return true;
            }
        return false;
    }

    public bool Remove(Entity entity)
    {
        foreach (var item in components.Ensure(new Filter(entity)))
            if (!toDestroy.Contains(item))
                toDestroy.AddLast(item);
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
        foreach (var item in toAdd)
            if (item.Entity == entity && item is T t)
            {
                component = t;
                return true;
            }
        component = null;
        return false;
    }

    public void SyncBuffers()
    {
        foreach (var c in toAdd)
            InternalAddComponent(c);

        foreach (var c in toDestroy)
            InternalRemoveComponent(c);

        toAdd.Clear();
        toDestroy.Clear();
    }

    private void InternalRemoveComponent(Component component)
    {
        foreach (var filter in GetFiltersFor(component))
        {
            var list = components[filter];
            if (!list.Remove(component))
                throw new Exception($"Failed to remove component {component} from filtered list: {filter}");
        }

        all.Remove(component);
        if (component is IDisposable disp)
            disp.Dispose();
    }

    private void InternalAddComponent(Component component)
    {
        if (Game.Main?.DevelopmentMode ?? true)
            AssertComponentRequirements(component);

        all.Add(component);
        foreach (var filter in GetFiltersFor(component))
            components.Ensure(filter).Add(component);
    }

    private void AssertComponentRequirements<T>(T component) where T : Component
    {
        var constraints = ReflectionCache.GetAttributes<ComponentAttribute, T>();

        foreach (var constraint in constraints)
            constraint.Assert(component, this);
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
}
