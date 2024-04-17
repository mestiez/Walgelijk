using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.IO.Hashing;
using Walgelijk.AssetManager;

namespace Tests.AssetManager;

[TestClass]
public class AssetsTests
{
    const string validPackage1 = "base.waa";
    const string validPackage2 = "koploper.waa";

    [TestInitialize]
    public void Init()
    {
        Assert.IsTrue(
            File.Exists(validPackage1) && File.Exists(validPackage2), 
            "These tests depend on the test archives, which aren't publicly available");
        Assets.ClearRegistry();
    }

    [TestMethod]
    public void RegisterPackages()
    {
        var @base = Assets.RegisterPackage(validPackage1);
        var koploper = Assets.RegisterPackage(validPackage2);

        Assert.AreEqual("base", @base.Metadata.Id);
        Assert.AreEqual("koploper", @koploper.Metadata.Id);

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void GetAsset()
    {
        Assets.RegisterPackage(validPackage1);
        Assets.RegisterPackage(validPackage2);

        var str1 = Assets.Load<byte[]>("base:shaders/sdf-font.frag");
        var str2 = Assets.Load<byte[]>("koploper:paths/safe.json");

        Assert.AreEqual(0x2CB81DDBu, Crc32.HashToUInt32(str1));
        Assert.AreEqual(0x78055C57u, Crc32.HashToUInt32(str2));

        Assets.ClearRegistry();
    }
}
