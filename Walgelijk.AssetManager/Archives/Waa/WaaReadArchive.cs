using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.AssetManager.Archives.Waa;

public class WaaReadArchive : IReadArchive
{
    private readonly ImmutableDictionary<string, ReadEntry> entries;
    private readonly ConcurrentBag<SubSeekableFileStream> ownedStreams = [];

    public string BaseFile { get; }

    private struct ReadEntry
    {
        public int StartOffset;
        public int ChunkCount;
        public long Length;
    }

    public WaaReadArchive(string file)
    {
        BaseFile = file;
        using var reader = new BinaryReader(new FileStream(file, FileMode.Open, FileAccess.Read), Encoding.UTF8, false);

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
                if (pathAccIndex >= pathAcc.Length)
                    throw new Exception("Path in index exceeds max length of " + pathAcc.Length);
            }
            int chunkStartIndex = reader.ReadInt32() * Chunk.ChunkSize;
            int chunkCount = reader.ReadInt32();
            long length = reader.ReadInt64();

            if (chunkCount != (int)double.Ceiling(length / (double)Chunk.ChunkSize))
                throw new Exception("Entry chunk count does not match length");

            index.Add(path, new ReadEntry
            {
                StartOffset = chunkStartIndex + chunkListOffset,
                ChunkCount = chunkCount,
                Length = length
            });
        }

        entries = index.ToImmutableDictionary();
    }

    public Stream? GetEntry(string path)
    {
        if (entries.TryGetValue(path, out var r))
        {
            var s = new SubSeekableFileStream(new FileStream(BaseFile, FileMode.Open, FileAccess.Read), r.StartOffset, r.Length);
            ownedStreams.Add(s);
            return s;
        }
        return null;
    }

    public bool HasEntry(string path) => entries.ContainsKey(path);

    public void Dispose()
    {
        while (ownedStreams.TryTake(out var t))
            t.Dispose();
    }
}
