using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Text.RegularExpressions;

/*
Usage: 
	Increment version of projects with changed files (according to git)
	> dotnet script increment_version.csx

	Increment version of all projects
	> dotnet script increment_version.csx all
*/

List<string> GetChangedFiles()
{
	var changedFiles = new List<string>();

	using var process = new Process();
	process.StartInfo.FileName = "git";
	process.StartInfo.Arguments = "diff --name-only --diff-filter=ACM";
	process.StartInfo.UseShellExecute = false;
	process.StartInfo.RedirectStandardOutput = true;
	process.Start();

	string line;
	while ((line = process.StandardOutput.ReadLine()) != null)
		changedFiles.Add(line);

	process.WaitForExit();

	return changedFiles;
}

HashSet<string> projectDirs = new();
bool doAll = Args.Count == 1 && Args[0] == "all";

if (doAll)
{
	foreach (var p in Directory.EnumerateFiles(".", "*.csproj", SearchOption.AllDirectories))
	{
		if (!Regex.IsMatch(p, @"(Walgelijk)\.?\w+"))
			continue;
		var dir = Path.GetDirectoryName(p);
		projectDirs.Add(dir);
		Console.WriteLine("Project found {0}, {1}", p, dir);
	}
}
else
{
	foreach (var path in GetChangedFiles())
	{
		// if (!path.EndsWith("csproj"))
		// 	continue;
		var p = new FileInfo(path);
		var relativeDir = Path.GetRelativePath(Environment.CurrentDirectory, p.DirectoryName);
		relativeDir = relativeDir.Split(new[]{Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar})[0];
		projectDirs.Add(relativeDir);
		Console.WriteLine("Change detected: {0} in solution {1}", path, relativeDir);
	}
}

foreach (var project in projectDirs)
{
	var csProj = $"{project}/{project}.csproj";
	if (File.Exists(csProj))
	{
		Console.WriteLine(csProj);
		var doc = XDocument.Load(csProj);
		var versionElement = doc.Root.Element("PropertyGroup").Element("Version");
		if (versionElement == null)
		{
			versionElement = new XElement("Version", "1.0.0");
			doc.Root.Element("PropertyGroup").Add(versionElement);
		}

		var version = Version.Parse((string)versionElement);
		var newVersion = new Version(version.Major, version.Minor, version.Build + 1);
		Console.WriteLine("{0} >> {1}", version, newVersion);
		versionElement.SetValue(newVersion.ToString());
		doc.Save(csProj);
	}else 
		Console.Error.WriteLine("csproj does not exist: {0}", csProj);
}