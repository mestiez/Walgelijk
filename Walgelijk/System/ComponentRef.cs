using System;
using System.Diagnostics.CodeAnalysis;

namespace Walgelijk;

/// <summary>
/// Represents a reference to a component of type T that is attached to an entity.
/// The point of this structure is that it doesn't actually store the component, only an entity and a type parameter.
/// This ensures that the ownership of the component remains with the scene and prevents memory leaks and "rogue" components.
/// </summary>
/// <typeparam name="T">The type of component.</typeparam>
public readonly struct ComponentRef<T> : IEquatable<ComponentRef<T>> where T : Component
{
    /// <summary>
    /// The entity to which the component is attached.
    /// </summary>
    public readonly Entity Entity;

    /// <summary>
    /// Construct a component reference
    /// </summary>
    /// <param name="entity"></param>
    public ComponentRef(Entity entity)
    {
        Entity = entity;
    }

    /// <summary>
    /// Gets the component of type T attached to the entity from the specified scene.
    /// </summary>
    /// <param name="scene">The scene to search for the component.</param>
    public readonly T Get(Scene scene)
        => scene.GetComponentFrom<T>(Entity);

    /// <summary>
    /// Gets the component of type T attached to the entity from the specified scene.
    /// </summary>
    /// <param name="scene">The scene to search for the component.</param>
    /// <param name="component">The component. Null if not found.</param>
    public readonly bool TryGet(Scene scene, [NotNullWhen(true)] out T? component)
        => scene.TryGetComponentFrom(Entity, out component);

    /// <summary>
    /// Checks whether the reference is valid in the specified scene.
    /// </summary>
    /// <param name="scene">The scene to check for the entity and component.</param>
    /// <returns>True if the entity exists in the scene and has the component of type T; otherwise, false.</returns>
    public readonly bool IsValid(Scene scene)
        => scene.HasEntity(Entity) && scene.HasComponent<T>(Entity);

    public override bool Equals(object? obj)
    {
        return obj is ComponentRef<T> @ref && Equals(@ref);
    }

    public bool Equals(ComponentRef<T> other)
    {
        return Entity.Equals(other.Entity);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Entity);
    }

    public static bool operator ==(ComponentRef<T> left, ComponentRef<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ComponentRef<T> left, ComponentRef<T> right)
    {
        return !(left == right);
    }

    public static implicit operator ComponentRef<T>(T value)
    {
        return new ComponentRef<T>(value?.Entity ?? Entity.None);
    }
}