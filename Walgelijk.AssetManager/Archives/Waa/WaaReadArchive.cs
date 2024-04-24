using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.AssetManager.Archives.Waa;

public class WaaReadArchive : IReadArchive
{
    private readonly ImmutableDictionary<string, ReadEntry> entries;

    private struct ReadEntry
    {
        public int StartOffset;
        public int Length;
    }

    public WaaReadArchive(Stream input)
    {
        using var reader = new BinaryReader(input);

        if (!reader.ReadChars(4).SequenceEqual("WALG"))
            throw new Exception("File is not a Waa archive");

        long totalSize = reader.ReadInt64();
        int totalChunkCount = reader.ReadInt32();
        int entryCount = reader.ReadInt32();
        int chunkListOffset = reader.ReadInt32();

        var index = new Dictionary<string, ReadEntry>();

        var pathAcc = new byte[256];
        for (int i = 0; i < entryCount; i++)
        {
            int pathAccIndex = 0;
            var path = string.Empty;
            while (true)
            {
                var b = reader.ReadByte();
                if (b == '\0')
                {
                    path = Encoding.UTF8.GetString(pathAcc.AsSpan(0, pathAccIndex));
                    break;
                }
                else
                    pathAcc[pathAccIndex] = b;

                pathAccIndex++;
            }
            int chunkStartIndex = reader.ReadInt32();
            int chunkCount = reader.ReadInt32();
            index.Add(path, (chunkStartIndex, chunkCount));
        }

        entries = index.ToImmutableDictionary();
    }

    public Stream? GetEntry(string path)
    {
        // I was JUST doing this! I have to go to bed though...
        if (entries.TryGetValue(path, out var r))
        {
            return new SubSeekableFileStream()
        }
    }

    public bool HasEntry(string path)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

internal class Entry
{
    public int StartIndex;
    public int ChunkCount;

    public byte[] Data = [];
}

internal struct Chunk
{
    public const int ChunkSize = 512;

    public int Index;
    public int Length;

    public byte[] Buffer;

    public readonly int Start => ChunkSize * Index;

    public Chunk(int index)
    {
        Index = index;
        Length = 0;
        Buffer = new byte[ChunkSize];
    }

    public void Set(ReadOnlySpan<byte> b)
    {
        if (b.Length > ChunkSize)
            throw new Exception("Written length exceeds bounds");

        Array.Clear(Buffer);
        b.CopyTo(Buffer);

        Length = b.Length;
    }
}