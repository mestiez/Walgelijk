using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Reflection;

namespace Walgelijk.FontGenerator;

public class Generator
{
    public const string intermediatePrefix = "wf-intermediate-";

    public readonly string FontName;
    public readonly FileInfo FontFile;
    public readonly int FontSize;

    private readonly string intermediateImageOut;
    private readonly string intermediateMetadataOut;
    private readonly string finalOut;

    public Generator(FileInfo fontFile, int fontSize = 72)
    {
        FontName = Path.GetFileNameWithoutExtension(fontFile.FullName).Replace(' ', '_');
        foreach (var invalid in Path.GetInvalidFileNameChars().Append('_'))
            FontName = FontName.Replace(invalid, '-');

        FontFile = fontFile;
        FontSize = fontSize;

        intermediateImageOut = Path.GetFullPath($"{intermediatePrefix}{FontName}.png");
        intermediateMetadataOut = Path.GetFullPath($"{intermediatePrefix}{FontName}.json");
        finalOut = FontFile.DirectoryName + Path.DirectorySeparatorChar + FontName + ".wf";
    }

    public void Generate()
    {
        var packageImageName = "atlas.png";
        var packageMetadataName = "meta.json";

        RunMsdfGen();
        var metadata = 
            JsonConvert.DeserializeObject<MsdfDataStructs.MsdfGenFont>(File.ReadAllText(intermediateMetadataOut)) 
            ?? throw new Exception("Exported metadata does not exist...");

        RunConvertGpos(metadata);

        using var archiveStream = new FileStream(finalOut, FileMode.Create, FileAccess.Write);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, false);

        var xheight = FontSize * (metadata.Glyphs!.Where(g => char.IsLower(g.Unicode)).Max(g => MathF.Abs(g.PlaneBounds.Top - MathF.Abs(metadata.Metrics.Descender))));
        var capheight = FontSize * (metadata.Glyphs!.Where(g => char.IsUpper(g.Unicode)).Max(g => MathF.Abs(g.PlaneBounds.Top - MathF.Abs(metadata.Metrics.Descender))));

        var final = new FontFormat(
            name: FontName,
            style: FontStyle.Regular,
            size: FontSize,
            xheight: xheight,
            atlas: null!, // will be loaded by game engine or whatever software interprets this file
            capHeight: capheight,
            lineHeight: metadata.Metrics.LineHeight * FontSize,
            kernings: (metadata.Kerning?.Select(a => new Kerning { Amount = a.Advance, FirstChar = a.Unicode1, SecondChar = a.Unicode2 }).ToArray())!,
            glyphs: (metadata.Glyphs?.Select(g => new Glyph(
                character: g.Unicode,
                advance: g.Advance * FontSize,
                textureRect: AbsoluteToTexcoords(g.AtlasBounds.GetRect(), new Vector2(metadata.Atlas.Width, metadata.Atlas.Height)),
                geometryRect: TransformGeometryRect(g.PlaneBounds.GetRect()).Translate(0, xheight)
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
        var charsetPath = Path.Combine(execDir, "charset.txt");
        process.StartInfo = new ProcessStartInfo(processPath,
            $"-font \"{FontFile.FullName}\" -size {FontSize} -charset \"{charsetPath}\" -format png -pots -imageout \"{intermediateImageOut}\" -json \"{intermediateMetadataOut}\"")
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

    private void RunConvertGpos(MsdfDataStructs.MsdfGenFont msdfGenFont)
    {
        using var process = new Process();
        var execDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var processPath = Path.Combine(execDir, "ConvertGpos/");
        var intermediatePath = Path.GetFullPath("kerning_intermediate.json");
        process.StartInfo = new ProcessStartInfo("cmd")
        {
            RedirectStandardError = true,
            RedirectStandardInput = true,
            WorkingDirectory = processPath
        };
        Console.WriteLine("Starting ConvertGpos...");
        process.ErrorDataReceived += (o, e) =>
        {
            throw new Exception(e.Data);
        };
        process.Start();
        process.StandardInput.WriteLine($"npm run getKerning \"{FontFile.FullName}\" \"{intermediatePath}\" & exit");
        process.WaitForExit();

        Console.WriteLine(process.StandardError.ReadToEnd());

        var json = File.ReadAllText(intermediatePath);
        var arr = JsonConvert.DeserializeObject<MsdfDataStructs.MsdfKerning[]>(json);
        if (arr == null)
            return;

        for (int i = 0; i < arr.Length; i++)
            arr[i].Advance *= FontSize;

        if (msdfGenFont.Kerning == null)
            msdfGenFont.Kerning = arr.Distinct().ToArray();
        else
            msdfGenFont.Kerning = arr.Concat(msdfGenFont.Kerning).Distinct().ToArray();

        Console.WriteLine($"ConvertGpos complete ({msdfGenFont.Kerning.Length} kerning pairs generated)");
        File.Delete(intermediatePath);
    }
}
