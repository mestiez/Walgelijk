using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.AssetManager.Archives.Waa;

public class WaaReadArchive : IReadArchive
{
    public Stream? GetEntry(string path)
    {
        // I was JUST doing this! I have to go to bed though...
        throw new NotImplementedException();
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

public class WaaWriteArchive : IWriteArchive
{
    public readonly Stream Output;

    private readonly Dictionary<string, Entry> entries = [];
    private Chunk[] chunks = [];
    private long totalWrittenBytes;

    public WaaWriteArchive(Stream output)
    {
        Output = output;
    }

    public long WriteEntry(string path, Stream stream)
    {
        using var m = new MemoryStream();
        stream.CopyTo(m);
        totalWrittenBytes += m.Length;
        entries.Add(path, new Entry
        {
            Data = m.ToArray(),
        });
        return m.Length;
    }

    public void Dispose()
    {
        Build();
        WriteToFile();
    }

    private void Build()
    {
        var chunks = new List<Chunk>();
        int chunkCursor = 0;

        foreach (var item in entries)
        {
            var entry = item.Value;

            int startIndex = chunkCursor;
            int i = 0;
            while (i < entry.Data.Length)
            {
                var segment = entry.Data.AsSpan(i..);
                if (segment.Length > Chunk.ChunkSize)
                    segment = segment[0..Chunk.ChunkSize];
                i += segment.Length;

                var chunk = new Chunk(chunkCursor);
                chunk.Set(segment);
                chunks.Add(chunk);

                chunkCursor++;
                entry.ChunkCount++;
            }

            entry.StartIndex = startIndex;
            entry.Data = [];
        }

        this.chunks = [.. chunks];
    }

    public void WriteToFile()
    {
        using var file = new BinaryWriter(Output, Encoding.UTF8, false);

        /* 
         */

        /* Header format
         * 
         * 0  - 4    WALG               (magic)
         * 4  - 12   Total length       (ulong)
         * 12 - 16   Chunk count        (int32)
         * 16 - 20   Entry count        (int32)
         * 20 - 24   Chunk set offset   (int32)
         */

        /* Index format
         * 
         * Key/path until null terminator   (string)
         * 4 bytes for chunk start index    (int32)
         * 4 bytes for chunk count          (int32)
         */

        // header data
        file.Write("WALG".ToByteArray());
        file.Write(totalWrittenBytes);
        file.Write(chunks.Length);
        file.Write(entries.Count);
        file.Write(0);

        // start write index
        foreach (var item in entries)
        {
            file.Write((byte)'>');
            file.Write(item.Key);
            file.Write((byte)'\0');
            file.Write(item.Value.StartIndex);
            file.Write(item.Value.ChunkCount);
        }

        var pos = file.BaseStream.Position;
        file.BaseStream.Seek(20, SeekOrigin.Begin);
        file.Write((int)pos); // write chunk set offset
        file.BaseStream.Seek(pos, SeekOrigin.Begin);

        // write data
        foreach (var item in chunks)
        {
            file.Write(item.Buffer);
        }
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