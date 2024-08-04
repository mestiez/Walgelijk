using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using Walgelijk.AssetManager.Archives.Waa;

namespace Walgelijk.AssetManager;

public static class AssetPackageUtils
{
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

    public static void Build(string id, DirectoryInfo directory, Stream output, IAssetBuilderProcessor? preprocessor = null)
    {
        Build(id, directory, output, preprocessor == null ? null : [preprocessor]);
    }

    public static void Build(string id, DirectoryInfo directory, Stream output, IAssetBuilderProcessor[]? preprocessors)
    {
        if (id.Contains(':'))
            throw new Exception("Package ID may not contain a colon");

        const string assetRoot = "assets/";
        const string metadataRoot = "metadata/";

        using var archive = new WaaWriteArchive(output);
        var guidsIntermediate = new HashSet<int>();
        var guidTable = new StringBuilder();
        var tagTable = new StringBuilder();
        var hierarchyMap = new Dictionary<string, List<AssetId>>();
        var tagTableMap = new Dictionary<string, List<AssetId>>();
        var assets = new HashSet<IntermediateAsset>();

        var package = new AssetPackageMetadata
        {
            Name = id,
            Id = new(id),
            EngineVersion = Assembly.GetAssembly(typeof(Game))!.GetName()!.Version!,
            FormatVersion = Assembly.GetAssembly(typeof(AssetPackageUtils))!.GetName()!.Version!,
        };

        // process root directory
        ProcessDirectory(directory, assetRoot, null, null);

        Console.WriteLine("Found {0} assets", package.Count);

        // write all assets
        int i = 1;
        foreach (var asset in assets)
        {
            var file = new FileInfo(asset.OriginalPath);

            // create and write entry
            archive.WriteEntry(
                asset.ArchivePath,
                file.OpenRead());

            // write metadata entry
            archive.WriteEntry(
                asset.MetadataPath,
                new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(asset.Metadata))));

            // write guid to table
            guidTable.AppendLine(asset.Id.ToString());
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
            var l = archive.WriteEntry("guid_table.txt", new MemoryStream(Encoding.UTF8.GetBytes(guidTable.ToString()), true));
            Console.WriteLine($"guid_table.txt written {l}");
        }

        // write tag_table.txt
        {
            foreach (var p in tagTableMap)
            {
                tagTable.AppendLine(p.Key);
                tagTable.AppendLine(p.Value.Count.ToString());
                foreach (var a in p.Value)
                    tagTable.AppendLine(a.ToString());
            }

            var l = archive.WriteEntry("tag_table.txt", new MemoryStream(Encoding.UTF8.GetBytes(tagTable.ToString()), true));
            Console.WriteLine($"tag_table.txt written {l}");
        }

        // write hierarchy.txt
        {
            var s = new StringBuilder();
            foreach (var p in hierarchyMap)
            {
                var path = p.Key;
                var set = p.Value;
                s.AppendLine(path);
                s.AppendLine(set.Count.ToString());
                foreach (var asset in set)
                    s.AppendFormat("\t{0}\n", asset.ToString());
            }
            var l = archive.WriteEntry("hierarchy.txt", new MemoryStream(Encoding.UTF8.GetBytes(s.ToString())));
            Console.WriteLine($"hierarchy.txt written {l}");
        }

        // write package.json
        {
            var l = archive.WriteEntry("package.json", new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(package))));
            Console.WriteLine($"package.json written {l}");
        }

        void ProcessDirectory(DirectoryInfo info, string target, string? parentMime, string[]? parentTags)
        {
#if DEBUG
            global::System.Diagnostics.Debug.Assert(target.EndsWith('/'));
#endif

            var globalMimeTypeFile = info.EnumerateFiles(".mimetype").FirstOrDefault();
            var globalTagsFile = info.EnumerateFiles(".tags").FirstOrDefault();

            string? globalMimeType = globalMimeTypeFile == null ? parentMime : File.ReadAllText(globalMimeTypeFile.FullName);
            string[]? globalTags = globalTagsFile == null ? parentTags : File.ReadAllLines(globalTagsFile.FullName);

            foreach (var childFile in info.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
                if (childFile.Name.EndsWith(".mimetype"))
                    continue;

                if (childFile.Name.EndsWith(".tags"))
                    continue;

                var path = target + childFile.Name;

                var resourcePath = NormalisePath(Path.GetRelativePath(assetRoot, path));
                var id = new AssetId(resourcePath);

                if (id == AssetId.None)
                    throw new Exception($"Resource at \"{resourcePath}\" generates reserved ID zero. This may be solved by renaming the file, but you really shouldn't be seeing this so it's probably a bug.");

                if (!guidsIntermediate.Add(id.Internal))
                    throw new Exception($"Resource at \"{resourcePath}\" collides with another resource. Rename or relocate.");

                var mimeTypeFile = info.EnumerateFiles(childFile.Name + ".mimetype").FirstOrDefault();
                var tagsFile = info.EnumerateFiles(childFile.Name + ".tags").FirstOrDefault();

                string? mimeType = mimeTypeFile == null ? null : File.ReadAllText(mimeTypeFile.FullName);
                string[] tags = tagsFile == null ? [] : File.ReadAllLines(tagsFile.FullName);

                if (globalTags != null)
                    tags = [.. tags, .. globalTags];

                var contentData = File.ReadAllBytes(childFile.FullName);
                var contentHash = global::System.IO.Hashing.XxHash3.Hash(contentData);

                var metadata = new AssetMetadata
                {
                    Id = id,
                    Path = resourcePath,
                    MimeType = mimeType ?? globalMimeType ?? MimeTypes.GetFromExtension(childFile.Extension),
                    Size = childFile.Length,
                    XXH3 = string.Join(null, contentHash.Select(static b => b.ToString("X2"))),
                    Tags = tags
                };

                if (preprocessors != null)
                {
                    var readonlyData = contentData.AsSpan();
                    foreach (var p in preprocessors)
                        try
                        {
                            p?.Process(ref metadata, readonlyData);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine("Asset builder processor {0} threw an exception {1}", p.GetType(), e);
                        }
                }

                if (metadata.Tags != null)
                    foreach (var tag in metadata.Tags)
                        tagTableMap.Ensure(tag).Add(id);

                assets.Add(new IntermediateAsset
                {
                    Id = id,
                    OriginalPath = childFile.FullName,
                    ArchivePath = path,
                    AssetPath = resourcePath,
                    MetadataPath = metadataRoot + resourcePath + ".json",
                    Metadata = metadata
                });

                package.Count++;
            }

            foreach (var childDir in info.GetDirectories("*", SearchOption.TopDirectoryOnly))
                ProcessDirectory(childDir, target + childDir.Name + '/', globalMimeType, globalTags);
        }
    }
}
