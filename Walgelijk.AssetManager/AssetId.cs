using Newtonsoft.Json;

namespace Walgelijk.AssetManager;

/// <summary>
/// Locally unique ID for an asset
/// </summary>
[JsonConverter(typeof(AssetIdConverter))]
public readonly struct AssetId : IEquatable<AssetId>
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

    public override bool Equals(object? obj)
    {
        return obj is AssetId id && Equals(id);
    }

    public bool Equals(AssetId other)
    {
        return Internal == other.Internal;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Internal);
    }

    public static readonly AssetId None = default;

    public static bool operator ==(AssetId left, AssetId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(AssetId left, AssetId right)
    {
        return !(left == right);
    }
}
