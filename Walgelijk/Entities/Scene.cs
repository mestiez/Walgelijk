using System;
using System.Collections.Generic;
using System.Linq;

namespace Walgelijk
{

    /// <summary>
    /// Stores and manages components and systems
    /// </summary>
    public sealed class Scene
    {
        /// <summary>
        /// Game this scene belongs to
        /// </summary>
        public Game Game { get; internal set; }

        private readonly Dictionary<int, Entity> entities = new();
        private readonly ComponentCollection components = new();
        private readonly Dictionary<Type, System> systems = new();
        private readonly List<System> orderedSystemCollection = new();

        private HashSet<ComponentCollection.EntityWithAnything> componentsToAdd = new();
        private HashSet<(Type, Entity)> componentsToDestroy = new();
        private HashSet<Entity> entitiesToDestroy = new();

        private HashSet<System> systemsToAdd = new();
        private HashSet<System> systemsToDestroy = new();

        /// <summary>
        /// Create scene for a <see cref="Game"/> without setting it as the active scene
        /// </summary>
        public Scene(Game game)
        {
            Game = game;
        }

        /// <summary>
        /// Create scene without an attached game. This can cause issues when a <see cref="System"/> expects a game
        /// </summary>
        public Scene()
        {

        }

        /// <summary>
        /// Fired when an entity is created and registered
        /// </summary>
        public event Action<Entity> OnCreateEntity;

        /// <summary>
        /// Fired when a component is attached to an entity
        /// </summary>
        public event Action<Entity, object> OnAttachComponent;

        /// <summary>
        /// Fired when a component is detacged from an entity
        /// </summary>
        public event Action<Entity> OnDetachComponent;

        /// <summary>
        /// Fired when a system is added
        /// </summary>
        public event Action<System> OnAddSystem;

        /// <summary>
        /// Add a system
        /// </summary>
        public T AddSystem<T>(T system) where T : System
        {
            system.Scene = this;
            systemsToAdd.Add(system);
            return system;
        }

        private void ReverseSortAddSystem(System system)
        {
            for (int i = orderedSystemCollection.Count - 1; i >= 0; i--)
                if (orderedSystemCollection[i].ExecutionOrder <= system.ExecutionOrder)
                {
                    orderedSystemCollection.Insert(i + 1, system);
                    return;
                }
            orderedSystemCollection.Insert(0, system);
        }

        private void ForceSortSystems()
        {
            while (true)
            {
                bool moved = false;
                for (int i = 0; i < orderedSystemCollection.Count - 1; i++)
                {
                    var current = orderedSystemCollection[i];
                    var next = orderedSystemCollection[i + 1];

                    current.ExecutionOrderChanged = false;
                    next.ExecutionOrderChanged = false;

                    if (current.ExecutionOrder > next.ExecutionOrder)
                    {
                        orderedSystemCollection.Remove(next);
                        orderedSystemCollection.Insert(i, next);
                        moved = true;
                    }
                }

                if (!moved)
                {
                    Logger.Log("System collection sorted");
                    break;
                }
            }
        }

        /// <summary>
        /// Retrieve a system
        /// </summary>
        public T GetSystem<T>() where T : System
        {
            return (T)systems[typeof(T)];
        }

        /// <summary>
        /// Returns true if the system of the given type exists in the scene and returns false otherwise.
        /// </summary>
        public bool HasSystem<T>() => systems.ContainsKey(typeof(T));

