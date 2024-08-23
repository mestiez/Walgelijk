using System;

namespace Walgelijk
{
    /// <summary>
    /// An entity. Does nothing, simply holds an identity. Implicitly an integer. Entity 0 is reserved for "non existent"
    /// </summary>
    public struct Entity : IEquatable<Entity>
    {
        /// <summary>
        /// The identity of the entity
        /// </summary>
        public int Identity;

        /// <summary>
        /// The "non existent" entity
        /// </summary>
        public static Entity None { get; } = 0;

        public Entity(int id)
        {
            Identity = id;
        }

        public override bool Equals(object? obj)
        {
            return obj is Entity entity &&
                   Identity == entity.Identity;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Identity);
        }

        public static bool operator ==(Entity left, Entity right)
        {
            return left.Identity == right.Identity;
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }

        public static implicit operator int(Entity entity)
        {
            return entity.Identity;
        }

        public static implicit operator Entity(int identity)
        {
            return new Entity { Identity = identity };
        }

        public override string ToString() => Identity == 0 ? "No entity" : $"Entity {Identity}";

        public bool Equals(Entity other) => other.Identity == Identity;
    }
}
