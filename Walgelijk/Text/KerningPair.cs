using System;

namespace Walgelijk;

/// <summary>
/// The two characters a kerning amount applies to
/// </summary>
public struct KerningPair : IEquatable<KerningPair>
{
    /// <summary>
    /// Previous character in the sequence
    /// </summary>
    public char Current;

    /// <summary>
    /// Current character in the sequence
    /// </summary>
    public char Next;

    public KerningPair(char current, char next)
    {
        Current = current;
        Next = next;
    }

    public override bool Equals(object? obj)
    {
        return obj is KerningPair pair && Equals(pair);
    }

    public bool Equals(KerningPair other)
    {
        return Current == other.Current &&
               Next == other.Next;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Current, Next);
    }

    public static bool operator ==(KerningPair left, KerningPair right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KerningPair left, KerningPair right)
    {
        return !(left == right);
    }
}
