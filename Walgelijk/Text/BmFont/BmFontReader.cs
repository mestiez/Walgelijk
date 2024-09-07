using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Walgelijk.BmFont;

internal static class BmFontReader
{
    private static string currentlyLoadingPath = "???";
    private static readonly Mutex mutex = new();

    public static Font LoadFromMetadata(Stream input, Texture page, string debugName)
    {
        mutex.WaitOne(); // this thing is not thread safe. honestly this whole BM font thing should be eradicated from the codebase anyway
        try
        {
            currentlyLoadingPath = debugName ?? "???";

            using var reader = new StreamReader(input);

            var lines = new List<string>();
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                    break;
                lines.Add(line);
            }
            var text = lines.ToArray();

            Font font = new Font();

            var info = GetInfo(text);

            AssignInfoToFont(font, info);

            ParsePages(font, info, page);
            ParseGlyphs(text, font, info);
            ParseKernings(text, font, info);

            var xheight = 0f;
            var capheight = 0f;
            int c1 = 0, c2 = 0;
            foreach (var item in font.Glyphs)
            {
                if (!char.IsLetter(item.Key))
                    continue;

                if (char.IsLower(item.Key))
                {
                    xheight += item.Value.GeometryRect.Height;
                    c1++;
                }
                else
                {
                    capheight += item.Value.GeometryRect.Height;
                    c2++;
                }
            }

            xheight /= c1;
            font.XHeight = (int)xheight;

            capheight /= c2;
            font.CapHeight = (int)capheight;

            foreach (var key in font.Glyphs.Keys)
            {
                var x = font.Glyphs[key];
                font.Glyphs[key] = new Glyph(
                    key,
                    x.Advance,
                    x.GeometryRect.Translate(0, -font.XHeight / 2),
                    x.TextureRect
                );
            }

            font.Material = FontMaterialCreator.CreateFor(font);
            return font;
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static void AssignInfoToFont(Font font, BmFontInfo info)
    {
        font.Name = info.Name;
        font.Size = info.Size;
        font.Bold = info.Bold;
        font.Italic = info.Italic;
        font.LineHeight = info.LineHeight;
        font.Rendering = info.Smooth ? FontRendering.SDF : FontRendering.Bitmap;
    }

    private static void ParsePages(in Font font, BmFontInfo info, Texture page)
    {
        var filterMode = info.Smooth ? FilterMode.Linear : FilterMode.Nearest;

        // we don't support multiple plages
        if (info.PageCount != 1)
            throw new Exception("Only fonts with 1 page are supported");

        //string path = Path.Combine(Path.GetDirectoryName(metadataPath) ?? string.Empty, info.PagePaths[0]);
        //var tex = Texture.Load(path, false);
        page.FilterMode = filterMode;
        page.NeedsUpdate = true;
        font.Page = page;
    }

    private static void ParseKernings(string[] text, in Font font, BmFontInfo info)
    {
        font.Kernings = [];

        var kerningLines = GetLines("kerning ", text);
        for (int i = 0; i < info.KerningCount; i++)
        {
            var line = kerningLines[i];
            Kerning kerning;

            kerning.FirstChar = (char)GetIntFrom("first", line);
            kerning.SecondChar = (char)GetIntFrom("second", line);
            kerning.Amount = GetIntFrom("amount", line);

            font.Kernings.Add(new KerningPair { Next = kerning.FirstChar, Current = kerning.SecondChar }, kerning);
        }
    }

    private static void ParseGlyphs(string[] text, in Font font, BmFontInfo info)
    {
        var atlasSize = font.Page.Size;
        font.Glyphs = [];
        var charLines = GetLines("char ", text);
        for (int i = 0; i < info.GlyphCount; i++)
        {
            var line = charLines[i];
            BmFontGlyph glyph;

            glyph.Identity = (char)GetIntFrom("id", line);
            glyph.X = GetIntFrom("x", line);
            glyph.Y = GetIntFrom("y", line);
            glyph.Width = GetIntFrom("width", line);
            glyph.Height = GetIntFrom("height", line);
            glyph.XOffset = GetIntFrom("xoffset", line);
            glyph.YOffset = GetIntFrom("yoffset", line);
            glyph.Advance = GetIntFrom("xadvance", line);
            //glyph.Page = GetIntFrom("page", line); // we don't support multiple pages, so page is always 0

            Rect geometryRect = new(glyph.XOffset, glyph.YOffset, glyph.XOffset + glyph.Width, glyph.YOffset + glyph.Height);
            Rect textureRect = new(
                (glyph.X) / atlasSize.X,
                (glyph.Y + glyph.Height) / atlasSize.Y,
                (glyph.X + glyph.Width) / atlasSize.X,
                (glyph.Y) / atlasSize.Y
                );

            font.Glyphs.Add(glyph.Identity, new Glyph(glyph.Identity, glyph.Advance, geometryRect.SortComponents(), textureRect.SortComponents()));
        }
    }

    private static string[] GetLines(string name, string[] text)
    {
        return text.Where(s => s.StartsWith(name)).ToArray();
    }

    private static string? GetStringFrom(string name, string line)
    {
        string target = name + "=";

        int index = line.IndexOf(target, StringComparison.InvariantCulture);
        if (index == -1)
        {
            Logger.Warn($"Cannot find font metadata variable \"{name}\" for \"{currentlyLoadingPath}\"", nameof(BmFontReader));
            return null;
        }
        index += target.Length;

        char delimiter = ' ';
        if (line[index] == '\"')
        {
            delimiter = '\"';
            index++;
        }

        int endIndex = line.IndexOf(delimiter, index);
        if (endIndex == -1)
            endIndex = line.Length;

        //Wat smeert C# mij nou weer aan? js looking ass
        return line[index..endIndex];
    }

    private static int GetIntFrom(string name, string line)
    {
        if (int.TryParse(GetStringFrom(name, line), out var result))
            return result;

        Logger.Warn($"Cannot parse font metadata variable \"{name}\" to integer for \"{currentlyLoadingPath}\"", nameof(BmFontReader));
        return default;
    }

    private static bool GetBoolFrom(string name, string line) => GetIntFrom(name, line) == 1;

    private static BmFontInfo GetInfo(string[] text)
    {
        BmFontInfo data;
        string info = GetLine("info");
        string common = GetLine("common");

        data.Name = GetStringFrom("face", info) ?? "Untitled";
        data.Size = GetIntFrom("size", info);
        data.Bold = GetBoolFrom("bold", info);
        data.Italic = GetBoolFrom("italic", info);
        data.Smooth = GetBoolFrom("smooth", info);

        data.Width = GetIntFrom("scaleW", common);
        data.Height = GetIntFrom("scaleH", common);
        data.PageCount = GetIntFrom("pages", common);
        data.LineHeight = GetIntFrom("lineHeight", common);
        data.Base = GetIntFrom("base", common);
        data.GlyphCount = GetIntFrom("count", GetLine("chars"));

        var kerningsLine = GetLine("kernings");
        if (kerningsLine == null)
            data.KerningCount = 0;
        else
            data.KerningCount = GetIntFrom("count", GetLine("kernings"));

        try
        {
            data.PagePaths = new string[data.PageCount];
            var pageLines = GetLines("page", text);
            for (int i = 0; i < data.PageCount; i++)
            {
                var line = pageLines[i];
                int id = GetIntFrom("id", line);
                data.PagePaths[id] = GetStringFrom("file", line) ?? throw new Exception("\"file\" property in font page is null");
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("Malformed font file: incorrect page ID");
        }

        return data;

        string GetLine(string name)
        {
            var block = text.FirstOrDefault(s => s.StartsWith(name));
            if (block == null)
            {
                Logger.Error($"Invalid font metadata file: cannot find \"{name}\" block for \"{currentlyLoadingPath}\"", nameof(BmFontReader));
                return " ";
            }
            else return block;
        }
    }
}
