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

        private readonly Dictionary<int, Entity> entities = new Dictionary<int, Entity>();

        private readonly Dictionary<Entity, CollectionByType> components = new Dictionary<Entity, CollectionByType>();
        private readonly Dictionary<Type, System> systems = new Dictionary<Type, System>();

        /// <summary>
        /// Fired when an entity is created and registered
        /// </summary>
        public event Action<Entity> OnCreateEntity;

        /// <summary>
        /// Fired when a component is attached to an entity
        /// </summary>
        public event Action<Entity, object> OnAttachComponent;

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
            components.Add(newEntity, new CollectionByType());

            OnCreateEntity?.Invoke(newEntity);

            return newEntity;
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
            return components[entity].GetAll();
        }

        /// <summary>
        /// Get all components and entities of a certain type
        /// </summary>
        public IEnumerable<ComponentEntityTuple<T>> GetAllComponentsOfType<T>() where T : class
        {
            foreach (var pair in components)
            {
                var componentDictionary = pair.Value;
                var entity = pair.Key;

                if (componentDictionary.TryGet(out T component))
                    yield return new ComponentEntityTuple<T>(component, entity);
            }
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        public T GetComponentFrom<T>(Entity entity) where T : class
        {
            if (components[entity].TryGet<T>(out var component))
                return component;
            return default;
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        public bool TryGetComponentFrom<T>(Entity entity, out T component) where T : class
        {
            component = default;
            if (!components.TryGetValue(entity, out var collection)) return false;

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
            return components[entity].Has<T>();
        }

        /// <summary>
        /// Attach a component to an entity
        /// </summary>
        public void AttachComponent<T>(Entity entity, T component) where T : class
        {
            components[entity].TryAdd<T>(component);
            OnAttachComponent?.Invoke(entity, component);
        }

        /// <summary>
        /// Detach a component from an entity
        /// </summary>
        /// <returns>if the operation was successful</returns>
        public bool DetachComponent<T>(Entity entity)
        {
            return components[entity].Remove<T>();
        }

        /// <summary>
        /// Executes all systems. This is typically handled by the window implementation
        /// </summary>
        public void UpdateSystems()
        {
            foreach (var system in systems.Values)
                system.Update();
        }

        /// <summary>
        /// Renders all systems that implement rendering code. This is typically handled by the window implementation
        /// </summary>
        public void RenderSystems()
        {
            foreach (var system in systems.Values)
                system.Render();
        }

        //TODO voeg manier to om meerdere componenten te krijgen per keer
    }
}
