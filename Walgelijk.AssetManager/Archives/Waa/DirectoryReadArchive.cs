using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Walgelijk.AssetManager.Archives.Waa;

[Experimental(nameof(DirectoryReadArchive))]
public class DirectoryReadArchive : IReadArchive
{
    public readonly DirectoryInfo Directory;

    public readonly ImmutableDictionary<string, FileInfo> Entries;
    public readonly ImmutableDictionary<string, HashSet<string>> SubDirectories;

    private readonly ConcurrentBag<Stream> ownedStreams = [];
    private readonly ImmutableDictionary<string, byte[]> metadataShadow;

    public DirectoryReadArchive(string path)
    {
        Directory = new(path);

        var allFiles = Directory.GetFiles("*", SearchOption.AllDirectories);

        var files = new Dictionary<string, FileInfo>();
        var directories = new Dictionary<string, HashSet<string>>();
        var metadataShadow = new Dictionary<string, byte[]>();

        foreach (var file in allFiles)
        {
            var key = AssetPackageUtils.NormalisePath(Path.GetRelativePath(Directory.FullName, file.FullName));
            files.Add("assets/" + key, file);

            var metadata = new AssetMetadata
            {
                Id = new AssetId(key),
                MimeType = MimeTypes.GetFromExtension(file.Extension),
                Path = key,
                Size = file.Length,
            };
            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata));
            metadataShadow.Add("metadata/" + key + ".json", bytes);
        }

        foreach (var directory in Directory.GetDirectories())
        {
            var relativePath = AssetPackageUtils.NormalisePath(Path.GetRelativePath(Directory.FullName, directory.FullName));
            var set = directories.Ensure("assets/" + relativePath);
            foreach (var subFile in directory.GetFiles("*", SearchOption.TopDirectoryOnly))
                set.Add(AssetPackageUtils.NormalisePath("assets/" + Path.GetRelativePath(Directory.FullName, subFile.FullName)));
        }

        this.Entries = files.ToImmutableDictionary();
        this.SubDirectories = directories.ToImmutableDictionary();
        this.metadataShadow = metadataShadow.ToImmutableDictionary();
    }

    public Stream? GetEntry(string path)
    {
        // we have to simulate the metadata shadow structure
        if (path.StartsWith("metadata/"))
        {
            if (metadataShadow.TryGetValue(path, out var data))
            {
                var s = new MemoryStream(data);
                ownedStreams.Add(s);
                return s;
            }
        }

        if (Entries.TryGetValue(path, out var r))
        {
            var s = new FileStream(r.FullName, FileMode.Open, FileAccess.Read);
            ownedStreams.Add(s);
            return s;
        }

        return null;
    }

    public bool HasEntry(string path) => Entries.ContainsKey(path);

    public void Dispose()
    {
        while (ownedStreams.TryTake(out var t))
            t.Dispose();
    }
}