using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Walgelijk;

public static class StringExtension
{
    /// <summary>
    /// Returns an array where each entry is a byte that corresponds to the given string in ASCII
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ToByteArray(this string str)
    {
        return Encoding.ASCII.GetBytes(str);
    }

    /// <summary>
    /// Populates an array where each entry is a byte that corresponds to the given string in ASCII
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ToByteArray(this string str, Span<byte> buffer)
    {
        Encoding.ASCII.GetBytes(str, buffer);
    }
}
