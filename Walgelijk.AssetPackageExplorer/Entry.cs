using Walgelijk.AssetManager;

namespace Walgelijk.AssetPackageExplorer;

public readonly struct Entry
{
    // applicable if asset
    public readonly AssetMetadata? Asset;

    // applicable if folder
    public readonly string? Destination;

    public readonly string Name;

    public Entry(string name, string folderpath)
    {
        Name = name;
        Destination = folderpath;
    }

    public Entry(AssetMetadata asset)
    {
        Asset = asset;
        Name = Path.GetFileName(asset.Path);
    }
}
