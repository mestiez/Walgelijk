using System.IO;
using System.Diagnostics;

Console.WriteLine(
@"This script assumes that the API key for the source is provided by your NuGet.Config file (the one in Roaming probably).
It also assumes you have bypass access to the package repository (Cloudflare Zero Trust).

If you don't have bypass access, make a GET request that includes a valid service token to start an authenticated session, e.g:
> curl --header ""CF-Access-Client-Id: XXX.access"" --header ""CF-Access-Client-Secret: XXX"" https://nuget.studiominus.nl/
"
);

const string source = "https://nuget.studiominus.nl/v3/index.json";

foreach (var arg in Directory.EnumerateFiles(Args[0], "*.nupkg", System.IO.SearchOption.AllDirectories))
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
