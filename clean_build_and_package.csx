using System.IO;
using System.Diagnostics;

const string outputPackageDir = "OutputPackages";

if (Directory.Exists(outputPackageDir ))
    Directory.Delete(outputPackageDir, true);
call("dotnet", "clean -c Release");
call("dotnet", "build -c Release");

/* Unnecessary because each project packs if it is configured to do so on build. 
If it wasn't configured to do so, it doesn't need to be packed. */
// call("dotnet", "pack -c Release");

void call(string str, string args)
{
    var process = Process.Start(str, args);
    process.WaitForExit();
}