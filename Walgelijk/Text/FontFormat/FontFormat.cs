using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace Walgelijk;

public class FontFormat
{
    public readonly string Name;
    public readonly Texture Atlas;
    public readonly Kerning[] Kernings;
    public readonly Glyph[] Glyphs;

    public FontFormat(string name, Texture atlas, Kerning[] kernings, Glyph[] glyphs)
    {
        Name = name;
        Atlas = atlas;
        Kernings = kernings;
        Glyphs = glyphs;
    }

    public static FontFormat Load(string path)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var zip = new ZipArchive(file, ZipArchiveMode.Read, false);

        var atlasEntry = zip.GetEntry("atlas.png") ?? throw new Exception("Archive does not contain an atlas");
        var metaEntry = zip.GetEntry("meta.json") ?? throw new Exception("Archive does not contain metadata");

        var atlas = new List<byte>();
        byte[] buffer = new byte[1024];

        using var atlasStream = atlasEntry.Open();
        while (true)
        {
            var read = atlasStream.Read(buffer, 0, 1024);
            if (read <= 0)
                break;
            for (int i = 0; i < read; i++)
                atlas.Add(buffer[i]);
        }
        atlasStream.Dispose();

        var metadataReader = new StreamReader(metaEntry.Open());
        var json = metadataReader.ReadToEnd();
        metadataReader.Dispose();

        var metadata = JsonConvert.DeserializeObject<FontFormat>(json) ?? throw new Exception("Metadata is null");

        zip.Dispose();
        file.Dispose();

        return new FontFormat(
                metadata.Name,
                TextureLoader.FromBytes(atlas.ToArray()),
                metadata.Kernings,
                metadata.Glyphs
            );
    }
}
