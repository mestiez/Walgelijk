using HarfBuzzSharp;
using Newtonsoft.Json;
using SkiaSharp;
using System.CommandLine.Rendering;
using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Walgelijk.FontGenerator;

public class Generator
{
    public const string intermediatePrefix = "wf-intermediate-";

    public readonly string FontName;
    public readonly FileInfo FontFile;
    public readonly FileInfo CharsetFile;
    public readonly int FontSize;

    private readonly string intermediateImageOut;
    private readonly string intermediateMetadataOut;

    private static readonly char[] vowels = "aeiou".ToCharArray();
    private FileInfo file;

    public Generator(FileInfo fontFile, int fontSize = 48, FileInfo? charset = null)
    {
        FontName = Path.GetFileNameWithoutExtension(fontFile.FullName).Replace(' ', '_');
        foreach (var invalid in Path.GetInvalidFileNameChars().Append('_'))
            FontName = FontName.Replace(invalid, '-');

        FontFile = fontFile;
        FontSize = fontSize;

        intermediateImageOut = Path.GetFullPath($"{intermediatePrefix}{FontName}.png");
        intermediateMetadataOut = Path.GetFullPath($"{intermediatePrefix}{FontName}.json");
        CharsetFile = charset ?? new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!, "charset.txt"));
    }

    public void Generate(FileInfo? output)
    {
        var packageImageName = "atlas.png";
        var packageMetadataName = "meta.json";
        var finalOut = output?.FullName ?? (FontFile.DirectoryName + Path.DirectorySeparatorChar + FontName + ".wf");

        RunMsdfGen();
        var metadata =
            JsonConvert.DeserializeObject<MsdfDataStructs.MsdfGenFont>(File.ReadAllText(intermediateMetadataOut))
            ?? throw new Exception("Exported metadata does not exist...");

        GetExtraData(metadata);

        using var archiveStream = new FileStream(finalOut, FileMode.Create, FileAccess.Write);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        var vowelGlyphs = metadata.Glyphs!.Where(c => vowels.Contains(char.ToLower(c.Unicode)));
        var xheight = FontSize * (vowelGlyphs.Where(g => char.IsLower(g.Unicode)).Average(g => MathF.Abs(g.PlaneBounds.Top - g.PlaneBounds.Bottom)));
        var capheight = FontSize * (vowelGlyphs.Where(g => char.IsUpper(g.Unicode)).Average(g => MathF.Abs(g.PlaneBounds.Top - g.PlaneBounds.Bottom)));

        var final = new FontFormat(
            name: FontName,
            style: FontStyle.Regular,
            size: FontSize,
            xheight: xheight,
            atlas: null!, // will be loaded by game engine or whatever software interprets this file
            capHeight: capheight,
            lineHeight: metadata.Metrics.LineHeight * FontSize,
            kernings: (metadata.Kerning?
                .Select(a => new Kerning { Amount = a.Advance, FirstChar = a.Unicode1, SecondChar = a.Unicode2 })
                .Where(k => float.Abs(k.Amount) > float.Epsilon).ToArray())!,
            glyphs: (metadata.Glyphs?.Select(g => new Glyph(
                character: g.Unicode,
                advance: g.Advance * FontSize,
                textureRect: AbsoluteToTexcoords(g.AtlasBounds.GetRect(), new Vector2(metadata.Atlas.Width, metadata.Atlas.Height)),
                geometryRect: TransformGeometryRect(g.PlaneBounds.GetRect()).Translate(0, capheight)
            )).ToArray())!);

        using var metadataEntry = new StreamWriter(archive.CreateEntry(packageMetadataName, CompressionLevel.Fastest).Open());
        metadataEntry.Write(JsonConvert.SerializeObject(final));
        metadataEntry.Dispose();

        archive.CreateEntryFromFile(intermediateImageOut, packageImageName, CompressionLevel.Fastest);

        archive.Dispose();
        archiveStream.Dispose();

        File.Delete(intermediateImageOut);
        File.Delete(intermediateMetadataOut);

        Console.WriteLine($"Font archive written to {finalOut} ({final.Glyphs.Length} glyphs)");
    }

    private static Rect AbsoluteToTexcoords(Rect rect, Vector2 size)
    {
        rect.MinX /= size.X;
        rect.MinY /= size.Y;
        rect.MaxX /= size.X;
        rect.MaxY /= size.Y;

        return rect;
    }

    private Rect TransformGeometryRect(Rect rect)
    {
        rect.MinX *= FontSize;
        rect.MinY *= -FontSize;
        rect.MaxX *= FontSize;
        rect.MaxY *= -FontSize;

        return rect;
    }

    private void RunMsdfGen()
    {
        using var process = new Process();
        var execDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var processPath = Path.Combine(execDir, "msdf-atlas-gen");

        // transform the charset into the questionable format for msdfgen
        var chars = string.Join(", ", File.ReadAllText(CharsetFile.FullName).Select(c => $"{Convert.ToInt32(c)}"));
        var tmp = Path.GetTempFileName();
        File.WriteAllText(tmp, chars);

        process.StartInfo = new ProcessStartInfo(processPath,
            $"-font \"{FontFile.FullName}\" -type msdf -outerempadding 0.1 -size {FontSize} -charset \"{tmp}\" -format png -potr -imageout \"{intermediateImageOut}\" -json \"{intermediateMetadataOut}\"")
        {
            RedirectStandardError = true
        };
        Console.WriteLine("Starting msdf-atlas-gen...");
        process.Start();
        process.WaitForExit();
        while (!process.StandardError.EndOfStream)
            Console.WriteLine(process.StandardError.ReadLine());
        Console.WriteLine("msdf-atlas-gen complete");
    }

    private void GetExtraData(MsdfDataStructs.MsdfGenFont msdfGenFont)
    {
        var gatheredKernings = new HashSet<MsdfDataStructs.MsdfKerning>();

        {
            var execDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
            var charset = File.ReadAllText(CharsetFile.FullName).Distinct().ToArray();

            using var blob = Blob.FromFile(FontFile.FullName);
            using var face = new Face(blob, 0);
            using var font = new HarfBuzzSharp.Font(face);
            font.SetFunctionsOpenType();

            float scaling = FontSize / (float)face.UnitsPerEm;

            int i = 0;
            int totalPairCount = charset.Length * charset.Length;
            var glyphWidths = new float[2];
            var glyphBounds = new SKRect[2];

            for (int o = 0; o < charset.Length; o++)
                for (int p = 0; p < charset.Length; p++)
                {
                    var a = charset[o];
                    var b = charset[p];

                    var codepointA = new Rune(a).Value;
                    var codepointB = new Rune(b).Value;

                    if (!font.TryGetGlyph(codepointA, out var glyphA))
                        continue;
                    if (!font.TryGetGlyph(codepointB, out var glyphB))
                        continue;

                    var k = font.GetHorizontalGlyphKerning(glyphA, glyphB) * scaling;

                    {
                        using var buffer = new HarfBuzzSharp.Buffer();
                        buffer.Direction = Direction.LeftToRight;
                        buffer.Script = Script.Latin;
                        buffer.Language = new Language("en");
                        buffer.AddUtf16($"{a}{b}");

                        font.Shape(buffer);

                        var infos = buffer.GetGlyphInfoSpan();
                        var positions = buffer.GetGlyphPositionSpan();

                        if (positions.Length != 2) // some ligature type shit is going on
                        {
                            // we do nothing...
                        }
                        else
                        {
                            var advance = (positions[0].XAdvance + positions[1].XOffset);
                            var diff = advance - font.GetHorizontalGlyphAdvance(glyphA);
                            if (float.Abs(diff) > float.Abs(k))
                                k = diff * scaling;
                        }
                    }

                    if (float.Abs(k) > 0)
                    {
                        gatheredKernings.Add(new MsdfDataStructs.MsdfKerning
                        {
                            Unicode1 = a,
                            Unicode2 = b,
                            Advance = k
                        });
                    }

                    var ocp = Console.CursorLeft;
                    Console.Write("{0}/{1}", i, totalPairCount);
                    Console.CursorLeft = ocp;
                    i++;
                }
            Console.WriteLine();
        }

        if (msdfGenFont.Kerning == null)
            msdfGenFont.Kerning = [.. gatheredKernings];
        else
            msdfGenFont.Kerning = [.. gatheredKernings.Union(msdfGenFont.Kerning)];

        Console.WriteLine($"Kerning read complete ({msdfGenFont.Kerning.Length} kerning pairs generated)");
    }
}
