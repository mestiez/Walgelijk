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
    public readonly PackageId External;

    /// <summary>
    /// Id of the asset within the asset package <see cref="External"/>
    /// </summary>
    public readonly AssetId Internal;

    public GlobalAssetId(string assetPackage, string path)
    {
        External = new(assetPackage);
        Internal = new(path);
    }

    public GlobalAssetId(int external, int @internal)
    {
        External = new(external);
        Internal = new(@internal);
    }

    public GlobalAssetId(PackageId external, AssetId @internal)
    {
        External = external;
        Internal = @internal;
    }

    public GlobalAssetId(ReadOnlySpan<char> formatted)
    {
        var index = formatted.IndexOf(':');

        var external = formatted[..index];
        var @internal = formatted[(index + 1)..];
        if (index == -1)
            throw new Exception("Formatted string contains no external ID part");

        External = new(external);
        Internal = new(@internal);
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
        return External.Equals(other.External) &&
               Internal.Equals(other.Internal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(External, Internal);
    }

    public static readonly GlobalAssetId None = new(PackageId.None, AssetId.None);
}
