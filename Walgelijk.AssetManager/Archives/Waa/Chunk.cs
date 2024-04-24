namespace Walgelijk.AssetManager.Archives.Waa;

internal struct Chunk
{
    public const int ChunkSize = 512;

    public int Index;
    public int Length;

    public byte[] Buffer;

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