using Newtonsoft.Json;
using System.Formats.Tar;
using System.Text;

namespace Walgelijk.AssetManager;

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
     *  | 📃 hierarchy.txt
     *  | 📃 package.json
     */

    private struct IntermediateAsset
    {
        public AssetId Id;

        public AssetMetadata Metadata;

        /// <summary>
        /// Path in the archive
        /// </summary>
        public string ArchivePath;

        /// <summary>
        /// Path within the asset directory in the archive
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// Original path on disk
        /// </summary>
        public string OriginalPath;

        /// <summary>
        /// Path to the metadata within the archive metadata root
        /// </summary>
        public string MetadataPath;
    }

    public static string NormalisePath(ReadOnlySpan<char> path)
    {
        return path.Trim().ToString().Replace('\\', '/');
    }

    public static void AssertPathValidity(ReadOnlySpan<char> path)
    {
        if (path.Length != path.Trim().Length)
            throw new Exception("Paths cannot have trailing or leading whitespace");

        for (int i = 0; i < path.Length; i++)
        {
            var c = path[i];

            if (c == '\\')
                throw new Exception("Backslashes are illegal");

            if (c == ':')
                throw new Exception("Colons are illegal");

            if (!char.IsAscii(c))
                throw new Exception($"Illegal character {c}. Only ASCII is allowed in paths");
        }
    }

    public static void Build(string id, DirectoryInfo directory, Stream output)
    {
        const string assetRoot = "assets/";
        const string metadataRoot = "metadata/";

        using var archive = new TarWriter(output, TarEntryFormat.Pax, true);
        var guidsIntermediate = new HashSet<int>();
        var guidTable = new StringBuilder();
        var hierarchyMap = new Dictionary<string, List<AssetId>>();
        var assets = new HashSet<IntermediateAsset>();

        var package = new AssetPackageMetadata
        {
            Id = id
        };

        // process root directory
        ProcessDirectory(directory, assetRoot);

        Console.WriteLine("Found {0} assets", package.Count);

        // write all assets
        int i = 1;
        foreach (var asset in assets)
        {
            var file = new FileInfo(asset.OriginalPath);

            // create and write entry
            {
                var e = new PaxTarEntry(TarEntryType.RegularFile, asset.ArchivePath);
                e.DataStream = file.OpenRead();
                archive.WriteEntry(e);
            }

            // write metadata entry
            {
                var e = new PaxTarEntry(TarEntryType.RegularFile, asset.MetadataPath);
                e.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(asset.Metadata)));
                archive.WriteEntry(e);
            }

            // write guid to table
            guidTable.AppendLine(asset.Id.Internal.ToString());
            guidTable.AppendLine(asset.AssetPath);

            // write guid to table
            var hSet = hierarchyMap.Ensure(NormalisePath(Path.GetDirectoryName(asset.AssetPath)));
            hSet.Add(asset.Id);

            Console.WriteLine("{0}/{1}\t{2}%\t{3}", 
                i.ToString("D" + (int)(float.Log10(package.Count) + 1)), package.Count, 
                (int)(float.Floor(i / (float)package.Count * 100)), 
                asset.AssetPath);

            i++;
        }

        // write guid_table.txt
        {
            var guidTableEntry = new PaxTarEntry(TarEntryType.RegularFile, "guid_table.txt");
            guidTableEntry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(guidTable.ToString()), true);
            archive.WriteEntry(guidTableEntry);
            Console.WriteLine($"guid_table.txt written {guidTableEntry.Length}");
        }

        // write hierarchy.txt
        {
            var hierarchyEntry = new PaxTarEntry(TarEntryType.RegularFile, "hierarchy.txt");
            var s = new StringBuilder();
            foreach (var p in hierarchyMap)
            {
                var path = p.Key;
                var set = p.Value;
                s.AppendLine(path);
                s.AppendLine(set.Count.ToString());
                foreach (var asset in set)
                    s.AppendFormat("\t{0}\n", asset.Internal);
            }
            hierarchyEntry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(s.ToString()));
            archive.WriteEntry(hierarchyEntry);
            Console.WriteLine($"hierarchy.txt written {hierarchyEntry.Length}");
        }

        // write package.json
        {
            var packageJsonEntry = new PaxTarEntry(TarEntryType.RegularFile, "package.json");
            packageJsonEntry.DataStream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(package)));
            archive.WriteEntry(packageJsonEntry);
            Console.WriteLine($"package.json written {packageJsonEntry.Length}");
        }

        archive.Dispose();

        void ProcessDirectory(DirectoryInfo info, string target)
        {
#if DEBUG
            global::System.Diagnostics.Debug.Assert(target.EndsWith('/'));
#endif
            foreach (var childFile in info.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                var path = target + childFile.Name;

                var resourcePath = NormalisePath(Path.GetRelativePath(assetRoot, path));
                var id = new AssetId(resourcePath);

                if (!guidsIntermediate.Add(id.Internal))
                    throw new Exception($"Resource at \"{resourcePath}\" collides with another resource.");

                var contentHash = global::System.IO.Hashing.XxHash3.Hash(File.ReadAllBytes(childFile.FullName));

                assets.Add(new IntermediateAsset
                {
                    Id = id,
                    OriginalPath = childFile.FullName,
                    ArchivePath = path,
                    AssetPath = resourcePath,
                    MetadataPath = metadataRoot + resourcePath + ".json",
                    Metadata = new AssetMetadata
                    {
                        Id = id,
                        Path = resourcePath,
                        MimeType = MimeTypes.GetFromExtension(childFile.Extension),
                        Size = childFile.Length,
                        Streamable = false,
                        XXH3 = string.Join(null, contentHash.Select(static b => b.ToString("x2"))),
                        Tags = []
                    }
                }) ;

                package.Count++;
            }

            foreach (var childDir in info.GetDirectories("*", SearchOption.TopDirectoryOnly))
                ProcessDirectory(childDir, target + childDir.Name + '/');
        }
    }
}
