using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

/// <summary>
/// Locally unique ID for an asset
/// </summary>
[JsonConverter(typeof(AssetIdConverter))]
public readonly struct AssetId
{
    /// <summary>
    /// Id of the asset within the asset package
    /// </summary>
    public readonly int Internal;

    public AssetId(string path)
    {
        Internal = Hashes.MurmurHash1(path);
    }  
    
    public AssetId(ReadOnlySpan<char> path)
    {
        Internal = Hashes.MurmurHash1(path);
    }

    public AssetId(int @internal)
    {
        Internal = @internal;
    }

    public override string ToString() => Internal.ToString();
}
