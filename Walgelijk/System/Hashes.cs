using System;

namespace Walgelijk;

/// <summary>
/// Utility struct that provides hashing functions that take a string and return an int
/// </summary>
public readonly struct Hashes
{
    public static int MurmurHash1(string key) => MurmurHash1(key.AsSpan());

    public static int MurmurHash1(ReadOnlySpan<char> key)
    {
        const uint c1 = 0xcc9e2d51;
        const uint c2 = 0x1b873593;
        const int r1 = 15;
        const uint m = 5;
        const uint n = 0xe6546b64;

        uint hash = 0;
        uint len = (uint)key.Length;
        for (int i = 0; i < len; i++)
        {
            hash ^= key[i];
            hash *= c1;
            hash = (hash << r1) | (hash >> (32 - r1));
            hash *= c2;
        }

        hash ^= len;
        hash ^= (hash >> 16);
        hash *= n;
        hash ^= (hash >> 13);
        hash *= m;
        hash ^= (hash >> 16);

        return (int)hash;
    }
}
