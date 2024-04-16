using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.IO.Compression;
using System.Text;

namespace Walgelijk.AssetManager;

public class AssetPackage : IDisposable
{
    public readonly AssetPackageMetadata Metadata;
    public readonly TarReader Archive;

    private readonly ImmutableDictionary<int, string> guidTable;
    private readonly AssetFolder hierarchyRoot = new("/");

    private readonly ConcurrentDictionary<AssetId, object> cache = [];
    private readonly ConcurrentDictionary<string, TarEntry> entries = [];

    private readonly SemaphoreSlim assetReadingLock = new(0);

    public AssetPackage(Stream input)
    {
        var guidTable = new Dictionary<int, string>();
        var archive = Archive = new TarReader(input, false);

        while (true)
        {
            var e = archive.GetNextEntry();
            if (e == null)
                break;

            entries.TryAdd(e.Name, e);
        }

        // read metadata
        {
            var metadataEntry = getEntry("package.json") ?? throw new Exception("Archive has no package.json. This asset package is invalid");
            using var e = new StreamReader(metadataEntry.DataStream!, encoding: Encoding.UTF8, leaveOpen: false);
            Metadata = JsonConvert.DeserializeObject<AssetPackageMetadata>(e.ReadToEnd());
        }

        // read guid table
        {
            var guidTableEntry = getEntry("guid_table.txt") ?? throw new Exception("Archive has no guid_table.txt. This asset package is invalid.");
            using var e = new StreamReader(guidTableEntry.DataStream!, encoding: Encoding.UTF8, leaveOpen: false);

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

            this.guidTable = guidTable.ToImmutableDictionary();
        }

        // read hierarchy
        {
            var hierarchtEntry = getEntry("hierarchy.txt") ?? throw new Exception("Archive has no hierarchy.txt. This asset package is invalid.");
            using var e = new StreamReader(hierarchtEntry.DataStream!, encoding: Encoding.UTF8, leaveOpen: false);

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

        assetReadingLock.Release();
    }

    TarEntry getEntry(string path) => entries[path];

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
            return EnumerateFolder(folder, searchOption);
        throw new Exception($"Path \"{path}\" cannot be found");
    }

    public Asset GetAsset(in AssetId id)
    {
        var path = GetAssetPath(id);

        var entry = getEntry("assets/" + path);
        if (entry == null)
            throw new Exception($"Asset {id.Internal} at path \"{path}\" not found. This indicates the archive is malformed.");
        // it is malformed because the guid table is pointing to entries that don't exist

        return new Asset
        {
            Metadata = GetAssetMetadata(id), // TODO we are doing unnecessary work by calling this function (double GetAssetPath)
            Stream = new Lazy<Stream>(() => entry.DataStream!),
        };
    }
    public string GetAssetPath(in AssetId id)
    {
        if (guidTable.TryGetValue(id.Internal, out var path))
            return path;

        throw new Exception($"Asset {id.Internal} does not exist.");
    }

    public T Load<T>(in string path) => Load<T>(new AssetId(path));

    public T Load<T>(in AssetId id)
    {
        assetReadingLock.Wait();
        try
        {
            if (cache.TryGetValue(id, out var obj))
            {
                if (obj is T t)
                    return t;
                else
                    throw new Exception($"Asset {id.Internal} was previously loaded as type {obj.GetType()}, this does not match the requested type {typeof(T)}");
            }
            var a = AssetDeserialisers.Load<T>(GetAsset(id))
                ?? throw new NullReferenceException($"Deserialising asset {id.Internal} returns null");
            if (!cache.TryAdd(id, a))
                throw new Exception("");
            return a;
        }
        finally
        {
            assetReadingLock.Release();
        }
    }

    public void Dispose()
    {
        foreach (var v in cache.Values)
        {
            if (v is IDisposable a)
                a.Dispose();
            else if (v is IAsyncDisposable b)
                Task.Run(b.DisposeAsync).RunSynchronously();
        }

        cache.Clear();
        Archive.Dispose();
        assetReadingLock.Dispose();
    }

    public static AssetPackage Load(string path)
    {
        var file = new FileStream(path, FileMode.Open);
        return new AssetPackage(file);
    }

    private AssetMetadata GetAssetMetadata(in AssetId id)
    {
        var path = GetAssetPath(id);
        var entry = getEntry("metadata/" + path + ".json");
        if (entry == null)
            throw new Exception($"Asset metadata {id.Internal} at path \"{path}\" not found. This indicates the archive is malformed.");
        // it is malformed because the guid table is pointing to entries that don't exist

        using var reader = new StreamReader(entry.DataStream!, encoding: Encoding.UTF8, leaveOpen: false);
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

/// <summary>
/// Struct that provides objects to access asset data
/// </summary>
public record struct Asset
{
    public AssetMetadata Metadata;
    public Lazy<Stream> Stream;
}