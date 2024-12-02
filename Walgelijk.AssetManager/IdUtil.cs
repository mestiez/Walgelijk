using System.Collections.Concurrent;
using System.Globalization;

namespace Walgelijk.AssetManager;

public static class IdUtil
{
    /// <summary>
    /// Any package IDs in this collection will be omitted when in the <see cref="ToNamedString(GlobalAssetId)"/> result.
    /// This is useful to prevent any serialised IDs from blocking modded overrides. 
    /// </summary>
    public static readonly ConcurrentBag<PackageId> VanillaPackageIds = [];

    public static int Hash(ReadOnlySpan<char> a) => Hashes.MurmurHash1(a);

    public static int Convert(ReadOnlySpan<char> a)
    {
        if (TryConvert(a, out var result))
            return result;

        throw new ArgumentException($"Given ID \"{a}\" is invalid");
    }

    public static bool TryConvert(ReadOnlySpan<char> a, out int result)
    {
        return int.TryParse(a, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public static string Convert(int a)
    {
        if (TryConvert(a, out var result))
            return result;

        throw new ArgumentException($"Given ID \"{a}\" is invalid");
    }

    public static bool TryConvert(int a, out string result)
    {
        result = a.ToString("D", CultureInfo.InvariantCulture);
        return true;
    }

    public static string ToNamedString(GlobalAssetId id)
    {
        if (id.IsAgnostic)
            Assets.TryFindFirst(id.Internal, out id);

        if (Assets.TryGetMetadata(id, out var asset))
        {
            if (VanillaPackageIds.Contains(id.External))
                return asset.Path;

            if (Assets.TryGetPackage(id.External, out var package))
                return $"{package.Metadata.Name}:{asset.Path}";
        }

        return id.ToString();
    }

    public static string ToUnnamedString(GlobalAssetId id)
    {
        if (id.IsAgnostic || VanillaPackageIds.Contains(id.External))
            return Convert(id.Internal.Internal);

        return $"{id.External}:{id.Internal}";
    }
}