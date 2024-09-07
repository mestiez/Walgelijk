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
            new(0x5346949C),
            new(0x757F7F40)
        ];

        Assert.IsTrue(expected.SequenceEqual(content), "read content does not match expected content");
    }

    [TestMethod]
    public void GetAssetPath()
    {
        using var package = AssetPackage.Load(validPackage);

        string[] paths = [
            "textures/ui/levelselect/levels_icon.png",
            "data/dj.json",
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

        using var actual = new StreamReader(asset.Stream(), encoding: Encoding.UTF8, leaveOpen: false);

        Assert.AreEqual(expectedContent, actual.ReadToEnd());
    }

    [TestMethod]
    public void SimpleDeserialise()
    {
        using var package = AssetPackage.Load(validPackage);

        var str = package.Load<string>("data/convars.txt");
        Assert.IsNotNull(str);

        var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str));
        var expectedHash = 0xD142F3B5u;
        Assert.AreEqual(expectedHash, hash);

        Assert.AreSame(package.Load<string>("data/convars.txt"), str);
    }

    [TestMethod]
    public async Task AsyncDeserialise()
    {
        var package = AssetPackage.Load(validPackage);
        byte[] str = [];

        await Task.Run(() =>
        {
            str = package.Load<byte[]>("data/dj.json");
            Assert.IsNotNull(str);

            var hash = Crc32.HashToUInt32(str);
            var expectedHash = 0x93F9269Bu;
            Assert.AreEqual(expectedHash, hash);

        });

        Assert.AreSame(package.Load<byte[]>("data/dj.json"), str);

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
                str1 = package.Load<byte[]>("data/dj.json");
                Assert.IsNotNull(str1);

                var hash = Crc32.HashToUInt32(str1);
                var expectedHash = 0x93F9269Bu;
                Assert.AreEqual(expectedHash, hash);

            }),
            Task.Run(() =>
            {
                str2 = package.Load<string>("data/convars.txt");
                Assert.IsNotNull(str2);

                var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str2));
                var expectedHash = 0xD142F3B5u;
                Assert.AreEqual(expectedHash, hash);

            })
        );

        Assert.AreSame(package.Load<byte[]>("data/dj.json"), str1);
        Assert.AreSame(package.Load<string>("data/convars.txt"), str2);
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
                str1 = package.Load<string>("data/dj.json");
                Assert.IsNotNull(str1);

                var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str1));
                var expectedHash = 0x93F9269Bu;
                Assert.AreEqual(expectedHash, hash);

            }),
            Task.Run(() =>
            {
                str2 = package.Load<string>("data/dj.json");
                Assert.IsNotNull(str2);

                var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str2));
                var expectedHash = 0x93F9269Bu;
                Assert.AreEqual(expectedHash, hash);

            })
        );

        Assert.AreSame(package.Load<string>("data/dj.json"), str1);
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
            var audio = package.Load<StreamAudioData>("sounds/music/splitmek.ogg");
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
            var audio = package.Load<StreamAudioData>("sounds/music/splitmek.ogg");
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

