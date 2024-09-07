using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

    /// <summary>
    /// True if the package ID was not supplied, meaning this ID is package-agnostic.
    /// </summary>
    public bool IsAgnostic => External == PackageId.None;

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
    
    public GlobalAssetId(AssetId @internal)
    {
        External = PackageId.None;
        Internal = @internal;
    }

    public GlobalAssetId(ReadOnlySpan<char> formatted)
    {
        var index = formatted.IndexOf(':');
        if (index == -1)
        {
            External = PackageId.None;
            Internal = new AssetId(formatted);
        }
        else
        {
            var external = formatted[..index];
            var @internal = formatted[(index + 1)..];

            External = new(external);
            Internal = new(@internal);
        }
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

    public override string ToString() => IsAgnostic ? Internal.ToString() : $"{External}:{Internal}";

    public string ToNamedString()
    {
        var id = this;

        if (IsAgnostic)
            Assets.TryFindFirst(Internal, out id);

        if (Assets.TryGetMetadata(id, out var asset) && Assets.TryGetPackage(id.External, out var package))
            return $"{package.Metadata.Name}:{asset.Path}";

        return ToString();
    }

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

    /// <summary>
    /// If this ID is agnostic, resolve the agnosticism (<see cref="Assets.FindFirst"/>).
    /// </summary>
    public GlobalAssetId ResolveExternal()
    {
        if (IsAgnostic && Assets.TryFindFirst(Internal, out var a))
            return a;
        return this;
    }

    public static readonly GlobalAssetId None = new(PackageId.None, AssetId.None);
}
