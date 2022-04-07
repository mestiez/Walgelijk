using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
        private readonly Dictionary<Entity, Dictionary<Type, object>> byEntityByType = new();

        /// <summary>
        /// Manages the disposal of components
        /// </summary>
        public readonly ComponentDisposalManager DisposalManager = new();

        /// <summary>
        /// Add a component to the collection
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(object component, Entity entity)
        {
            var a = new EntityWithAnything(component, entity);
            var componentType = component.GetType();
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

            if (byEntityByType.TryGetValue(entity, out var dict))
                dict.Add(componentType, component);
            else
            {
                byEntityByType.Add(entity, new Dictionary<Type, object>() {
                    {componentType,  component}
                });
            }

            foreach (var item in byType)
            {
                var target = item.Key;

                if (componentType.IsAssignableTo(target))
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

            if (list != null)
                foreach (var item in list)
                    if (item.Component is T typed)
                        yield return new EntityWith<T>(typed, item.Entity);
        }

        /// <summary>
        /// Get the component of the <b>exact</b> type that is given
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetComponentFrom<T>(Entity entity) where T : class
        {
            return byEntityByType[entity][typeof(T)] as T ?? throw new Exception("Component is not of the expected type");
        }

        /// <summary>
        /// Try to get the component of the <b>exact</b> type that is given
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponentFrom<T>(Entity entity, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? component) where T : class
        {
            if (byEntityByType.TryGetValue(entity, out var dict) && dict.TryGetValue(typeof(T), out var untyped) && untyped is T typed)
            {
                component = typed;
                return true;
            }
            component = null;
            return false;
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
            bool c = true;

            c &= allComponents.RemoveWhere(t => t.Entity == entity) > 0;

            foreach (var listPair in byType)
            {
                var l = listPair.Value;
                for (int i = l.Count - 1; i >= 0; i--)
                    if (l[i].Entity == entity)
                    {
                        DisposalManager.DisposeOf(l[i].Component);
                        l.RemoveAt(i);
                    }
            }

            c &= byEntityByType.Remove(entity);
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

            foreach (var item in allComponents)
                if (shouldRemove(item))
                    DisposalManager.DisposeOf(item.Component);

            success &= allComponents.RemoveWhere(shouldRemove) > 0;

            if (byType.ContainsKey(type))
            {
                if (byType.TryGetValue(type, out var allComponentsOfThisType))
                {
                    success &= allComponentsOfThisType.RemoveAll(a => a.Entity == entity) > 0;
                    if (!allComponentsOfThisType.Any())
                        byType.Remove(type);
                }
                else success = false;
            }

            if (byEntity.TryGetValue(entity, out var allComponentsOnThisEntity))
            {
                success &= allComponentsOnThisEntity.RemoveAll(a => type.IsAssignableTo(a.GetType())) > 0;
                if (!allComponentsOnThisEntity.Any())
                    byEntity.Remove(entity);
            }
            else success = false;

            if (byEntityByType.TryGetValue(entity, out var componentsByType))
            {
                success &= componentsByType.Remove(type);
                if (!componentsByType.Any())
                    byEntityByType.Remove(entity);
            }
            else success = false;

            if (!success)
                throw new Exception("FAILED TO REMOVE");

            return success;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool shouldRemove(EntityWithAnything t) =>
                type.IsInstanceOfType(t.Component) && t.Entity == entity;
        }

        private bool TryCreateNewTypeList(Type type, out List<EntityWithAnything>? list)
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
