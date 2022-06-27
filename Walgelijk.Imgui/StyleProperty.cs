using System;
using System.Collections.Generic;

namespace Walgelijk.Imgui
{
    public struct StyleProperty<T>
    {
        public T Inactive;
        public T Hover;
        public T Active;

        public StyleProperty(T inactive, T hover, T active)
        {
            Inactive = inactive;
            Hover = hover;
            Active = active;
        }

        public StyleProperty(T colour)
        {
            Inactive = colour;
            Hover = colour;
            Active = colour;
        }

        public readonly T GetFor(State state) => state switch
        {
            State.Hover => Hover,
            State.Active => Active,
            _ => Inactive,
        };

        public override bool Equals(object? obj)
        {
            return obj is StyleProperty<T> property &&
                   EqualityComparer<T>.Default.Equals(Inactive, property.Inactive) &&
                   EqualityComparer<T>.Default.Equals(Hover, property.Hover) &&
                   EqualityComparer<T>.Default.Equals(Active, property.Active);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Inactive, Hover, Active);
        }

        public static implicit operator StyleProperty<T>(T s) => new(s);
        public static implicit operator StyleProperty<T>((T, T, T) c) => new(c.Item1, c.Item2, c.Item3);

        public static bool operator ==(StyleProperty<T> left, StyleProperty<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StyleProperty<T> left, StyleProperty<T> right)
        {
            return !(left == right);
        }
    }
}