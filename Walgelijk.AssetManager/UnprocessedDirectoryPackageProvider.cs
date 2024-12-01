using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Walgelijk.AssetManager.Archives.Waa;

namespace Walgelijk.AssetManager;

[Experimental(nameof(DirectoryReadArchive))]
public class UnprocessedDirectoryPackageProvider(DirectoryReadArchive archive) : IAssetPackageProvider
{
    public ImmutableDictionary<AssetId, string> GetGuidTable()
    {
        var guidTable = new Dictionary<AssetId, string>();

        foreach (var item in archive.Entries)
            guidTable.Add(new AssetId(item.Key), item.Key);

        return guidTable.ToImmutableDictionary();
    }

    public IEnumerator<string> GetHierarchy()
    {
        foreach (var dir in archive.SubDirectories)
        {
            yield return dir.Key;
            yield return dir.Value.Count.ToString();
            foreach (var item in dir.Value)
                yield return new AssetId(item).Internal.ToString();
        }
    }

    public AssetPackageMetadata GetMetadata()
    {
        return new()
        {
            Id = new PackageId(archive.Directory.Name),
            Name = archive.Directory.Name,
            Count = archive.Entries.Count
        };
    }

    public ImmutableDictionary<string, AssetId[]> GetTaggedCache()
    {
        return new Dictionary<string, AssetId[]>().ToImmutableDictionary();
    }
}
