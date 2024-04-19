namespace Walgelijk.AssetManager;

public interface IWriteArchive : IDisposable
{
    public long WriteEntry(string path, Stream stream);
}
