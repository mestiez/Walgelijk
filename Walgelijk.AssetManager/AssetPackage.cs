using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;


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
    public readonly AssetPackageMetadata Metadata;
    public readonly ZipArchive Archive;

    private Dictionary<int, string> guidTable = [];
    private AssetFolder hierarchyRoot = new("/");

    public AssetPackage(ZipArchive archive)
    {
        if (archive.Mode != ZipArchiveMode.Read)
            throw new Exception("The provided archive must be loaded in Read only mode");

        Archive = archive;

        // read metadata
        {
            var metadataEntry = archive.GetEntry("package.json") ?? throw new Exception("Archive has no package.json. This asset package is invalid");
            using var e = new StreamReader(metadataEntry.Open(), encoding: Encoding.UTF8, leaveOpen: false);
            Metadata = JsonConvert.DeserializeObject<AssetPackageMetadata>(e.ReadToEnd());
        }

        // read guid table
        {
            var guidTableEntry = archive.GetEntry("guid_table.txt") ?? throw new Exception("Archive has no guid_table.txt. This asset package is invalid.");
            using var e = new StreamReader(guidTableEntry.Open(), encoding: Encoding.UTF8, leaveOpen: false);

            while (true)
            {
                var idLine = e.ReadLine();
                if (idLine == null)
                    break;

                if (!int.TryParse(idLine, out var id))
                    throw new Exception($"Id {idLine} is not an integer");

                var path = e.ReadLine() ?? throw new Exception($"Invalid GUID table: id {idLine} is missing path");

                AssetPackageUtils.AssertPathValidity(path);

                guidTable.Add(id, path);
            }
        }

        // read hierarchy
        {
            var hierarchtEntry = archive.GetEntry("hierarchy.txt") ?? throw new Exception("Archive has no hierarchy.txt. This asset package is invalid.");
            using var e = new StreamReader(hierarchtEntry.Open(), encoding: Encoding.UTF8, leaveOpen: false);

            while (true)
            {
                // TODO error handling

                // read next path
                var path = e.ReadLine();

                if (path == null)
                    break;

                var assetFolder = EnsureHierarchyFolder(path);
                var count = int.Parse(e.ReadLine()!);
                assetFolder.Assets = new int[count];
                for (int i = 0; i < count; i++)
                {
                    int asset = int.Parse(e.ReadLine()!);
                    assetFolder.Assets[i] = asset;
                }
            }
        }
    }

    private static IEnumerable<AssetId> EnumerateFolder(AssetFolder folder, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (folder.Assets != null)
            foreach (var asset in folder.Assets)
                yield return new AssetId(asset);

        if (searchOption == SearchOption.AllDirectories && folder.Folders != null)
            foreach (var c in folder.Folders)
                foreach (var item in EnumerateFolder(c))
                    yield return item;
    }

    public IEnumerable<AssetId> EnumerateFolder(ReadOnlySpan<char> path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (TryGetAssetFolder(path, out var folder))
            return EnumerateFolder(folder);
        throw new Exception($"Path \"{path}\" cannot be found");
    }

    public Asset GetAsset(in AssetId id)
    {
        var path = GetAssetPath(id);

        var entry = Archive.GetEntry("assets/" + path);
        if (entry == null)
            throw new Exception($"Asset {id.Internal} at path \"{path}\" not found. This indicates the archive is malformed.");
        // it is malformed because the guid table is pointing to entries that don't exist

        return new Asset
        {
            Metadata = GetAssetMetadata(id), // TODO we are doing unnecessary work by calling this function (double GetAssetPath)
            Stream = new Lazy<Stream>(() => entry.Open()),
        };
    }

    public string GetAssetPath(in AssetId id)
    {
        if (guidTable.TryGetValue(id.Internal, out var path))
            return path;

        throw new Exception($"Asset {id.Internal} does not exist.");
    }

    public T Load<T>(in string path)
    {
        return Load<T>(new AssetId(path));
    }

    public T Load<T>(in AssetId id)
    {
        return default;
    }

    public void Dispose()
    {
        Archive.Dispose();
    }

    public static AssetPackage Load(string path)
    {
        var file = new FileStream(path, FileMode.Open);
        var archive = new ZipArchive(file, ZipArchiveMode.Read, false);
        return new AssetPackage(archive);
    }

    private AssetMetadata GetAssetMetadata(in AssetId id)
    {
        var path = GetAssetPath(id);
        var entry = Archive.GetEntry("metadata/" + path + ".json");
        if (entry == null)
            throw new Exception($"Asset metadata {id.Internal} at path \"{path}\" not found. This indicates the archive is malformed.");
        // it is malformed because the guid table is pointing to entries that don't exist

        using var reader = new StreamReader(entry.Open(), encoding: Encoding.UTF8, leaveOpen: false);
        var json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<AssetMetadata>(json);
    }

    private bool TryGetAssetFolder(ReadOnlySpan<char> path, [NotNullWhen(true)] out AssetFolder? folder)
    {
        folder = null;
        var navigator = hierarchyRoot;
        ReadOnlySpan<char> current = path;
        while (true)
        {
            current = EatFolder(current, out var eaten);

            if (navigator.TryGetFolder(eaten, out var found))
                navigator = found;
            else
                return false;

            if (current.IsEmpty)
                break;
        }

        folder = navigator;
        return true;

        static ReadOnlySpan<char> EatFolder(ReadOnlySpan<char> input, out ReadOnlySpan<char> folder)
        {
            int i = input.IndexOf('/');
            if (i == -1)
            {
                folder = input;
                return [];
            }

            folder = input[..i];
            return input[(i + 1)..];
        }
    }

    private AssetFolder EnsureHierarchyFolder(ReadOnlySpan<char> path)
    {
        if (path[^1] == '/')
            path = path[..^1];

        if (TryGetAssetFolder(path, out var folder))
            return folder;

        var l = path.LastIndexOf('/');

        AssetFolder? parent = null;

        if (l == -1)
            parent = hierarchyRoot;
        else
            parent = EnsureHierarchyFolder(path[..l]);

        var created = new AssetFolder(path[(l + 1)..].ToString());
        if (parent.Folders == null)
            parent.Folders = [created];
        else
            parent.Folders = [.. parent.Folders.Append(created)];
        return created;
    }


    private class AssetFolder
    {
        /// <summary>
        /// The name of this folder. NOT THE PATH!!
        /// </summary>
        public readonly string Name;

        public AssetFolder(string name)
        {
            Name = name;
        }

        public AssetFolder[]? Folders;
        public int[]? Assets;

        public bool TryGetFolder(ReadOnlySpan<char> name, [NotNullWhen(true)] out AssetFolder? f)
        {
            f = null;

            if (Folders == null)
                return false;

            foreach (var i in Folders)
                if (name.SequenceEqual(i.Name))
                {
                    f = i;
                    return true;
                }

            return false;
        }
    }
}

public class StreamSegment : Stream
{
    private readonly Stream parent;
    private readonly Range segment;

    public StreamSegment(Stream parent, Range segment)
    {
        this.parent = parent;
        this.segment = segment;
    }

    public override bool CanRead => parent.CanRead;

    public override bool CanSeek => parent.CanSeek;

    public override bool CanWrite => parent.CanWrite;

    public override long Length => parent.Length;

    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override void Flush()
    {

    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return 0;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return 0L;
    }

    public override void SetLength(long value)
    {
    }

    public override void Write(byte[] buffer, int offset, int count)
    {

    }
}

public record struct Asset
{
    public AssetMetadata Metadata;
    public Lazy<Stream> Stream;
}