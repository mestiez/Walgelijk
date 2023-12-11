namespace Walgelijk.FontGenerator;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ConvertDir(new DirectoryInfo("."));
            return;
        }
        else if (args.Length == 1)
        {
            if (Directory.Exists(args[0]))
            {
                ConvertDir(new DirectoryInfo(args[0]));
                return;
            }
            else if (File.Exists(args[0]))
            {
                ConvertFile(new FileInfo(args[0]));
                return;
            }
        }
        
        Console.Error.WriteLine($"Invalid input. Usage:\nwfont [path to ttf or directory]");
    }

    private static void ConvertDir(DirectoryInfo dir)
    {
        Console.WriteLine($"Converting all valid files (otf, ttf) in {dir}...");
        var files = dir.GetFiles("*.ttf").Concat(dir.GetFiles("*.otf"));

        foreach (var item in files)
            Console.WriteLine("Found {0}", item.Name);
        Console.WriteLine();

        foreach (var item in files)
            ConvertFile(item);
    }

    private static void ConvertFile(FileInfo file)
    {
        new Generator(file).Generate();
    }
}
