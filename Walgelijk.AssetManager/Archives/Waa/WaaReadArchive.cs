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
        using var reader = new BinaryReader(new FileStream(file, FileMode.Open), Encoding.UTF8, false);

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
            long length = reader.ReadInt64();
            index.Add(path, new ReadEntry
            {
                StartOffset = chunkStartIndex,
                ChunkCount = chunkCount,
                Length = length
            });
        }

        entries = index.ToImmutableDictionary();
    }

    public Stream? GetEntry(string path)
    {
        // I was JUST doing this! I have to go to bed though...
        if (entries.TryGetValue(path, out var r))
            return new SubSeekableFileStream(new FileStream(BaseFile, FileMode.Open), r.StartOffset, r.Length);
        return null;
    }

    public bool HasEntry(string path) => entries.ContainsKey(path);

    public void Dispose()
    {
    }
}

public class SubSeekableFileStream : Stream
{
    public readonly FileStream BaseStream;
    private readonly bool leaveOpen;

    public SubSeekableFileStream(FileStream baseStream, int start, long length, bool leaveOpen = false)
    {
        BaseStream = baseStream;
        StartOffset = start;
        Length = length;
        this.leaveOpen = leaveOpen;
    }

    public override bool CanRead => BaseStream.CanRead;

    public override bool CanSeek => BaseStream.CanSeek;

    public override bool CanWrite => false;

    public override long Length { get; }

    public override long Position
    {
        get => BaseStream.Position - StartOffset;
        set => BaseStream.Position = value + StartOffset;
    }

    public int StartOffset { get; }

    public override void Flush()
    {
        BaseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        count = int.Min(buffer.Length, count);
        var actualStart = offset + StartOffset;
        var actualEnd = int.Min(actualStart + count, StartOffset + (int)Length);
        var actualCount = actualEnd - actualStart;

        return BaseStream.Read(buffer, actualStart, actualCount);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length - offset;
                break;
            default:
                Position = offset;
                break;
        }
        return Position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!leaveOpen)
            BaseStream.Dispose();
    }
}