/* -- Asset manager system --
 * 
 * What I need:
 * - Retrieve asset
 * - Query assets (e.g get all assets of type, in set, with tag, whatever the fuck)
 * - Cross-platform paths
 * - Reference counting / lifetime
 * - Mod support
 * - Stream support
 * - Async
 * - Develop using files, create package on build
 * - Loading packages
 * - Unloading packages
 * - Having multiple packages loaded at once
 * 
 * Proposed API:
 * - static AssetPackage.Load(string path)
 *      - Integrates with Resources.Load<AssetPackage>(string path)
 * - AssetPackage.Dispose()
 * - AssetPackage.Query()
 * - AssetPackage.Id
 * 
 * Resources:
 *  - Game Engine Architecture 3rd Edition, ch 7
 */

using System.IO;

namespace Walgelijk.AssetManager;

/// <summary>
/// Globally unique ID for an asset
/// </summary>
public readonly struct GlobalAssetId
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

    public override string ToString() => $"{External}:{Internal}";
}
