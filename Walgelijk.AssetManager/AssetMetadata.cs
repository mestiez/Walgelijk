namespace Walgelijk.AssetManager;

public struct AssetMetadata
{
    public AssetId Id;
    public string Path;
    public long Size;
    public string MimeType;
    public string[]? Tags;
    public string XXH3;
}
