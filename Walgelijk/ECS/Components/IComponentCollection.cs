using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk;

/// <summary>
/// Represents a thread safe collection of components and their entities
/// </summary>
public interface IComponentCollection : IDisposable
{
    /// <summary>
    /// Attach component to entity
    /// </summary>
    T Attach<T>(Entity entity, T component) where T : Component;
    /// <summary>
    /// Detach component from entity and dispose
    /// </summary>
    bool Remove<T>(Entity entity) where T : Component;
    /// <summary>
    /// Detach component from entity and dispose
    /// </summary>
    bool Remove(Type type, Entity entity);

    /// <summary>
    /// Remove all components from the entity 
    /// </summary>
    bool Remove(Entity entity);

    /// <summary>
    /// Get all components attached to an entity
    /// </summary>
    IEnumerable<Component> GetAllFrom(Entity entity);

    /// <summary>
    /// Get all components attached to an entity and put it in the given span. 
    /// Returns actual amount that was put in.
    /// </summary>
    int GetAllFrom(Entity entity, Span<Component> span);

    /// <summary>
    /// Get all components attached to an entity and put it int the given array at the given position. 
    /// Returns actual amount that was put in.
    /// </summary>
    int GetAllFrom(Entity entity, Component[] arr, int offset, int count);

    /// <summary>
    /// Get a component from an entity
    /// </summary>
    T GetFrom<T>(Entity entity) where T : Component;

    /// <summary>
    /// Try to get a component from an entity. 
    /// Returns true if the retrieval was succesful
    /// </summary>
    bool TryGetFrom<T>(Entity entity, [NotNullWhen(true)] out T? component) where T : Component;

    /// <summary>
    /// Get all components by type
    /// </summary>
    IEnumerable<T> GetAllOfType<T>() where T : Component;
    /// <summary>
    /// Get all components by type and put it in the given span. 
    /// Returns actual amount that was put in.
    /// </summary>
    int GetAllOfType<T>(Span<T> span) where T : Component;
    /// <summary>
    /// Get all components by type and put it int the given array at the given position. 
    /// Returns actual amount that was put in.
    /// </summary>
    int GetAllOfType<T>(T[] arr, int offset, int count) where T : Component;

    /// <summary>
    /// Get all components
    /// </summary>
    IEnumerable<Component> GetAll();
    /// <summary>
    /// Get all components and put it in the given span
    /// Returns actual amount that was put in.
    /// </summary>
    int GetAll(Span<Component> span);
    /// <summary>
    /// Get all components and put it in the given array at the given position
    /// Returns actual amount that was put in.
    /// </summary>
    int GetAll(Component[] span, int offset, int count);

    /// <summary>
    /// Returns true if the entity has a component of the given type
    /// </summary>
    bool Has<T>(Entity entity) where T : Component;

    /// <summary>
    /// Returns true if the entity was found in the cache
    /// </summary>
    bool HasEntity(Entity entity);

    /// <summary>
    /// Returns true if there exists a component of this type
    /// </summary>
    bool Contains<T>() where T : Component;

    /// <summary>
    /// Called when a frame has ended. 
    /// The collection will empty its add and destroy buffers and update the main component collection.
    /// </summary>
    public void SyncBuffers();

    /// <summary>
    /// Amount of components in total
    /// </summary>
    public int Count { get; }
}
