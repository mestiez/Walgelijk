using Newtonsoft.Json;
using System.IO;

namespace Walgelijk.AssetManager;

/// <summary>
/// Globally unique ID for an asset
/// </summary>
[JsonConverter(typeof(GlobalAssetIdConverter))]
public readonly struct GlobalAssetId : IEquatable<GlobalAssetId>
{
    /// <summary>
    /// Id of the asset package this asset resides in
    /// </summary>
    public readonly int External;

    /// <summary>
    /// Id of the asset within the asset package <see cref="External"/>
    /// </summary>
    public readonly AssetId Internal;

    public GlobalAssetId(string assetPackage, string path)
    {
        External = Hashes.MurmurHash1(assetPackage);
        Internal = new(path);
    }

    public GlobalAssetId(int external, int @internal)
    {
        External = external;
        Internal = new(@internal);
    }

    public GlobalAssetId(int external, AssetId @internal)
    {
        External = external;
        Internal = @internal;
    }

    public GlobalAssetId(string assetPackage, AssetId @internal)
    {
        External = Hashes.MurmurHash1(assetPackage);
        Internal = @internal;
    }   
    
    public GlobalAssetId(string assetPackage, int @internal)
    {
        External = Hashes.MurmurHash1(assetPackage);
        Internal = new(@internal);
    }

    public GlobalAssetId(ReadOnlySpan<char> formatted)
    {
        var sp = formatted.IndexOf(':');
        if (sp == -1)
            throw new Exception("Formatted string contains no external ID part");

        External = Hashes.MurmurHash1(formatted[..sp]);
        Internal = new(formatted[(sp+1)..]);
    }

    public static implicit operator GlobalAssetId(ReadOnlySpan<char> formatted)
        => new GlobalAssetId(formatted);

    public static implicit operator GlobalAssetId(string formatted)
        => new GlobalAssetId(formatted);

    public static bool operator ==(GlobalAssetId left, GlobalAssetId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GlobalAssetId left, GlobalAssetId right)
    {
        return !(left == right);
    }

    public override string ToString() => $"{External}:{Internal}";

    public override bool Equals(object? obj)
    {
        return obj is GlobalAssetId id && Equals(id);
    }

    public bool Equals(GlobalAssetId other)
    {
        return External == other.External &&
               Internal.Equals(other.Internal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(External, Internal);
    }

    public static readonly GlobalAssetId None = default;
}
