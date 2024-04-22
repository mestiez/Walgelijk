using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using Walgelijk;
using Walgelijk.AssetManager;

namespace Tests.AssetManager;

[TestClass]
public class AssetJsonConverterTests
{
    [TestMethod]
    public void AssetId()
    {
        AssetId[] p = [
            new("data/test.txt"),
            new("textures/lol.png"),
            new("textures/ui/star.qoi"),
        ];

        var json = JsonConvert.SerializeObject(p);
        var returned = JsonConvert.DeserializeObject<AssetId[]>(json);

        Assert.IsTrue(p.SequenceEqual(returned));
    }   
    
    [TestMethod]
    public void StringAssetId()
    {
        var des = JsonConvert.DeserializeObject<AssetId[]>(@"[""data/test.txt"", ""textures/lol.png"", ""textures/ui/star.qoi""]");

        Assert.AreEqual(new("data/test.txt"), des[0]);
        Assert.AreEqual(new("textures/lol.png"), des[1]);
        Assert.AreEqual(new("textures/ui/star.qoi"), des[2]);
    }   
    
    [TestMethod]
    public void GlobalAssetId()
    {
        GlobalAssetId[] p = [
            new("test:data/test.txt"),
            new("gaming:textures/lol.png"),
            new("assets:textures/ui/star.qoi"),
        ];

        var json = JsonConvert.SerializeObject(p);
        var returned = JsonConvert.DeserializeObject<GlobalAssetId[]>(json);

        Assert.IsTrue(p.SequenceEqual(returned));
    }   
    
    [TestMethod]
    public void StringGlobalAssetId()
    {
        var des = JsonConvert.DeserializeObject<GlobalAssetId[]>(@"[""test:data/test.txt"", ""gaming:textures/lol.png"", ""assets:textures/ui/star.qoi""]");

        Assert.AreEqual(new("test:data/test.txt"), des[0]);
        Assert.AreEqual(new("gaming:textures/lol.png"), des[1]);
        Assert.AreEqual(new("assets:textures/ui/star.qoi"), des[2]);
    }    
    
    [TestMethod]
    public void AssetRef()
    {
        AssetRef<string> a0 = new("test:data/test.txt");
        AssetRef<Texture> a1 = new("gaming:textures/lol.png");
        AssetRef<Texture> a2 = new("assets:textures/ui/star.qoi");

        check(a0);
        check(a1);
        check(a2);

        void check<T>(AssetRef<T> expected)
        {
            var j = JsonConvert.SerializeObject(expected);
            var des = JsonConvert.DeserializeObject<AssetRef<T>>(j);
            Assert.AreEqual(expected, des);
        }
    }   
    
    [TestMethod]
    public void StringAssetRef()
    {
        var des = JsonConvert.DeserializeObject<AssetRef<AudioData>[]>(@"[""test:data/test.txt"", ""gaming:textures/lol.png"", ""assets:textures/ui/star.qoi""]");

        Assert.AreEqual(new("test:data/test.txt"), des[0]);
        Assert.AreEqual(new("gaming:textures/lol.png"), des[1]);
        Assert.AreEqual(new("assets:textures/ui/star.qoi"), des[2]);
    }    
}

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

        using var str1 = Assets.Load<byte[]>("base:shaders/sdf-font.frag");
        using var str2 = Assets.Load<byte[]>("koploper:paths/safe.json");

        Assert.AreEqual(0x2CB81DDBu, Crc32.HashToUInt32(str1.Value));
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

    [TestMethod]
    public void ReplacementTable()
    {
        var @base = Assets.RegisterPackage(validPackage1);
        var koploper = Assets.RegisterPackage(validPackage2);

        Assets.SetReplacement("base:textures/door.png", "koploper:textures/bg.png");

        using var tex1 = Assets.Load<Texture>("base:textures/door.png");
        using var tex2 = Assets.Load<Texture>("koploper:textures/bg.png");

        Assert.AreSame(tex2.Value, tex1.Value);

        Assert.AreEqual(1920, tex2.Value.Width);
        Assert.AreEqual(1080, tex2.Value.Height);

        Assert.IsFalse(@base.IsCached(tex1.Id.Internal)); // it is false because the asset has been replaced by another package, so the original asset was never loaded
        Assert.IsTrue(koploper.IsCached(tex2.Id.Internal));

        tex1.Dispose();
        tex2.Dispose();

        Assert.IsFalse(@base.IsCached(tex1.Id.Internal));
        Assert.IsFalse(koploper.IsCached(tex2.Id.Internal));

        Assets.ClearRegistry();
        Assets.ClearReplacements();
    }
}
