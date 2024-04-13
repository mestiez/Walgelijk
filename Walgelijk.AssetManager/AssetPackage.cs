using System.IO.Compression;


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

public class AssetPackage : IDisposable
{
    public readonly string Id;
    public readonly ZipArchive Archive;

    public AssetPackage(string id, ZipArchive archive)
    {
        Id = id;
        Archive = archive;
    }

    public static AssetPackage Load(string path)
    {
        var file = new FileStream(path, FileMode.Open);
        var archive = new ZipArchive(file, ZipArchiveMode.Read, false);
        return new AssetPackage(Path.GetFileNameWithoutExtension(path), archive);
    }

    public AssetWrapper Get(int internalId)
    {
        return default;
        //Archive.GetEntry() // Ja dit kan niet want
        // die zip archive weet niks van IDs. 
        // je hebt twee opties:
        // 1. Geen ZipArchive, zelf binary sequence maken en lezen
        // 2. Geen integer-IDs, hou het pad bij
    }

    public void Dispose()
    {
        Archive.Dispose();
    }
}
