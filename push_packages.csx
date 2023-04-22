using System.IO;
using System.Diagnostics;

Console.WriteLine(
@"This script assumes that the API key for the source is provided by your NuGet.Config file (the one in Roaming probably).
It also assumes you have bypass access to the package repository (Cloudflare Zero Trust)."
);

const string source = "https://nuget.studiominus.nl/v3/index.json";

// if (Args.Count == 1 && Args[0] == "-useVars")
// {
//     Console.WriteLine("Using environment variables for authentication!");
//     var clientId = Environment.GetEnvironmentVariable("CF-Access-Client-Id");
//     var clientSecret = Environment.GetEnvironmentVariable("CF-Access-Client-Secret");

//     var curl = Process.Start(
//         "curl.exe", $"-S -s --header \"CF-Access-Client-Id: {clientId}\" --header \"CF-Access-Client-Secret: {clientSecret}\" {source}");
//     curl.WaitForExit();
// }

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
