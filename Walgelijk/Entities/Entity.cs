﻿using System;

namespace Walgelijk
{
    /// <summary>
    /// An entity. Does nothing, simply holds an identity. Implicitly an integer.
    /// </summary>
    public struct Entity
    {
        /// <summary>
        /// The identity of the entity
        /// </summary>
        public int Identity { get; set; }

        public override bool Equals(object obj)
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
            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }

        public static implicit operator int(Entity entity)
        {
            return entity.Identity;
        }

        public override string ToString()
        {
            return $"Entity {Identity}";
        }
    }
}