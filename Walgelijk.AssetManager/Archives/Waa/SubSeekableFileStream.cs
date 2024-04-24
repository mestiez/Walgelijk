namespace Walgelijk.AssetManager.Archives.Waa;

public class SubSeekableFileStream : Stream
{
    public readonly FileStream BaseStream;
    private readonly bool leaveOpen;

    public SubSeekableFileStream(FileStream baseStream, long start, long length, bool leaveOpen = false)
    {
        BaseStream = baseStream;
        StartOffset = start;
        Length = length;
        BaseStream.Seek(start, SeekOrigin.Begin);
        this.leaveOpen = leaveOpen;
    }

    public override bool CanRead => AssertValidBase() && BaseStream.CanRead;

    public override bool CanSeek => AssertValidBase() && BaseStream.CanSeek;

    public override bool CanWrite => false;

    public override long Length { get; }

    public override long Position
    {
        get
        {
            AssertValidBase();
            return BaseStream.Position - StartOffset;
        }

        set
        {
            AssertValidBase();
            var p = value + StartOffset;
            p = long.Max(StartOffset, p);
            p = long.Min(StartOffset + Length, p);
            BaseStream.Position = p;
        }
    }

    public long StartOffset { get; }

    public override void Flush()
    {
        AssertValidBase();

        BaseStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        AssertValidBase();

        count = int.Min(buffer.Length, count);
        count = (int)long.Min(Length - Position, count);
        if (count <= 0)
            return 0;

        return BaseStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        AssertValidBase();

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

    private bool AssertValidBase() => BaseStream == null ? throw new ObjectDisposedException("Base stream is disposed") : true;
}