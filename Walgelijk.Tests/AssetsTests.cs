using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Hashing;
using Walgelijk;
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

        Assert.AreEqual("base", @base.Metadata.Name);
        Assert.AreEqual("koploper", @koploper.Metadata.Name);

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void GetAsset()
    {
        Assets.RegisterPackage(validPackage1);
        Assets.RegisterPackage(validPackage2);

        using var str1 = Assets.Load<byte[]>("base:shaders/red.frag");
        using var str2 = Assets.Load<byte[]>("koploper:paths/safe.json");

        Assert.AreEqual(str1, Assets.Load<byte[]>("shaders/red.frag")); // we gotta check if the auto-find asset package thing works
        Assert.AreEqual(str2, Assets.Load<byte[]>("paths/safe.json"));

        // check content match
        Assert.AreEqual(0xC6F82BF0u, Crc32.HashToUInt32(str1.Value));
        Assert.AreEqual(0x78055C57u, Crc32.HashToUInt32(str2.Value));

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void ExplicitDisposal()
    {
        var @base = Assets.RegisterPackage(validPackage1);
        var koploper = Assets.RegisterPackage(validPackage2);

        using var tex1 = Assets.Load<Texture>("base:textures/door.png");
        using var tex2 = Assets.Load<Texture>("koploper:textures/bg.png");

        Assert.AreEqual(735, tex1.Value.Width);
        Assert.AreEqual(1057, tex1.Value.Height);

        Assert.AreEqual(1920, tex2.Value.Width);
        Assert.AreEqual(1080, tex2.Value.Height);

        Assert.IsTrue(@base.IsCached(tex1.Id.Internal));
        Assert.IsTrue(koploper.IsCached(tex2.Id.Internal));

        tex1.Dispose();
        tex2.Dispose();

        Assert.IsFalse(@base.IsCached(tex1.Id.Internal));
        Assert.IsFalse(koploper.IsCached(tex2.Id.Internal));

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void LifetimeOperatorDisposal()
    {
        var @base = Assets.RegisterPackage(validPackage1);
        var koploper = Assets.RegisterPackage(validPackage2);

        using var baseTex = Assets.Load<Texture>("base:textures/door.png");
        using var koploperTex = Assets.Load<Texture>("koploper:textures/bg.png");

        Assets.AssignLifetime(baseTex.Id, new TimedLifetimeOperator(TimeSpan.FromSeconds(2)));

        Assert.AreEqual(735, baseTex.Value.Width);
        Assert.AreEqual(1057, baseTex.Value.Height);

        Assert.AreEqual(1920, koploperTex.Value.Width);
        Assert.AreEqual(1080, koploperTex.Value.Height);

        Assert.IsTrue(@base.IsCached(baseTex.Id.Internal));
        Assert.IsTrue(koploper.IsCached(koploperTex.Id.Internal));

        // only dispose koploper tex
        koploperTex.Dispose();

        Assert.IsTrue(@base.IsCached(baseTex.Id.Internal));
        Assert.IsFalse(koploper.IsCached(koploperTex.Id.Internal));

        // pretend time has passed
        for (int i = 0; i < 300; i++)
            RoutineScheduler.StepRoutines(1 / 60f);

        Assert.IsFalse(@base.IsCached(baseTex.Id.Internal));
        Assert.IsFalse(koploper.IsCached(koploperTex.Id.Internal));

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void LinkedDisposal()
    {
        var @base = Assets.RegisterPackage(validPackage1);
        var koploper = Assets.RegisterPackage(validPackage2);

        using var tex1 = Assets.Load<Texture>("base:textures/door.png");
        using var tex2 = Assets.Load<Texture>("koploper:textures/bg.png");

        Assets.LinkDisposal(tex1.Id, tex2);

        Assert.AreEqual(735, tex1.Value.Width);
        Assert.AreEqual(1057, tex1.Value.Height);

        Assert.AreEqual(1920, tex2.Value.Width);
        Assert.AreEqual(1080, tex2.Value.Height);

        Assert.IsTrue(@base.IsCached(tex1.Id.Internal));
        Assert.IsTrue(koploper.IsCached(tex2.Id.Internal));

        tex1.Dispose(); // only dispose tex1, and then confirm tex2 was also disposed because they are linked

        Assert.IsFalse(@base.IsCached(tex1.Id.Internal));
        Assert.IsFalse(koploper.IsCached(tex2.Id.Internal));

        Assets.ClearRegistry();
    }
}