        /// <summary>
        /// Remove system from the list. Getting rid of any references to it is not handled, so the object might remain in memory.
        /// </summary>
        /// <returns>if the operation was successful</returns>
        public bool RemoveSystem<T>()
        {
            if (systems.TryGetValue(typeof(T), out var s))
            {
                systemsToDestroy.Add(s);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all systems
        /// </summary>
        public IEnumerable<System> GetSystems()
        {
            foreach (var s in orderedSystemCollection)
                yield return s;
        }

        /// <summary>
        /// Register a new entity to the scene
        /// </summary>
        public Entity CreateEntity()
        {
            Entity newEntity = new Entity
            {
                Identity = IdentityGenerator.Generate()
            };

            entities.Add(newEntity.Identity, newEntity);
            OnCreateEntity?.Invoke(newEntity);

            return newEntity;
        }

        /// <summary>
        /// Register an existing Entity ID to the scene. This should not be used if you are to create a new entity. Use <see cref="CreateEntity"/> instead. <see cref="OnCreateEntity"/> will not be invoked.
        /// </summary>
        public void RegisterExistingEntity(Entity entity)
        {
            if (HasEntity(entity))
                throw new InvalidOperationException($"Prefab ID ({entity}) already exists in the scene");

            entities.Add(entity.Identity, entity);
        }

        /// <summary>
        /// Removes the entity from the list. Also removes all attached components. Any references to the entity will become useless as they will point to nothing. References to any attached components are not handled, so they may remain in memory.
        /// </summary>
        /// <param name="identity"></param>
        public void RemoveEntity(int identity)
        {
            entitiesToDestroy.Add(identity);
        }

        /// <summary>
        /// Get if an entity lives in the scene
        /// </summary>
        public bool HasEntity(int identity)
        {
            return entities.ContainsKey(identity);
        }

        /// <summary>
        /// Get all entities
        /// </summary>
        public IEnumerable<Entity> GetAllEntities()
        {
            return entities.Values;
        }

        /// <summary>
        /// Amount of entities in the scene
        /// </summary>
        public int EntityCount => entities.Count;

        /// <summary>
        /// Get all components attached to the given entity
        /// </summary>
        public IEnumerable<object> GetAllComponentsFrom(Entity entity)
        {
            foreach (var item in componentsToAdd)
                if (item.Entity == entity)
                    yield return item.Component;

            foreach (var component in components.GetAllComponentsFrom(entity))
                yield return component;
        }

        /// <summary>
        /// Get all components and entities of a certain type
        /// </summary>
        public IEnumerable<EntityWith<T>> GetAllComponentsOfType<T>() where T : class
        {
            foreach (var item in componentsToAdd)
                if (item.Component is T typed)
                    yield return new EntityWith<T>(typed, item.Entity);

            foreach (var item in components.GetAllComponentsOfType<T>())
                yield return new EntityWith<T>(item.Component, item.Entity);
        }

        /// <summary>
        /// Get all components and entities and add them into the given array. It returns the amount of items that were inserted.
        /// </summary>
        public int CopyAllComponentsOfType<T>(EntityWith<T>[] array) where T : class
        {
            //TODO include creation buffer
            int i = 0;
            foreach (var item in components.GetAllComponentsOfType<T>())
            {
                if (i >= array.Length)
                    break;

                array[i] = item;
                i++;
            }
            return i;
        }

        /// <summary>
        /// Returns the first found instance of the given type.
        /// </summary>
        public bool FindAnyComponent<T>(out T anyInstance, out Entity entity) where T : class
        {
            foreach (var item in componentsToAdd)
                if (item.Component is T typed)
                {
                    anyInstance = typed;
                    entity = item.Entity;
                    return true;
                }

            foreach (var item in components.GetAllComponentsOfType<T>())
            {
                anyInstance = item.Component;
                entity = item.Entity;
                return true;
            }
            entity = default;
            anyInstance = null;
            return false;
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity, otherwise returns default
        /// </summary>
        public T GetComponentFrom<T>(Entity entity) where T : class
        {
            foreach (var item in componentsToAdd)
                if (item.Entity == entity && item.Component is T typed)
                    return typed;

            //TODO dit werkt niet omdat die camera er nog niet is. die zit in de creation buffer. dus ja hier ga je weer
            foreach (var item in components.GetAllComponentsOfType<T>())
                if (item.Entity == entity)
                    return item.Component;

            return default;
        }

        /// <summary>
        /// Get the <see cref="Entity"/> connected to the given component
        /// </summary>
        public bool TryGetEntityWith(object component, out Entity entity)
        {
            return components.GetEntityFromComponent(component, out entity);
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        public bool TryGetComponentFrom<T>(Entity entity, out T component) where T : class
        {
            component = default;

            foreach (var item in componentsToAdd)
                if (item.Entity == entity && item.Component is T typed)
                {
                    component = typed;
                    return true;
                }

            foreach (var item in components.GetAllComponentsFrom(entity))
                if (item is T typed)
                {
                    component = typed;
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Get if an entity has a component
        /// </summary>
        public bool HasComponent<T>(Entity entity) where T : class
        {
            foreach (var item in componentsToAdd)
                if (item.Entity == entity && item.Component is T typed)
                    return true;

            foreach (var a in components.GetAllComponentsFrom(entity))
                if (a is T)
                    return true;
            return false;
        }

        /// <summary>
        /// Attach a component to an entity
        /// </summary>
        public T AttachComponent<T>(Entity entity, T component) where T : class
        {
            componentsToAdd.Add(new ComponentCollection.EntityWithAnything(component, entity));

            return component;
        }

        private void AssertComponentRequirements<T>(Entity entity, T component) where T : class
        {
            RequiresComponents[] requirements = ReflectionCache.GetAttributes<RequiresComponents, T>();
            if (requirements.Length == 0) return;
            IEnumerable<object> existing = GetAllComponentsFrom(entity);

            foreach (var requirement in requirements)
                foreach (var type in requirement.Types)
                    if (!(existing.Any(e => type.IsAssignableFrom(e.GetType()))))
                        throw new InvalidOperationException($"{component.GetType()} requires a {type}");
        }

        /// <summary>
        /// Detach a component from an entity
        /// </summary>
        public void DetachComponent<T>(Entity entity)
        {
            componentsToDestroy.Add((typeof(T), entity));
        }

        /// <summary>
        /// Executes all systems. This is typically handled by the window implementation
        /// </summary>
        public void UpdateSystems()
        {
            ProcessAddDestroyBuffers();
            for (int i = 0; i < orderedSystemCollection.Count; i++)
                if (orderedSystemCollection[i].ExecutionOrderChanged)
                {
                    ForceSortSystems();
                    break;
                }

            foreach (var system in orderedSystemCollection)
                system.Update();
        }

        private void ProcessAddDestroyBuffers()
        {
            foreach (var system in systemsToAdd)
            {
                OnAddSystem?.Invoke(system);
                systems.Add(system.GetType(), system);
                ReverseSortAddSystem(system);
                system.ExecutionOrderChanged = false;
                system.Initialise();
            }
            systemsToAdd.Clear();

            foreach (var system in systemsToDestroy)
            {
                if (systems.Remove(system.GetType(), out var removed))
                    orderedSystemCollection.Remove(removed);
            }
            systemsToDestroy.Clear();

            foreach (var pair in componentsToAdd)
            {
                if (Game?.DevelopmentMode ?? false)
                    AssertComponentRequirements(pair.Entity, pair.Component);

                components.Add(pair.Component, pair.Entity);
                OnAttachComponent?.Invoke(pair.Entity, pair.Component);
            }
            componentsToAdd.Clear();

            foreach ((Type type, Entity entity) in componentsToDestroy)
            {
                bool success = components.RemoveComponentOfType(type, entity);

                if (success)
                    OnDetachComponent?.Invoke(entity);
            }
            componentsToDestroy.Clear();

            foreach (var identity in entitiesToDestroy)
            {
                bool entityRemovalSuccess = entities.Remove(identity);
                if (entityRemovalSuccess)
                    components.DeleteEntity(identity);
            }
            entitiesToDestroy.Clear();
        }

        /// <summary>
        /// Renders all systems that implement rendering code. This is typically handled by the window implementation
        /// </summary>
        public void RenderSystems()
        {
            foreach (var system in orderedSystemCollection)
                system.PreRender();

            foreach (var system in orderedSystemCollection)
                system.Render();

            foreach (var system in orderedSystemCollection)
                system.PostRender();
        }

        //TODO voeg manier to om meerdere componenten te krijgen per keer
    }
}
