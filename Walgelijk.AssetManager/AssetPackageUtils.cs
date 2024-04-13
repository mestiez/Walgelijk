using Newtonsoft.Json;
using System.IO.Compression;
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

    public static void Build(string id, DirectoryInfo directory, Stream output, CompressionLevel compressionLevel = CompressionLevel.NoCompression)
    {
        const string assetRoot = "assets/";
        const string metadataRoot = "metadata/";

        using var archive = new ZipArchive(output, ZipArchiveMode.Create, true);
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
                var e = archive.CreateEntry(asset.ArchivePath, compressionLevel);
                using var s = e.Open();
                using var fileStream = file.OpenRead();
                fileStream.CopyTo(s);
            }

            // write metadata entry
            {
                var e = archive.CreateEntry(asset.MetadataPath, compressionLevel);
                using var s = e.Open();
                var json = JsonConvert.SerializeObject(asset.Metadata);
                s.Write(Encoding.UTF8.GetBytes(json));
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
            var guidTableEntry = archive.CreateEntry("guid_table.txt");
            using var s = guidTableEntry.Open();
            s.Write(Encoding.UTF8.GetBytes(guidTable.ToString()));
            s.Dispose();
        }

        // write hierarchy.txt
        {
            var hierarchyEntry = archive.CreateEntry("hierarchy.txt");
            using var s = new StreamWriter(hierarchyEntry.Open(), Encoding.UTF8, leaveOpen: false);

            foreach (var p in hierarchyMap)
            {
                var path = p.Key;
                var set = p.Value;
                s.WriteLine(path);
                foreach (var asset in set)
                    s.WriteLine("\t{0}", asset.Internal);
            }

            s.Dispose();
        }

        // write package.json
        {
            var packageJsonEntry = archive.CreateEntry("package.json");
            using var s = new StreamWriter(packageJsonEntry.Open(), Encoding.UTF8, leaveOpen: false);
            s.WriteLine(JsonConvert.SerializeObject(package));
            s.Dispose();
        }

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

                assets.Add(new IntermediateAsset
                {
                    Id = id,
                    OriginalPath = childFile.FullName,
                    ArchivePath = path,
                    AssetPath = resourcePath,
                    MetadataPath = metadataRoot + resourcePath + ".json",
                    Metadata = new AssetMetadata
                    {
                        MimeType = MimeTypes.GetFromExtension(childFile.Extension),
                        Size = childFile.Length,
                        Streamable = false,
                        Tags = []
                    }
                });

                package.Count++;
            }

            foreach (var childDir in info.GetDirectories("*", SearchOption.TopDirectoryOnly))
                ProcessDirectory(childDir, target + childDir.Name + '/');
        }
    }
}
