using Newtonsoft.Json;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Walgelijk;

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

public static class AssetPackageUtils
{
    /* AssetPackage structure:
     * 
     * 📁 root
     *  | 📁 assets 
     *  |  | ~ all assets go here
     *  | 📁 metadata
     *  |  | ~ every asset has a corresponding .meta file, which contains
     *  |  |   a kv map of properties such as file size, mime type, tags, etc.
     *  | 📃 guid_table.txt
     *  | 📃 package.json
     */

    public static void Build(string id, DirectoryInfo directory, Stream output)
    {
        const string assetRoot = "assets/";
        using var archive = new ZipArchive(output, ZipArchiveMode.Create, true);
        var guidsIntermediate = new HashSet<int>();
        var guidTable = new StringBuilder();

        var package = new AssetPackageMetadata
        {
            Id = id
        };

        ProcessDirectory(directory, assetRoot);

        void ProcessDirectory(DirectoryInfo info, string target)
        {
#if DEBUG
            global::System.Diagnostics.Debug.Assert(target.EndsWith('/'));
#endif
            foreach (var childFile in info.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                var path = target + childFile.Name;

                var resourcePath = Path.GetRelativePath(assetRoot, path).Replace('\\', '/').ToLowerInvariant().Trim();
                var id = new AssetId(resourcePath);
                if (!guidsIntermediate.Add(id.Internal))
                    throw new Exception($"Resource at \"{resourcePath}\" collides with another resource.");

                // create and write entry
                {
                    var e = archive.CreateEntry(path, CompressionLevel.Fastest);
                    using var s = e.Open();
                    using var fileStream = childFile.OpenRead();
                    fileStream.CopyTo(s);
                }

                // write guid to table
                guidTable.AppendLine(id.Internal.ToString());
                guidTable.AppendLine(resourcePath);

                package.Count++;

                Console.WriteLine("Processed {0} entries", package.Count);
            }

            foreach (var childDir in info.GetDirectories("*", SearchOption.TopDirectoryOnly))
                ProcessDirectory(childDir, target + childDir.Name + '/');
        }

        {
            var guidTableEntry = archive.CreateEntry("guid_table.txt");
            using var s = guidTableEntry.Open();
            s.Write(Encoding.UTF8.GetBytes(guidTable.ToString()));
            s.Dispose();
        }

        {
            var packageJsonEntry = archive.CreateEntry("package.json");
            using var s = new StreamWriter(packageJsonEntry.Open(), Encoding.UTF8, leaveOpen: false);
            s.WriteLine(JsonConvert.SerializeObject(package));
            s.Dispose();
        }
    }
}

public struct AssetPackageMetadata
{
    public required string Id;
    public int Count;
}


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

public readonly struct AssetWrapper
{
    public readonly AssetId Id;
    public readonly AssetType Type;

    public AssetWrapper(AssetId id, AssetType type)
    {
        Id = id;
        Type = type;
    }
}

public readonly struct AssetType
{
    public readonly string Value;

    public AssetType(string value)
    {
        Value = value;
    }

    public readonly static AssetType Binary = new("BINARY");
    public readonly static AssetType String = new("STRING");
    public readonly static AssetType Texture = new("TEXTURE");
    public readonly static AssetType Video = new("VIDEO");
    public readonly static AssetType FixedAudio = new("AUDIO_FIXED");
    public readonly static AssetType StreamAudio = new("AUDIO_STREAM");
}

public static class AssetTypeLoaders
{
    //public static readonly Dictionary<AssetType, Func<Stream, object>> Loaders = [];

    static AssetTypeLoaders()
    {
        // Een asset kan fixed of streaming zijn en deze twee types zijn
        // zó anders dat er twee verschillende manieren moeten komen
        // om assets te lezen. Hoegadegijdadoen

        //Loaders.Add(AssetType.Binary, s => Game.Main.AudioRenderer.LoadStream(s));
    }
}

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
}

/// <summary>
/// Locally unique ID for an asset
/// </summary>
public readonly struct AssetId
{
    /// <summary>
    /// Id of the asset within the asset package
    /// </summary>
    public readonly int Internal;

    public AssetId(string path)
    {
        Internal = Hashes.MurmurHash1(path);
    }

    public AssetId(int @internal)
    {
        Internal = @internal;
    }
}
