namespace Walgelijk.AssetManager;

public interface IReadArchive : IDisposable
{
    public bool HasEntry(string path);
    public Stream? GetEntry(string path);
}
