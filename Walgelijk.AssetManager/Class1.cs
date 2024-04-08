namespace Walgelijk.AssetManager;

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
 * Resources:
 *  - Game Engine Architecture 3rd Edition, ch 7
 */

public class AssetPackage
{

}

public struct AssetType
{
    public string Value;
}