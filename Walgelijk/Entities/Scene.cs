using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;

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

        /// <summary>
        /// When set to true, safety checks will be done at runtime. This will degrade performance and should be turned off in release. <b>True by default</b>
        /// </summary>
        public bool DevelopmentMode { get; set; } = true;

        private readonly Dictionary<int, Entity> entities = new Dictionary<int, Entity>();
        private readonly Dictionary<Entity, CollectionByType> components = new Dictionary<Entity, CollectionByType>();
        private readonly Dictionary<Entity, CollectionByType> creationBuffer = new Dictionary<Entity, CollectionByType>();
        private readonly Dictionary<Type, System> systems = new Dictionary<Type, System>();

        private bool stepIsInLoop;

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
        public void AddSystem<T>(T system) where T : System
        {
            system.Scene = this;
            OnAddSystem?.Invoke(system);
            systems.Add(system.GetType(), system);
            system.Initialise();
        }

        /// <summary>
        /// Retrieve a system
        /// </summary>
        public T GetSystem<T>() where T : System
        {
            return (T)systems[typeof(T)];
        }

        /// <summary>
        /// Remove system from the list. Getting rid of any references to it is not handled, so the object might remain in memory.
        /// </summary>
        /// <returns>if the operation was successful</returns>
        public bool RemoveSystem<T>()
        {
            return systems.Remove(typeof(T));
        }

        /// <summary>
        /// Get all systems
        /// </summary>
        public IEnumerable<System> GetSystems()
        {
            foreach (var pair in systems)
                yield return pair.Value;
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
            InitialiseComponentCollection(newEntity);
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
            InitialiseComponentCollection(entity);
        }

        private void InitialiseComponentCollection(Entity newEntity)
        {
            if (stepIsInLoop)
                creationBuffer.Add(newEntity, new CollectionByType());
            else
                components.Add(newEntity, new CollectionByType());
        }

        /// <summary>
        /// Get the entity struct from an entity ID. Generally not necessary
        /// </summary>
        public Entity GetEntity(int identity)
        {
            return entities[identity];
        }

        /// <summary>
        /// Removes the entity from the list. Also removes all attached components. Any references to the entity will become useless as they will point to nothing. References to any attached components are not handled, so they may remain in memory.
        /// </summary>
        /// <param name="identity"></param>
        /// <returns>if the operation was successful</returns>
        public bool RemoveEntity(int identity)
        {
            bool entityRemovalSuccess = entities.Remove(identity);
            if (!entityRemovalSuccess)
                return false;

            components[identity].Dispose();
            components.Remove(identity);
            return true;
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
        /// Get all components attached to the given entity
        /// </summary>
        public IEnumerable<object> GetAllComponentsFrom(Entity entity)
        {
            if (stepIsInLoop && creationBuffer.TryGetValue(entity, out var c))
                return c.GetAll();
            
            return components[entity].GetAll();
        }

        /// <summary>
        /// Get all components and entities of a certain type
        /// </summary>
        public IEnumerable<EntityWith<T>> GetAllComponentsOfType<T>() where T : class
        {
            stepIsInLoop = true;
            foreach (var pair in components)
            {
                var componentDictionary = pair.Value;
                var entity = pair.Key;

                if (componentDictionary.TryGetAll(out IEnumerable<T> set))
                {
                    foreach (var component in set)
                        yield return new EntityWith<T>(component, entity);
                }
            }
            stepIsInLoop = false;
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        public T GetComponentFrom<T>(Entity entity) where T : class
        {
            if (components[entity].TryGet<T>(out var component) || (creationBuffer.TryGetValue(entity, out var collection) && collection.TryGet<T>(out component)))
                return component;
            return default;
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        public bool TryGetComponentFrom<T>(Entity entity, out T component) where T : class
        {
            component = default;
            if (!components.TryGetValue(entity, out var collection) && !creationBuffer.TryGetValue(entity, out collection)) return false;

            if (collection.TryGet(out T c))
            {
                component = c;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get if an entity has a component
        /// </summary>
        public bool HasComponent<T>(Entity entity) where T : struct
        {
            return components[entity].Has<T>() || (creationBuffer.TryGetValue(entity, out var value) && value.Has<T>());
        }

        /// <summary>
        /// Attach a component to an entity
        /// </summary>
        public void AttachComponent<T>(Entity entity, T component) where T : class
        {
            if (DevelopmentMode)
                AssertComponentRequirements(entity, component);

            if (creationBuffer.TryGetValue(entity, out var value))
                value.TryAdd<T>(component);
            else
                components[entity].TryAdd<T>(component);

            OnAttachComponent?.Invoke(entity, component);
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
        /// <returns>if the operation was successful</returns>
        public bool DetachComponent<T>(Entity entity)
        {
            bool success;

            if (creationBuffer.TryGetValue(entity, out var value))
                success = value.Remove<T>();
            else
                success = components[entity].Remove<T>();

            if (success)
                OnDetachComponent?.Invoke(entity);

            return success;
        }

        /// <summary>
        /// Executes all systems. This is typically handled by the window implementation
        /// </summary>
        public void UpdateSystems()
        {
            foreach (var component in creationBuffer)
                components.Add(component.Key, component.Value);

            creationBuffer.Clear();

            stepIsInLoop = true;
            foreach (var system in systems.Values)
                system.Update();
            stepIsInLoop = false;
        }

        /// <summary>
        /// Renders all systems that implement rendering code. This is typically handled by the window implementation
        /// </summary>
        public void RenderSystems()
        {
            var s = systems.Values;

            foreach (var system in s)
                system.PreRender();

            foreach (var system in s)
                system.Render();

            foreach (var system in s)
                system.PostRender();
        }

        //TODO voeg manier to om meerdere componenten te krijgen per keer
    }
}
