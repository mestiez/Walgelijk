using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVorbis;
using System;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;
using Walgelijk.CommonAssetDeserialisers.Audio;

namespace Tests.AssetManager;

[TestClass]
public class AssetPackages
{
    const string validPackage = "assetspack_test1.waa";

    [TestInitialize]
    public void Init()
    {
        Assert.IsTrue(File.Exists(validPackage));
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

        var content = package.EnumerateFolder("sounds/click").ToArray();
        AssetId[] expected = [
            new(1340043485),
            new(-444787356),
            new(942193713)
        ];

        Assert.IsTrue(expected.SequenceEqual(content), "read content does not match expected content");
    }

    [TestMethod]
    public void GetAssetPath()
    {
        using var package = AssetPackage.Load(validPackage);

        string[] paths = [
            "textures/dog.png",
            "state.json",
            "sounds/click/ui_button_simple_click_01.wav",
            "shaders/door.frag",
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

        var asset = package.GetAsset(new AssetId("small.txt"));
        var expectedContent =
@"first line
second line
next comes an empty line
";

        using var actual = new StreamReader(asset.Stream(), encoding: Encoding.UTF8, leaveOpen: false);

        Assert.AreEqual(expectedContent, actual.ReadToEnd());
    }

    [TestMethod]
    public void SimpleDeserialise()
    {
        using var package = AssetPackage.Load(validPackage);

        var str = package.Load<string>("state.json");
        Assert.IsNotNull(str);

        var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str));
        var expectedHash = 0x1F4538A4u;
        Assert.AreEqual(expectedHash, hash);

        Assert.AreSame(package.Load<string>("state.json"), str);
    }

    [TestMethod]
    public async Task AsyncDeserialise()
    {
        var package = AssetPackage.Load(validPackage);
        byte[] str = [];

        await Task.Run(() =>
        {
            str = package.Load<byte[]>("state.json");
            Assert.IsNotNull(str);

            var hash = Crc32.HashToUInt32(str);
            var expectedHash = 0x1F4538A4u;
            Assert.AreEqual(expectedHash, hash);

        });

        Assert.AreSame(package.Load<byte[]>("state.json"), str);

        package.Dispose();
    }

    [TestMethod]
    public void ConcurrentUniqueDeserialise()
    {
        using var package = AssetPackage.Load(validPackage);

        byte[] str1 = [];
        var str2 = "invalid2";

        Task.WaitAll(
            Task.Run(() =>
            {
                str1 = package.Load<byte[]>("state.json");
                Assert.IsNotNull(str1);

                var hash = Crc32.HashToUInt32(str1);
                var expectedHash = 0x1F4538A4u;
                Assert.AreEqual(expectedHash, hash);

            }),
            Task.Run(() =>
            {
                str2 = package.Load<string>("small.txt");
                Assert.IsNotNull(str2);

                var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str2));
                var expectedHash = 0xF42CA29Bu;
                Assert.AreEqual(expectedHash, hash);

            })
        );

        Assert.AreSame(package.Load<byte[]>("state.json"), str1);
        Assert.AreSame(package.Load<string>("small.txt"), str2);
    }

    [TestMethod]
    public void ConcurrentIdenticalDeserialise()
    {
        using var package = AssetPackage.Load(validPackage);

        var str1 = "invalid1";
        var str2 = "invalid2";

        Task.WaitAll(
            Task.Run(() =>
            {
                str1 = package.Load<string>("state.json");
                Assert.IsNotNull(str1);

                var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str1));
                var expectedHash = 0x1F4538A4u;
                Assert.AreEqual(expectedHash, hash);

            }),
            Task.Run(() =>
            {
                str2 = package.Load<string>("state.json");
                Assert.IsNotNull(str2);

                var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str2));
                var expectedHash = 0x1F4538A4u;
                Assert.AreEqual(expectedHash, hash);

            })
        );

        Assert.AreSame(package.Load<string>("state.json"), str1);
        Assert.AreSame(str1, str2);
    }

    [TestMethod]
    public void StreamDeserialise()
    {
        using var package = AssetPackage.Load(validPackage);
        AssetDeserialisers.Register(new OggStreamAudioDeserialiser());

        for (int i = 0; i < 5; i++)
            stream();

        void stream()
        {
            using var reference = new VorbisReader("splitmek.ogg");
            var audio = package.Load<StreamAudioData>("sounds/splitmek.ogg");
            Assert.IsNotNull(audio);
            Assert.AreEqual(reference.Channels, audio.ChannelCount);
            Assert.AreEqual(reference.SampleRate, audio.SampleRate);
            Assert.AreEqual(reference.TotalSamples, audio.SampleCount);

            using var source = audio.InputSourceFactory();
            var buffer = new float[1024];
            var referenceBuffer = new float[1024];
            while (true)
            {
                int c = source.ReadSamples(buffer);
                int rC = reference.ReadSamples(referenceBuffer);
                Assert.AreEqual(rC, c);
                if (c == 0)
                    break;

                Assert.IsTrue(buffer.AsSpan(0, c).SequenceEqual(referenceBuffer.AsSpan(0, rC)));
            }
        }
    }

    [TestMethod]
    public void ConcurrentStreamDeserialise()
    {
        using var package = AssetPackage.Load(validPackage);
        AssetDeserialisers.Register(new OggStreamAudioDeserialiser());
        var l = new ManualResetEventSlim(true);

        Task.WaitAll(
            Task.Run(stream),
            Task.Run(stream)
        );

        void stream()
        {
            // l.Wait();
            using var reference = new VorbisReader("splitmek.ogg");
            var audio = package.Load<StreamAudioData>("sounds/splitmek.ogg");
            Assert.IsNotNull(audio);
            Assert.AreEqual(reference.Channels, audio.ChannelCount);
            Assert.AreEqual(reference.SampleRate, audio.SampleRate);
            Assert.AreEqual(reference.TotalSamples, audio.SampleCount);

            using var source = audio.InputSourceFactory();
            var buffer = new float[1024];
            var referenceBuffer = new float[1024];
            //   l.Set();
            while (true)
            {
                //    l.Wait();
                int c = source.ReadSamples(buffer);
                int rC = reference.ReadSamples(referenceBuffer);
                //  l.Set();
                Assert.AreEqual(rC, c);
                if (c == 0)
                    break;

                lock (referenceBuffer)
                    for (int i = 0; i < c; i++)
                        Assert.AreEqual(buffer[i], referenceBuffer[i], 0.1f);
            }
        }
    }


    // TODO test the following:
    // - weird edge cases (unloading while loading)
    // - multiple candidates
    // - disposing
    // - lifetime stuff
}

