namespace Walgelijk.AssetManager;

public struct AssetPackageMetadata
{
    public required PackageId Id;
    public required string Name;
    public int Count;
    public Version FormatVersion;
    public Version EngineVersion;
}
