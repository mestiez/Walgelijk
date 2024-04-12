using System.IO.Compression;
using System.Security.Cryptography;

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

public static class AssetPackageBuilder
{
    public void Build(DirectoryInfo directory, Stream output)
    {
        using var archive = new ZipArchive(output, ZipArchiveMode.Create, true);

        ProcessDirectory(directory, "/");

        void ProcessDirectory(DirectoryInfo info, string currentPath)
        {
#if DEBUG
            global::System.Diagnostics.Debug.Assert(currentPath.EndsWith('/'));
#endif
            foreach (var childFile in info.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                var e = archive.CreateEntry(currentPath + childFile.Name, CompressionLevel.Fastest);
                using var s = e.Open();

                using var fileStream = childFile.OpenRead();
                fileStream.CopyTo(s);

                s.Dispose();
                fileStream.Dispose();
            }

            foreach (var childDir in info.GetDirectories("*", SearchOption.TopDirectoryOnly))
            {
                ProcessDirectory(info, currentPath + childDir.Name + '/');
            }
        }
    }
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
        Archive.GetEntry() // Ja dit kan niet want
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
    public static readonly Dictionary<AssetType, Func<Stream, object>> Loaders = [];

    static AssetTypeLoaders()
    {
        // Een asset kan fixed of streaming zijn en deze twee types zijn
        // zó anders dat er twee verschillende manieren moeten komen
        // om assets te lezen. Hoegadegijdadoen

        Loaders.Add(AssetType.Binary, s => Game.Main.AudioRenderer.LoadStream(s));
    }
}

/// <summary>
/// Globally unique ID for an asset
/// </summary>
public readonly struct AssetId
{
    /// <summary>
    /// Id of the asset package this asset resides in
    /// </summary>
    public readonly int External;

    /// <summary>
    /// Id of the asset within the asset package <see cref="External"/>
    /// </summary>
    public readonly int Internal;

    public AssetId(string assetPackage, string path)
    {
        External = Hashes.MurmurHash1(assetPackage);
        Internal = Hashes.MurmurHash1(path);
    }

    public AssetId(int external, int @internal)
    {
        External = external;
        Internal = @internal;
    }
}
