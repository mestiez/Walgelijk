using System.Text;

namespace Walgelijk.AssetManager.Archives.Waa;

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
            IntermediateData = m.ToArray(),
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
            while (i < entry.IntermediateData.Length)
            {
                var segment = entry.IntermediateData.AsSpan(i..);
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
            entry.Length = entry.IntermediateData.Length;
            entry.IntermediateData = [];
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
         * 0    WALG                (magic) 4
         * 4    Total length        (ulong) 8
         * 12   Chunk count         (int32) 4
         * 16   Entry count         (int32) 4
         * 20   Chunk set offset    (int32) 4
         */

        /* Index format
         * 
         * Path until null terminator   (string) n
         * Chunk start index            (int32)  4
         * Chunk count                  (int32)  4
         * Length in bytes              (int64)  8
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
            file.Write(Encoding.UTF8.GetBytes(item.Key));
            file.Write((byte)'\0');
            file.Write(item.Value.StartIndex);
            file.Write(item.Value.ChunkCount);
            file.Write(item.Value.Length);
        }

        var pos = file.BaseStream.Position;
        file.BaseStream.Seek(20, SeekOrigin.Begin);
        file.Write((int)pos); // write chunk set offset
        file.BaseStream.Seek(pos, SeekOrigin.Begin);

        // write actual binary data
        foreach (var item in chunks)
            file.Write(item.Buffer);
    }
}
