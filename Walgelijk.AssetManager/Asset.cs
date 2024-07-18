namespace Walgelijk.AssetManager;

/// <summary>
/// Struct that provides objects to access asset data
/// </summary>
public readonly record struct Asset
{
    public readonly AssetMetadata Metadata;
    public readonly Func<Stream> Stream;

    public Asset(AssetMetadata metadata, Func<Stream> stream)
    {
        Metadata = metadata;
        Stream = stream;
    }
}
