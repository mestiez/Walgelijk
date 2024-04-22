using System.Collections.Concurrent;
using System.Formats.Tar;

namespace Walgelijk.AssetManager;

public class TarReadArchive : IReadArchive
{
    private readonly ConcurrentDictionary<string, TarEntry> entries = [];
    private readonly TarReader reader;

    public TarReadArchive(Stream input)
    {
        reader = new TarReader(input, false);
        while (true)
        {
            var e = reader.GetNextEntry();
            if (e == null)
                break;

            entries.TryAdd(e.Name, e);
        }
    }

    public void Dispose()
    {
        reader.Dispose();
    }

    public Stream? GetEntry(string path)
    {
        if (entries.TryGetValue(path,out var entry))
            return entry.DataStream ?? new MemoryStream();
        return null;
    }

    public bool HasEntry(string path) => entries.ContainsKey(path);
}

public class TarWriteArchive : IWriteArchive
{
    private readonly TarWriter writer;

    public TarWriteArchive(Stream output)
    {
        writer = new TarWriter(output, false);
    }

    public void Dispose()
    {
        writer.Dispose();
    }

    public long WriteEntry(string path, Stream stream)
    {
        var p = new PaxTarEntry(TarEntryType.RegularFile, path);
        p.DataStream = stream;
        writer.WriteEntry(p);
        return p.Length;
    }
}