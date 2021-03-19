using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Add a component to the collection
        /// </summary>
        public void Add(object component, Entity entity)
        {
            var a = new EntityWithAnything(component, entity);
            var t = component.GetType();
            allComponents.Add(a);

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
        public IEnumerable GetAllComponentsFrom(Entity entity)
        {
            foreach (var a in allComponents)
                if (a.Entity == entity)
                    yield return a.Component;
        }

        /// <summary>
        /// Get the entity that the given component is attached to
        /// </summary>
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

            return c;
        }

        /// <summary>
        /// Detach a component of a type from an entity
        /// </summary>
        /// <returns>True if anything was removed</returns>
        public bool RemoveComponentOfType<T>(Entity entity)
        {
            TryCreateNewTypeList(typeof(T), out _);

            allComponents.RemoveWhere(t => t.Component is T && t.Entity == entity);

            if (!byType.TryGetValue(typeof(T), out var list))
                return false;

            return list.RemoveAll(a => a.Entity == entity) > 0;
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
