using System.IO;
using System.Diagnostics;

Console.WriteLine(
@"This script assumes that the API key for the source is provided by your NuGet.Config file (the one in Roaming probably)."
);

const string source = "https://api.nuget.org/v3/index.json";

foreach (var arg in Directory.EnumerateFiles("OutputPackages", "*.nupkg", System.IO.SearchOption.AllDirectories))
{
    Console.WriteLine(arg);
    try
    {
        var process = Process.Start("nuget.exe", $"push \"{arg}\" -Source \"{source}\" -SkipDuplicate");
        process.WaitForExit();
    }
    catch (System.Exception e)
    {
        Console.WriteLine($"Failed to push {arg}: {e.Message}");
    }
}
