namespace Walgelijk.AssetManager;

/// <summary>
/// Struct that provides objects to access asset data
/// </summary>
public record struct Asset
{
    public AssetMetadata Metadata;
    public Lazy<Stream> Stream;
}