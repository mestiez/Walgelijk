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
    /// <summary>
    /// Name of the font
    /// </summary>
    public readonly string Name;
    /// <summary>
    /// Reference to the atlas texture
    /// </summary>
    public readonly Texture Atlas;
    /// <summary>
    /// Kernings between different characters
    /// </summary>
    public readonly Kerning[] Kernings;
    /// <summary>
    /// Set of glyphs in the font
    /// </summary>
    public readonly Glyph[] Glyphs;
    /// <summary>
    /// Height of a line of text in pixels
    /// </summary>
    public readonly float LineHeight;
    /// <summary>
    /// Font size in points
    /// </summary>
    public readonly int Size;
    /// <summary>
    /// The distance from the baseline to the mean line
    /// </summary>
    public readonly float XHeight; 
    /// <summary>
    /// The maximum height of uppercase character
    /// </summary>
    public readonly float CapHeight;
    /// <summary>
    /// The style this font represents
    /// </summary>
    public readonly FontStyle Style;

    public FontFormat(
        string name,
        int size,
        float xheight,
        float capHeight,
        float lineHeight,
        Texture atlas,
        Kerning[] kernings,
        Glyph[] glyphs,
        FontStyle style = FontStyle.Regular)
    {
        Name = name;
        Atlas = atlas;
        Kernings = kernings;
        Glyphs = glyphs;
        CapHeight = capHeight;
        LineHeight = lineHeight;
        Style = style;
        Size = size;
        XHeight = xheight;
    }

    public static FontFormat Load(string path)
    {
        using var file = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var zip = new ZipArchive(file, ZipArchiveMode.Read, false);

        var atlasEntry = zip.GetEntry("atlas.png") ?? throw new Exception("Archive does not contain an atlas");
        var metaEntry = zip.GetEntry("meta.json") ?? throw new Exception("Archive does not contain metadata");

        var atlas = new byte[atlasEntry.Length];
        var atlasIndex = 0;

        using var atlasStream = atlasEntry.Open();
        while (true)
        {
            var read = atlasStream.Read(atlas, atlasIndex, Math.Min(1024, atlas.Length - atlasIndex));
            atlasIndex += read;
            if (read <= 0)
                break;
        }
        atlasStream.Dispose();

        var metadataReader = new StreamReader(metaEntry.Open());
        var json = metadataReader.ReadToEnd();
        metadataReader.Dispose();

        var metadata = JsonConvert.DeserializeObject<FontFormat>(json) ?? throw new Exception("Metadata is null");

        zip.Dispose();
        file.Dispose();

        var texture = TextureLoader.FromBytes(atlas);
        texture.FilterMode = FilterMode.Linear;

        return new FontFormat(
                metadata.Name,
                metadata.Size,
                metadata.XHeight,
                metadata.CapHeight,
                metadata.LineHeight,
                texture,
                metadata.Kernings,
                metadata.Glyphs,
                metadata.Style);
    }
}
