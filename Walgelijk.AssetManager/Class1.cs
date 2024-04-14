using System.Security.Cryptography;
using Walgelijk;


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


namespace Walgelijk.AssetManager;

public interface IAssetDeserialiser
{
    public bool IsCandidate(in AssetMetadata assetMetadata);

    public object Deserialise(Stream stream, in AssetMetadata assetMetadata);
}

public interface IAssetDeserialiser<T> : IAssetDeserialiser
{
    public T Deserialise(Stream stream, in AssetMetadata assetMetadata);
}
