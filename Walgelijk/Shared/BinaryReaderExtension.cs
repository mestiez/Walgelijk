using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Walgelijk;

public static class BinaryReaderExtension
{
    public static bool SkipUntil(this BinaryReader reader, in string marker, Encoding? encoding = null)
    {
        return SkipUntil(reader, (encoding ?? Encoding.ASCII).GetBytes(marker));
    }

    public static bool SkipUntil(this BinaryReader reader, in ReadOnlySpan<byte> marker)
    {
        var buffer = new byte[marker.Length];
        var c = reader.Read(buffer);

        if (c != buffer.Length)
            return false; // the stream isnt even long enough

        if (marker.SequenceEqual(buffer))
            return true;

        while (true)
        {
            var next = reader.Read();
            if (next == -1)
                return false;

            Array.Copy(buffer, 1, buffer, 0, buffer.Length - 1); // we shift the array to make place for a new entry
            buffer[^1] = (byte)next;

            if (marker.SequenceEqual(buffer))
                return true;
        }
    }
}
