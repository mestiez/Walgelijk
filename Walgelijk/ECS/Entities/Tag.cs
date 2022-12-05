using System;

namespace Walgelijk
{
    /// <summary>
    /// A tag that can be connected to an entity. Multiple objects can share the same tag.
    /// </summary>
    public struct Tag
    {
        /// <summary>
        /// The tag value
        /// </summary>
        public int Value;

        public Tag(int value)
        {
            Value = value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Tag tag &&
                   Value == tag.Value;
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
}
