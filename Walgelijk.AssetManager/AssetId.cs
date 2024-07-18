using Newtonsoft.Json;
using System.Globalization;

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

    public static AssetId Parse(ReadOnlySpan<char> id)
    {
        if (TryParse(id, out var asset))
            return asset;

        throw new Exception($"{id} is not a valid ID");
    }

    public static AssetId Parse(string id)
    {
        return Parse(id.AsSpan());
    }

    public static bool TryParse(ReadOnlySpan<char> id, out AssetId asset)
    {
        if (int.TryParse(id, out var i) || int.TryParse(id, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out i))
        {
            asset = new AssetId(i);
            return true;
        }

        asset = default;
        return false;
    }

    public override string ToString() => Internal.ToString("X2");

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
