using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Walgelijk
{
    /// <summary>
    /// A collection of components
    /// </summary>
    public class ComponentCollection
    {
        private readonly HashSet<EntityWithAnything> allComponents = new HashSet<EntityWithAnything>();

        private readonly Dictionary<Type, List<EntityWithAnything>> byType = new();
        private readonly Dictionary<Entity, List<object>> byEntity = new();

        /// <summary>
        /// Add a component to the collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(object component, Entity entity)
        {
            var a = new EntityWithAnything(component, entity);
            var t = component.GetType();
            allComponents.Add(a);

            if (byEntity.TryGetValue(entity, out var list))
                list.Add(component);
            else
            {
                var l = new List<object>
                {
                    component
                };
                byEntity.Add(entity, l);
            }

            foreach (var item in byType)
            {
                var target = item.Key;

                if (t.IsAssignableTo(target))
                    item.Value.Add(a);
            }
        }

        /// <summary>
        /// Iterates over components by type
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<EntityWith<T>> GetAllComponentsOfType<T>() where T : class
        {
            var t = typeof(T);

            if (!byType.TryGetValue(t, out var list))
                TryCreateNewTypeList(t, out list);

            foreach (var item in list)
                yield return new EntityWith<T>(item.Component as T, item.Entity);
        }

        /// <summary>
        /// Iterate over all components attached to an entity
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable GetAllComponentsFrom(Entity entity)
        {
            if (byEntity.ContainsKey(entity))
                foreach (var a in byEntity[entity])
                        yield return a;
        }

        /// <summary>
        /// Get the entity that the given component is attached to
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetEntityFromComponent(object comp, out Entity entity)
        {
            entity = 0;
            foreach (var c in allComponents)
                if (c.Component == comp)
                {
                    entity = c.Entity;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Delete all components belonging to the given entity
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool DeleteEntity(Entity entity)
        {
            bool c = false;

            allComponents.RemoveWhere(t => t.Entity == entity);

            foreach (var listPair in byType)
            {
                var l = listPair.Value;
                for (int i = l.Count - 1; i >= 0; i--)
                    if (l[i].Entity == entity)
                    {
                        l.RemoveAt(i);
                        c = true;
                    }
            }

            c &= byEntity.Remove(entity);

            return c;
        }

        /// <summary>
        /// Detach a component of a type from an entity
        /// </summary>
        /// <returns>True if anything was removed</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponentOfType<T>(Entity entity)
        {
            return RemoveComponentOfType(typeof(T), entity);
        }

        /// <summary>
        /// Detach a component of a type from an entity
        /// </summary>
        /// <returns>True if anything was removed</returns>
        public bool RemoveComponentOfType(Type type, Entity entity)
        {
            bool success = true;
            TryCreateNewTypeList(type, out _);

            allComponents.RemoveWhere(t => type.IsInstanceOfType(t.Component) && t.Entity == entity);

            success &= !byType.TryGetValue(type, out var list);
            success &= list.RemoveAll(a => a.Entity == entity) > 0;

            success &= !byEntity.TryGetValue(entity, out var listb);
            success &= listb.RemoveAll(a => type.IsAssignableTo(a.GetType())) > 0;

            return success;
        }

        private bool TryCreateNewTypeList(Type type, out List<EntityWithAnything> list)
        {
            list = null;

            if (byType.ContainsKey(type))
                return false;

            list = new List<EntityWithAnything>();

            foreach (var comp in allComponents)
                if (comp.Component.GetType().IsAssignableTo(type))
                    list.Add(comp);

            byType.Add(type, list);

            return true;
        }

        /// <summary>
        /// Entity and object tuple
        /// </summary>
        public class EntityWithAnything
        {
            /// <summary>
            /// Related component
            /// </summary>
            public object Component;
            /// <summary>
            /// Entity that the component is attached to
            /// </summary>
            public Entity Entity;

            public EntityWithAnything(object component, Entity entity)
            {
                Component = component;
                Entity = entity;
            }

            public static explicit operator EntityWithAnything((object, Entity) v)
            {
                return new EntityWithAnything
                (
                    v.Item1,
                    v.Item2
                );
            }
        }
    }
}
