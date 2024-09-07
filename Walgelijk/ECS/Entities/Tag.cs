using System;

namespace Walgelijk;

/// <summary>
/// A tag that can be connected to an entity. Multiple objects can share the same tag.
/// </summary>
public struct Tag : IEquatable<Tag>
{
    /// <summary>
    /// The tag value
    /// </summary>
    public int Value;

    public Tag(int value)
    {
        Value = value;
    }

    public Tag(in ReadOnlySpan<char> value)
    {
        Value = Hashes.MurmurHash1(value);
    }

    public Tag(in string value)
    {
        Value = Hashes.MurmurHash1(value);
    }

    /// <summary>
    /// Generate new Tag with a unique value that probably doesn't matter
    /// </summary>
    public static Tag CreateUnique() => new Tag(Guid.NewGuid().GetHashCode());

    public override bool Equals(object? obj)
    {
        return obj is Tag tag &&
               Value == tag.Value;
    }

    public bool Equals(Tag other)
    {
        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
    }

    public static bool operator ==(Tag left, Tag right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Tag left, Tag right)
    {
        return !(left == right);
    }
}
