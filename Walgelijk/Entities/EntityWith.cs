using System;
using System.Collections.Generic;

namespace Walgelijk
{
    /// <summary>
    /// Struct that holds a component and its entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct EntityWith<T> : IEquatable<EntityWith<T>> where T : class
    {
        public EntityWith(T component, Entity entity)
        {
            Component = component;
            Entity = entity;
        }

        /// <summary>
        /// The component of type <typeparamref name="T"/>
        /// </summary>
        public T Component { get; set; }
        /// <summary>
        /// The entity that <see cref="Component"/> is attached to
        /// </summary>
        public Entity Entity { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is EntityWith<T> with && Equals(with);
        }

        public bool Equals(EntityWith<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Component, other.Component) &&
                   Entity.Equals(other.Entity);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Component, Entity);
        }

        public static bool operator ==(EntityWith<T> left, EntityWith<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EntityWith<T> left, EntityWith<T> right)
        {
            return !(left == right);
        }

        public static implicit operator EntityWithAnything(EntityWith<T> v)
        {
            return new EntityWithAnything
            (
                v.Component,
                v.Entity
            );
        }
    }
}
