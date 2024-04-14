using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using Walgelijk.AssetManager;

namespace Tests;

[TestClass]
public class AssetPackages
{
    const string validPackage = "base.waa";

    [TestInitialize]
    public void Init()
    {
        Assert.IsTrue(File.Exists(validPackage), "These tests depend on the test archive, which isn't publicly available");
    }

    [TestMethod]
    public void LoadAssetPackage()
    {
        // we just need to make sure no exceptions are thrown
        using var package = AssetPackage.Load(validPackage);
        Assert.IsNotNull(package);
    }

    [TestMethod]
    public void EnumerateFolders()
    {
        using var package = AssetPackage.Load(validPackage);

        var content = package.EnumerateFolder("textures/ui/levelselect/").ToArray();
        AssetId[] expected = [
            new(1397134492),
            new(1971289920)
        ];

        Assert.IsTrue(expected.SequenceEqual(content), "read content does not match expected content");
    }

    [TestMethod]
    public void GetAssetPath()
    {
        using var package = AssetPackage.Load(validPackage);

        string[] paths = [
            "textures/ui/levelselect/levels_icon.png",
            "data/dj.txt",
            "sounds/swish/swish-2.wav",
            "shaders/sdf-font.frag",
        ];
        var ids = paths.Select(p => new AssetId(p)).ToArray();

        for (int i = 0; i < paths.Length; i++)
        {
            var id = ids[i];
            var asset = package.GetAsset(id);
            Assert.AreEqual(paths[i], asset.Metadata.Path);
        }
    }

    [TestMethod]
    public void ReadAssetSynchronous()
    {
        using var package = AssetPackage.Load(validPackage);

        var asset = package.GetAsset(new AssetId("data/stats/grunt.stats"));
        var expectedContent =
@"0.13.1

name Grunt

aiming_randomness 1.25
shooting_timeout 1
recoil_handling 0.4
dodge 0
panic 2
melee 0.2";

        using var actual = new StreamReader(asset.Stream.Value, encoding: Encoding.UTF8, leaveOpen: false);

        Assert.AreEqual(expectedContent, actual.ReadToEnd());
    }
}
