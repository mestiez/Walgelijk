using System.IO.Compression;
using Walgelijk.AssetManager;

namespace Walgelijk.AssetPackageBuilder;

public class Program
{
    public static void Main(string[] args)
    {
        var input = new DirectoryInfo(args[0]);
        using var output = new FileStream(input.Name + ".waa", FileMode.CreateNew);

        AssetPackageUtils.Build(input.Name, input, output);

        Console.WriteLine("Success. written to {0}", output.Name);
    }
}