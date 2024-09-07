namespace Walgelijk.FontGenerator;

public class Program
{
    /// <summary>
    /// Converts font files to Walgelijk font archives
    /// </summary>
    /// <param name="input">Path to input file or directory</param>
    /// <param name="output">Path to output file. Does nothing if the input is a directory.</param>
    /// <param name="charset">Path to text file containing all to-be-encoded characters. ASCII by default.</param>
    /// <param name="fontSize">Font size to render the font with. Effectively determines bitmap resolution.</param>
    static void Main(string input = ".", FileInfo? output = null, FileInfo? charset = null, int fontSize = 48)
    {
        if (Directory.Exists(input))
        {
            ConvertDir(new DirectoryInfo(input), charset, fontSize);
            return;
        }
        else if (File.Exists(input))
        {
            ConvertFile(new FileInfo(input), output, charset, fontSize);
            return;
        }

        Console.Error.WriteLine($"Invalid input: input path not found");
    }

    private static void ConvertDir(DirectoryInfo dir, FileInfo? charset, int fontSize)
    {
        Console.WriteLine($"Converting all valid files (otf, ttf) in {dir}...");
        var files = dir.GetFiles("*.ttf").Concat(dir.GetFiles("*.otf"));

        foreach (var item in files)
            Console.WriteLine("Found {0}", item.Name);
        Console.WriteLine();

        foreach (var item in files)
            ConvertFile(item, null, charset, fontSize);
    }

    private static void ConvertFile(FileInfo file, FileInfo? output, FileInfo? charset, int fontSize)
    {
        if (charset != null)
            Console.WriteLine("Using charset at \"{0}\"", charset.FullName);
        new Generator(file, fontSize, charset).Generate(output);
    }
}
