using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

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

foreach (var path in GetChangedFiles())
{
	if (path.EndsWith("csproj"))
		continue;
	var p = new FileInfo(path);
	var relativeDir = Path.GetRelativePath(Environment.CurrentDirectory, p.DirectoryName);
	relativeDir = relativeDir.Split(new[]{Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar})[0];
	projectDirs.Add(relativeDir);
	Console.WriteLine("Change detected: {0} in solution {1}", path, relativeDir);
}

foreach (var project in projectDirs)
{
	var csProj = $"{project}/{project}.csproj";
	if (File.Exists(csProj))
	{
		Console.WriteLine(csProj);
		var doc = XDocument.Load(csProj);
		var versionElement = doc.Root.Element("PropertyGroup").Element("Version");
		var version = Version.Parse((string)versionElement);
		var newVersion = new Version(version.Major, version.Minor, version.Build + 1);
		Console.WriteLine("{0} >> {1}", version, newVersion);
		versionElement.SetValue(newVersion.ToString());
		doc.Save(csProj);
	}else Console.Error.WriteLine("csproj does not exist: {0}", csProj);
}