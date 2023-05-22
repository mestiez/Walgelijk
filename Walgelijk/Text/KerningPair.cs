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
    public char PreviousChar;
    /// <summary>
    /// Current character in the sequence
    /// </summary>
    public char CurrentChar;

    public override bool Equals(object? obj)
    {
        return obj is KerningPair pair &&
               PreviousChar == pair.PreviousChar &&
               CurrentChar == pair.CurrentChar;
    }

    public bool Equals(KerningPair other)
    {
        return PreviousChar == other.PreviousChar &&
               CurrentChar == other.CurrentChar;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PreviousChar, CurrentChar);
    }

    public static bool operator ==(KerningPair left, KerningPair right)
    {
        return left.PreviousChar == right.PreviousChar &&
               left.CurrentChar == right.CurrentChar;
    }

    public static bool operator !=(KerningPair left, KerningPair right)
    {
        return !(left == right);
    }
}
