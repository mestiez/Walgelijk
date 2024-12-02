using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Text;

namespace Walgelijk.AssetManager;

public class CommonAssetPackageProvider(IReadArchive archive) : IAssetPackageProvider
{
    public AssetPackageMetadata GetMetadata()
    {
        var metadataEntry = archive.GetEntry("package.json") ?? throw new Exception("Archive has no package.json. This asset package is invalid");
        using var e = new StreamReader(metadataEntry!, encoding: Encoding.UTF8, leaveOpen: false);
        var json = e.ReadToEnd();
        /// YOU NEED AN INTERMEDIARY FORMAT
        /// FUCKING JSON!!! FUCK!!!!!!!!!!!
        var metadata = JsonConvert.DeserializeObject<AssetPackageMetadata>(json);

        if (metadata.FormatVersion < new Version(0, 14))
        {
            // older version uses a different metadata format

            var oldTemplate = new
            {
                Id = string.Empty,
                NumericalId = 0
            };
            var old = JsonConvert.DeserializeAnonymousType(json, oldTemplate) ?? throw new Exception("Invalid package metadata. Rebuild please!");
            metadata.Id = new PackageId(old.NumericalId);
            metadata.Name = old.Id;
        }

        if (metadata.Id == PackageId.None)
            throw new Exception("Package ID is None");

        return metadata;
    }

    public ImmutableDictionary<AssetId, string> GetGuidTable()
    {
        var guidTable = new Dictionary<AssetId, string>();
        var guidTableEntry = archive.GetEntry("guid_table.txt") ?? throw new Exception("Archive has no guid_table.txt. This asset package is invalid.");
        using var e = new StreamReader(guidTableEntry!, encoding: Encoding.UTF8, leaveOpen: false);

        while (true)
        {
            var idLine = e.ReadLine();
            if (idLine == null)
                break;

            if (!AssetId.TryParse(idLine.Trim(), out var assetId))
                throw new Exception($"Id {idLine} is not a valid asset ID");

            var path = e.ReadLine() ?? throw new Exception($"Invalid GUID table: id {idLine} is missing path");

            AssetPackageUtils.AssertPathValidity(path);

            guidTable.Add(assetId, path);
        }

        return guidTable.ToImmutableDictionary();
    }

    public IEnumerator<string> GetHierarchy()
    {
        var hierarchyEntry = archive.GetEntry("hierarchy.txt") ?? throw new Exception("Archive has no hierarchy.txt. This asset package is invalid.");
        using var e = new StreamReader(hierarchyEntry!, encoding: Encoding.UTF8, leaveOpen: false);
        while (true)
        {
            var line = e.ReadLine();
            if (line == null)
                yield break;

            yield return line;
        }
    }

    public ImmutableDictionary<string, AssetId[]> GetTaggedCache()
    {
        var taggedCache = new Dictionary<string, AssetId[]>();
        var tagTableEntry = archive.GetEntry("tag_table.txt") ?? throw new Exception("Archive has no tag_table.txt. This asset package is invalid.");
        using var e = new StreamReader(tagTableEntry!, encoding: Encoding.UTF8, leaveOpen: false);

        while (true)
        {
            // TODO error handling

            var tag = e.ReadLine();

            if (tag == null)
                break;

            var count = int.Parse(e.ReadLine()!);
            var arr = new AssetId[count];
            taggedCache.AddOrSet(tag, arr);

            for (int i = 0; i < count; i++)
                arr[i] = AssetId.Parse(e.ReadLine()!);
        }

        return taggedCache.ToImmutableDictionary();
    }
}
