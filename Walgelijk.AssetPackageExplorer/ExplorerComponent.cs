using Walgelijk.AssetManager;

namespace Walgelijk.AssetPackageExplorer;

public class ExplorerComponent : Component, IDisposable
{
    public ReaderWriterLockSlim Lock = new();
    public AssetPackage? Package;

    public bool PackageChanged = false;
    public bool FolderChanged = false;

    public bool IsRebuildingFolderCache = false;
    public Entry[] FolderCache = [];
    public string CurrentPath = string.Empty;
    public AssetMetadata? SelectedAsset;

    public object? SelectedLoadedAsset;

    public CancellationTokenSource FolderRefreshCancel = new();

    public void Dispose()
    {
        Lock.Dispose();
        Package?.Dispose();
    }
}
