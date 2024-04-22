namespace Walgelijk.AssetManager;

public struct AssetPackageMetadata
{
    public required string Id;
    public required int NumericalId;
    public int Count;
    public Version FormatVersion;
    public Version EngineVersion;
}
