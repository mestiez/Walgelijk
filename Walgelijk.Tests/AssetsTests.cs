using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Hashing;
using System.Threading;
using Walgelijk;
using Walgelijk.AssetManager;

namespace Tests.AssetManager;

[TestClass]
public class AssetsTests
{
    const string validPackage1 = "assetspack_test1.waa";
    const string validPackage2 = "assetspack_test2.waa";

    private SemaphoreSlim useIdUtil = new(1);

    [TestInitialize]
    public void Init()
    {
        Assert.IsTrue(File.Exists(validPackage1) && File.Exists(validPackage2));
        Assets.ClearRegistry();
    }

    [TestMethod]
    public void RegisterPackages()
    {
        var pack1 = Assets.RegisterPackage(validPackage1);
        var pack2 = Assets.RegisterPackage(validPackage2);

        Assert.AreEqual("assetspack_test1", pack1.Metadata.Name);
        Assert.AreEqual("assetspack_test2", pack2.Metadata.Name);

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void VanillaPackageNamedString()
    {
        useIdUtil.Wait();
        var b = Assets.RegisterPackage(validPackage1);
        try
        {
            var id = new GlobalAssetId("assetspack_test1:textures/cat.png");

            var beforeVanilla = id.ToNamedString();
            IdUtil.VanillaPackageIds.Add(b.Metadata.Id);
            var afterVanilla = id.ToNamedString();

            Assert.AreNotEqual(beforeVanilla, afterVanilla);
            Assert.AreEqual("assetspack_test1:textures/cat.png", beforeVanilla);
            Assert.AreEqual("textures/cat.png", afterVanilla);
        }
        finally
        {
            IdUtil.VanillaPackageIds.Clear();
            useIdUtil.Release();
            Assets.ClearRegistry();
        }
    }

    [TestMethod]
    public void GetAsset()
    {
        Assets.RegisterPackage(validPackage1);
        Assets.RegisterPackage(validPackage2);

        using var str1 = Assets.Load<byte[]>("assetspack_test1:shaders/red.frag");
        using var str2 = Assets.Load<byte[]>("assetspack_test2:events/winter_festival.json");

        Assert.AreEqual(str1, Assets.Load<byte[]>("shaders/red.frag")); // we gotta check if the auto-find asset package thing works
        Assert.AreEqual(str2, Assets.Load<byte[]>("events/winter_festival.json"));

        // check content match
        Assert.AreEqual(0xC6F82BF0u, Crc32.HashToUInt32(str1.Value));
        Assert.AreEqual(0x7828F8B1u, Crc32.HashToUInt32(str2.Value));

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void ExplicitDisposal()
    {
        var pack1 = Assets.RegisterPackage(validPackage1);
        var pack2 = Assets.RegisterPackage(validPackage2);

        using var tex1 = Assets.Load<Texture>("assetspack_test1:textures/dog.png");
        using var tex2 = Assets.Load<Texture>("assetspack_test2:textures/space.jpg");

        Assert.AreEqual(828, tex1.Value.Width);
        Assert.AreEqual(826, tex1.Value.Height);

        Assert.AreEqual(1920, tex2.Value.Width);
        Assert.AreEqual(1256, tex2.Value.Height);

        Assert.IsTrue(pack1.IsCached(tex1.Id.Internal));
        Assert.IsTrue(pack2.IsCached(tex2.Id.Internal));

        tex1.Dispose();
        tex2.Dispose();

        Assert.IsFalse(pack1.IsCached(tex1.Id.Internal));
        Assert.IsFalse(pack2.IsCached(tex2.Id.Internal));

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void LifetimeOperatorDisposal()
    {
        var pack1 = Assets.RegisterPackage(validPackage1);
        var pack2 = Assets.RegisterPackage(validPackage2);

        using var tex1 = Assets.Load<Texture>("assetspack_test1:textures/dog.png");
        using var tex2 = Assets.Load<Texture>("assetspack_test2:textures/space.jpg");

        Assets.AssignLifetime(tex1.Id, new TimedLifetimeOperator(TimeSpan.FromSeconds(2)));

        Assert.AreEqual(828, tex1.Value.Width);
        Assert.AreEqual(826, tex1.Value.Height);

        Assert.AreEqual(1920, tex2.Value.Width);
        Assert.AreEqual(1256, tex2.Value.Height);

        Assert.IsTrue(pack1.IsCached(tex1.Id.Internal));
        Assert.IsTrue(pack2.IsCached(tex2.Id.Internal));

        // only dispose tex2
        tex2.Dispose();

        Assert.IsTrue(pack1.IsCached(tex1.Id.Internal));
        Assert.IsFalse(pack2.IsCached(tex2.Id.Internal));

        // pretend time has passed
        for (int i = 0; i < 300; i++)
            RoutineScheduler.StepRoutines(1 / 60f);

        Assert.IsFalse(pack1.IsCached(tex1.Id.Internal));
        Assert.IsFalse(pack2.IsCached(tex2.Id.Internal));

        Assets.ClearRegistry();
    }

    [TestMethod]
    public void LinkedDisposal()
    {
        var pack1 = Assets.RegisterPackage(validPackage1);
        var pack2 = Assets.RegisterPackage(validPackage2);

        using var tex1 = Assets.Load<Texture>("assetspack_test1:textures/dog.png");
        using var tex2 = Assets.Load<Texture>("assetspack_test2:textures/space.jpg");

        Assets.LinkDisposal(tex1.Id, tex2);

        Assert.AreEqual(828, tex1.Value.Width);
        Assert.AreEqual(826, tex1.Value.Height);

        Assert.AreEqual(1920, tex2.Value.Width);
        Assert.AreEqual(1256, tex2.Value.Height);

        Assert.IsTrue(pack1.IsCached(tex1.Id.Internal));
        Assert.IsTrue(pack2.IsCached(tex2.Id.Internal));

        tex1.Dispose(); // only dispose tex1, and then confirm tex2 was also disposed because they are linked

        Assert.IsFalse(pack1.IsCached(tex1.Id.Internal));
        Assert.IsFalse(pack2.IsCached(tex2.Id.Internal));

        Assets.ClearRegistry();
    }
}
