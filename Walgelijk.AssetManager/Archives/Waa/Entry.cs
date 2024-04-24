namespace Walgelijk.AssetManager.Archives.Waa;

internal class Entry
{
    public int StartIndex;
    public int ChunkCount;
    public long Length;

    public byte[] IntermediateData = [];
}
