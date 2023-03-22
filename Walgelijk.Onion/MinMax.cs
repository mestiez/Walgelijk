using System;

namespace Walgelijk.Onion;

public readonly struct MinMax<T> : IEquatable<MinMax<T>> where T : struct, IEquatable<T>, IComparable<T>
{
    public readonly T Min;
    public readonly T Max;

    public MinMax(T min, T max)
    {
        Min = min;
        Max = max;
    }

    public override bool Equals(object? obj)
    {
        return obj is MinMax<T> max && Equals(max);
    }

    public bool Equals(MinMax<T> other)
    {
        return EqualityComparer<T>.Default.Equals(Min, other.Min) &&
               EqualityComparer<T>.Default.Equals(Max, other.Max);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Min, Max);
    }

    public override string ToString() => $"({Min} < {Max})";

    public static bool operator ==(MinMax<T> left, MinMax<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MinMax<T> left, MinMax<T> right)
    {
        return !(left == right);
    }
}
