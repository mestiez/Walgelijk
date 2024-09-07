using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Walgelijk;

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
    /// Globally unique ID for this scene
    /// </summary>
    public readonly SceneId Id;

    /// <summary>
    /// Determines behaviour when this scene is switched away by <see cref="Game"/>
    /// </summary>
    public ScenePersistence ScenePersistence = ScenePersistence.Dispose;

    private readonly IEntityCollection entities = new BasicEntityCollection();
    private readonly IComponentCollection components = new FilterComponentCollection();
    private readonly ISystemCollection systems;

    internal bool HasBeenLoadedAlready = false;

    /// <summary>
    /// Create scene for a <see cref="Game"/> without setting it as the active scene
    /// </summary>
    public Scene(Game game, SceneId id)
    {
        Game = game;
        Id = id;
        systems = new BasicSystemCollection(this);
    }

    /// <summary>
    /// Create scene without an attached game. This can cause issues when a <see cref="System"/> expects a game
    /// </summary>
    public Scene(SceneId id)
    {
        systems = new BasicSystemCollection(this);
        Id = id;
    }

    /// <summary>
    /// Create scene for a <see cref="Game"/> without setting it as the active scene
    /// </summary>
    public Scene(Game game)
    {
        Game = game;
        Id = Guid.NewGuid().ToString();
        systems = new BasicSystemCollection(this);
    }

    /// <summary>
    /// Create scene without an attached game. This can cause issues when a <see cref="System"/> expects a game
    /// </summary>
    public Scene()
    {
        systems = new BasicSystemCollection(this);
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Fired when this scene is made active
    /// </summary>
    public event Action? OnActive;

    /// <summary>
    /// Fired when this scene is made inactive
    /// </summary>
    public event Action? OnInactive;

    /// <summary>
    /// Fired when an entity is created and registered
    /// </summary>
    public event Action<Entity>? OnCreateEntity;

    /// <summary>
    /// Fired when an entity is removed
    /// </summary>
    public event Action<Entity>? OnRemovedEntity;

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
    public T AddSystem<T>(T system) where T : System => systems.Add(system);

    /// <summary>
    /// Retrieve a system
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetSystem<T>() where T : System => systems.Get<T>();

    /// <summary>
    /// Retrieve a system
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public System GetSystem(Type type) => systems.Get(type);

    /// <summary>
    /// Try to retrieve a system
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetSystem<T>([NotNullWhen(true)] out T? system) where T : System
        => systems.TryGet(out system);

    /// <summary>
    /// Try to retrieve a system
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetSystem(Type type, [NotNullWhen(true)] out System? system)
        => systems.TryGet(type, out system);

    /// <summary>
    /// Returns true if the system of the given type exists in the scene and returns false otherwise.
    /// </summary>
    public bool HasSystem<T>() where T : System => systems.Has<T>();

    /// <summary>
    /// Returns true if the system of the given type exists in the scene and returns false otherwise.
    /// </summary>
    public bool HasSystem(Type type) => systems.Has(type);

    /// <summary>
    /// Remove system from the list. Getting rid of any references to it is not handled, so the object might remain in memory.
    /// </summary>
    public bool RemoveSystem<T>() where T : System => systems.Remove<T>();

    /// <summary>
    /// Get all systems
    /// </summary>
    public IEnumerable<System> GetSystems() => systems;

    /// <summary>
    /// Register a new entity to the scene
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity CreateEntity()
    {
        Entity newEntity = new(IdentityGenerator.Generate());
        entities.Add(newEntity);
        OnCreateEntity?.Invoke(newEntity);

        return newEntity;
    }

    /// <summary>
    /// Add an entity with a specific identity. Note that this might cause a collision
    /// </summary>
    /// <param name="entity"></param>
    public void RegisterExistingEntity(Entity entity)
    {
        entities.Add(entity);
        OnCreateEntity?.Invoke(entity);
    }

    /// <summary>
    /// Removes the entity from the list. Also removes all attached components. Any references to the entity will become useless as they will point to nothing. References to any attached components are not handled, so they may remain in memory.
    /// </summary>
    /// <param name="identity"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveEntity(Entity identity)
    {
        OnRemovedEntity?.Invoke(identity);
        entities.Remove(identity);
        components.Remove(identity);
    }

    /// <summary>
    /// Get if an entity lives in the scene
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasEntity(Entity identity) => entities.Has(identity);

    /// <summary>
    /// Get all entities
    /// </summary>
    public IEnumerable<Entity> GetAllEntities() => entities;

    /// <summary>
    /// Amount of entities in the scene
    /// </summary>
    public int EntityCount => entities.Count;

    /// <summary>
    /// Returns true if this scene has been disposed and should no longer be used
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Get all components attached to the given entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<Component> GetAllComponentsFrom(Entity entity) => components.GetAllFrom(entity);

    /// <summary>
    /// Get all components and entities of a certain type
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> GetAllComponentsOfType<T>() where T : Component => components.GetAllOfType<T>();

    /// <summary>
    /// Get all components and entities of a certain type and put them into the given buffer, returning the relevant section of it
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> GetAllComponentsOfType<T>(T[] array) where T : Component
    {
        int i = components.GetAllOfType<T>(array);
        return array.AsSpan(0, i);
    }

    /// <summary>
    /// Get all components and entities and add them into the given array. It returns the amount of items that were inserted.
    /// </summary>
    public int CopyAllComponentsOfType<T>(T[] array) where T : Component => components.GetAllOfType<T>(array);

    /// <summary>
    /// Returns the first found instance of the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool FindAnyComponent<T>([NotNullWhen(true)] out T? anyInstance) where T : Component
    {
        foreach (var item in components.GetAllOfType<T>())
        {
            anyInstance = item;
            return true;
        }
        anyInstance = null;
        return false;
    }

    /// <summary>
    /// Retrieve the first component of the specified type on the given entity, otherwise returns default
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetComponentFrom<T>(Entity entity) where T : Component => components.GetFrom<T>(entity);

    /// <summary>
    /// Retrieve the first component of the specified type on the given entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetComponentFrom<T>(Entity entity, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out T? component) where T : Component
        => components.TryGetFrom(entity, out component);

    /// <summary>
    /// Get if an entity has a component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>(Entity entity) where T : Component
    => components.Has<T>(entity);

    /// <summary>
    /// Attach a component to an entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T AttachComponent<T>(Entity entity, T component) where T : Component
    {
        return components.Attach(entity, component);
    }

    /// <summary>
    /// Detach a component from an entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachComponent<T>(Entity entity) where T : Component
    {
        components.Remove<T>(entity);
    }

    /// <summary>
    /// Detach a component from an entity and sync the buffers immediately.
    /// So if you call <see cref="DetachComponentImmediate{T}(Entity)"/> and then
    /// <see cref="HasComponent{T}(Entity)"/> in the same frame/tick, 
    /// <see cref="HasComponent{T}(Entity)"/> will return the correct value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DetachComponentImmediate<T>(Entity entity) where T : Component
    {
        DetachComponent<T>(entity);
        SyncBuffers();
    }

    public bool HasTag(Entity entity, Tag tag) => entities.HasTag(entity, tag);
    public bool ClearTag(Entity entity) => entities.ClearTag(entity);
    public void SetTag(Entity entity, Tag tag) => entities.SetTag(entity, tag);
    public bool TryGetEntityWithTag(Tag tag, out Entity entity) => entities.TryGetEntityWithTag(tag, out entity);
    public bool TryGetTag(Entity entity, out Tag tag) => entities.TryGetTag(entity, out tag);
    public IEnumerable<Entity> GetEntitiesWithTag(Tag tag) => entities.GetEntitiesWithTag(tag);

    /// <summary>
    /// Executes all systems
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateSystems()
    {
        if (!HasBeenLoadedAlready)
            Initialise();

        bool shouldSort = false;
        foreach (var item in systems.GetAll())
            if (item.ExecutionOrderChanged)
            {
                shouldSort = true;
                break;
            }
        if (shouldSort)
            systems.Sort(); // TODO double sorting. die onderste systems.SyncBuffers sorteert ook maar je moet dus kunnen aangeven om dat niet te doen

        SyncBuffers();
        systems.InitialiseNewSystems();

        foreach (var system in systems.GetAll())
            if (system?.Enabled ?? false)
                system.Update();
    }

    /// <summary>
    /// When adding or removing components or systems, they won't be returned by querying methods until the following frame. You can call this method to force update the buffers after adding/removing stuff
    /// if you really need to query them immediately.
    /// </summary>
    public void SyncBuffers()
    {
        entities.SyncBuffers();
        components.SyncBuffers();
        //systems.SyncBuffers();
    }

    /// <summary>
    /// Executes all systems
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FixedUpdateSystems()
    {
        if (!HasBeenLoadedAlready)
            Initialise();

        foreach (var system in systems.GetAll())
            if (system.Enabled)
                system.FixedUpdate();
    }

    /// <summary>
    /// Prepares the scene for immediate activity. This is handled by the engine so there is no need to call this unless you know why you're calling it.
    /// </summary>
    public void Initialise()
    {
        if (Game != null)
            Game.State.Time.DeltaTime = 0;
    }

    internal void Activate()
    {
        OnActive?.Invoke();
        foreach (var s in systems)
            if (s.Enabled)
                s.OnActivate();
    }

    internal void Deactivate()
    {
        OnInactive?.Invoke();
        foreach (var s in systems)
            if (s.Enabled)
                s.OnDeactivate();
    }

    /// <summary>
    /// Renders all systems that implement rendering code. This is typically handled by the window implementation
    /// </summary>
    public void RenderSystems()
    {
        var systems = this.systems.GetAll();

        foreach (var system in systems)
            if (system.Enabled)
                system.PreRender();

        foreach (var system in systems)
            if (system.Enabled)
                system.Render();

        foreach (var system in systems)
            if (system.Enabled)
                system.PostRender();
    }

    /// <summary>
    /// Unload all entities and components in the scene.
    /// </summary>
    public void Dispose()
    {
        foreach (var item in entities)
            RemoveEntity(item);

        entities.SyncBuffers();
        components.SyncBuffers();

        systems.Dispose();
        components.Dispose();

        Disposed = true;
        Game.SceneCache.Remove(Id); // if you move this line one line up, you will probably cause a stack overflow 
    }

    //TODO voeg manier to om meerdere componenten te krijgen per keer
}
