using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Walgelijk
{
    /// <summary>
    /// Stores and manages components and systems
    /// </summary>
    public sealed class Scene : IDisposable
    {
        /// <summary>
        /// Game this scene belongs to
        /// </summary>
        public Game Game { get; internal set; }

        /// <summary>
        /// Should the game dispose of this scene when it is made inactive by setting <see cref="Game.Scene"/> to something else?
        /// </summary>
        public bool ShouldBeDisposedOnSceneChange = false;

        /// <summary>
        /// Manages the disposal of components
        /// </summary>
        public ComponentDisposalManager ComponentDisposal => components.DisposalManager;

        private readonly Dictionary<int, Entity> entities = new();
        private readonly IComponentCollection components = new BasicComponentCollection();
        private readonly Dictionary<Type, System> systems = new();
        private readonly List<System> orderedSystemCollection = new();

        private readonly Dictionary<Entity, Tag> tagByEntity = new();
        private readonly Dictionary<Tag, HashSet<Entity>> entitiesByTag = new();

        private readonly HashSet<EntityWithAnything> componentsToAdd = new();
        private readonly HashSet<(Type, Entity)> componentsToDestroy = new();
        private readonly HashSet<Entity> entitiesToDestroy = new();

        private readonly HashSet<System> systemsToAdd = new();
        private readonly HashSet<System> systemsToDestroy = new();

        internal bool HasBeenLoadedAlready = false;

        /// <summary>
        /// Create scene for a <see cref="Game"/> without setting it as the active scene
        /// </summary>
        public Scene(Game game)
        {
            Game = game;
            ComponentDisposal.RegisterDisposer(new ShapeRendererDisposer());
        }

        /// <summary>
        /// Create scene without an attached game. This can cause issues when a <see cref="System"/> expects a game
        /// </summary>
        public Scene() { }

        /// <summary>
        /// Fired when an entity is created and registered
        /// </summary>
        public event Action<Entity>? OnCreateEntity;

        /// <summary>
        /// Fired when a component is attached to an entity
        /// </summary>
        public event Action<Entity, object>? OnAttachComponent;

        /// <summary>
        /// Fired when a component is detached from an entity
        /// </summary>
        public event Action<Entity>? OnDetachComponent;

        /// <summary>
        /// Fired when a system is added
        /// </summary>
        public event Action<System>? OnAddSystem;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetSystem<T>() where T : System
        {
            return (T)systems[typeof(T)];
        }

        /// <summary>
        /// Attaches a tag to an entity
        /// </summary>
        public void SetTag(Entity entity, Tag tag)
        {
            if (tagByEntity.ContainsKey(entity))//does this entity already have a tag? 
            {
                if (entitiesByTag.TryGetValue(tag, out var existing))
                    existing.Remove(entity);//remove entity from old tag collection

                tagByEntity[entity] = tag;
            }
            else
            {
                //this entity never had a tag so we need to create everything
                tagByEntity.Add(entity, tag);

                if (entitiesByTag.TryGetValue(tag, out var coll))//does the tag already have a collection?
                    coll.Add(entity);
                else
                    entitiesByTag.Add(tag, new HashSet<Entity> { entity });
            }
        }

        /// <summary>
        /// Returns true if the entity has the given tag
        /// </summary>
        public bool HasTag(Entity entity, Tag tag) => tagByEntity.TryGetValue(entity, out var t) && t == tag;

        /// <summary>
        /// Detaches a tag from an entity
        /// </summary>
        public bool ClearTag(Entity entity)
        {
            if (tagByEntity.Remove(entity, out var tag))
            {
                var coll = entitiesByTag[tag];
                bool s = coll.Remove(entity);
                if (coll.Count == 0)
                    s &= entitiesByTag.Remove(tag);
                return s;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given entity has a tag attached. The tag is returned in the output parameter <paramref name="tag"/>
        /// </summary>
        public bool TryGetTag(Entity entity, out Tag tag) => tagByEntity.TryGetValue(entity, out tag);

        /// <summary>
        /// Returns all entities with the given tag
        /// </summary>
        public IEnumerable<Entity> GetEntitiesWithTag(Tag tag)
        {
            if (entitiesByTag.TryGetValue(tag, out var coll))
                foreach (var entity in coll)
                    yield return entity;
            yield break;
        }

        /// <summary>
        /// Returns true if any entity with a given tag is found. The entity is returned in the output parameter <paramref name="entity"/>
        /// </summary>
        public bool TryGetEntityWithTag(Tag tag, out Entity entity)
        {
            if (entitiesByTag.TryGetValue(tag, out var coll) && coll.Count > 0)
            {
                entity = coll.First();
                return true;
            }
            entity = default;
            return false;
        }

        /// <summary>
        /// Returns true if the system of the given type exists in the scene and returns false otherwise.
        /// </summary>
        public bool HasSystem<T>() => systems.ContainsKey(typeof(T)) || systemsToAdd.Any(t => t is T);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntity(int identity)
        {
            entitiesToDestroy.Add(identity);
        }

        /// <summary>
        /// Get if an entity lives in the scene
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindAnyComponent<T>([global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? anyInstance) where T : class => FindAnyComponent(out anyInstance, out _);

        /// <summary>
        /// Returns the first found instance of the given type.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindAnyComponent<T>([global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? anyInstance, out Entity entity) where T : class
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T? GetComponentFrom<T>(Entity entity) where T : class
        {
            if (components.TryGetComponentFrom<T>(entity, out var attemptResult))
                return attemptResult;

            foreach (var item in componentsToAdd)
                if (item.Entity == entity && item.Component is T typed)
                    return typed;

            foreach (var item in components.GetAllComponentsFrom(entity))
                if (item is T a)
                    return a;

            foreach (var item in components.GetAllComponentsOfType<T>())
                if (item.Entity == entity)
                    return item.Component;

            return default;
        }

        /// <summary>
        /// Gets the component of the <b>exact</b> given type from the given entity. Use this is you are absolutely sure that the entity has the component you're asking for and that it refers to a component of that exact type. Inheritance is supported here but it's discouraged as it can lead to ambiguity.
        /// </summary>
        public T GetComponentFast<T>(Entity entity) where T : class
        {
            //This has to happen :(
            foreach (var item in componentsToAdd)
                if (item.Entity == entity && item.Component is T typed)
                    return typed;

            return components.GetComponentFrom<T>(entity);
        }

        /// <summary>
        /// Get the <see cref="Entity"/> connected to the given component
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEntityWith(object component, out Entity entity)
        {
            return components.GetEntityFromComponent(component, out entity);
        }

        /// <summary>
        /// Retrieve the first component of the specified type on the given entity
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetComponentFrom<T>(Entity entity, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? component) where T : class
        {
            component = null;

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
        /// Get if an entity has a component. This is a bit slow so you should really just have the entity at hand
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T AttachComponent<T>(Entity entity, T component) where T : class
        {
            componentsToAdd.Add(new EntityWithAnything(component, entity));

            return component;
        }

        private void AssertComponentRequirements<T>(Entity entity, T component) where T : class
        {
            RequiresComponents[] requirements = ReflectionCache.GetAttributes<RequiresComponents, T>();
            if (requirements.Length == 0) return;
            IEnumerable<object> existing = GetAllComponentsFrom(entity);

            foreach (var requirement in requirements)
                if (requirement.Types != null)
                    foreach (var type in requirement.Types)
                        if (!(existing.Any(e => type.IsAssignableFrom(e.GetType()))))
                            throw new InvalidOperationException($"{component.GetType()} requires a {type}");
        }

        /// <summary>
        /// Detach a component from an entity
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DetachComponent<T>(Entity entity)
        {
            componentsToDestroy.Add((typeof(T), entity));
        }

        /// <summary>
        /// Executes all systems. This is typically handled by the window implementation
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateSystems()
        {
            if (!HasBeenLoadedAlready)
                Initialise();

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

        /// <summary>
        /// Prepares the scene for immediate activity. This is handled by the engine so there is no need to call this unless you know why you're calling it.
        /// </summary>
        public void Initialise()
        {
            if (Game != null)
                Game.Time.DeltaTime = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                {
                    ClearTag(identity);
                    components.DeleteEntity(identity);
                }
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

        /// <summary>
        /// Unload all entities and components in the scene. You are responsible for registering custom <see cref="IComponentDisposer"/>s for your components.
        /// </summary>
        public void Dispose()
        {
            foreach (var item in entities)
                RemoveEntity(item.Value);

            ProcessAddDestroyBuffers();
            entities.Clear();
        }

        //TODO voeg manier to om meerdere componenten te krijgen per keer
    }
}
