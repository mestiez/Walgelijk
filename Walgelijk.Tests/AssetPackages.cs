using Microsoft.VisualStudio.TestTools.UnitTesting;
using NVorbis;
using System;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Text;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.OpenTK;

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

    [TestMethod]
    public void SimpleDeserialise()
    {
        using var package = AssetPackage.Load(validPackage);

        var str = package.Load<string>("data/convars.txt");
        Assert.IsNotNull(str);

        var hash = Crc32.HashToUInt32(Encoding.UTF8.GetBytes(str));
        var expectedHash = 0x7603F444u;
        Assert.AreEqual(expectedHash, hash);

        Assert.AreSame(package.Load<string>("data/convars.txt"), str);
    }

    [TestMethod]
    public void StreamDeserialise()
    {
        using var reference = new VorbisReader("splitmek.ogg");

        using var package = AssetPackage.Load(validPackage);

        AssetDeserialisers.Register(new TestStreamAudioDeserialiser());

        var audio = package.Load<StreamAudioData>("sounds/music/splitmek.ogg");
        Assert.IsNotNull(audio);
        Assert.AreEqual(reference.Channels, audio.ChannelCount);
        Assert.AreEqual(reference.SampleRate, audio.SampleRate);
        Assert.AreEqual(reference.TotalSamples, audio.SampleCount);

        var source = audio.InputSourceFactory();
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

    // TODO test the following:
    // - async loading
    // - concurrent loading
    // - weird edge cases (unloading while loading)
    // - multiple candidates
    // - streaming
    // - disposing
    // - lifetime stuff
}

public class TestStreamAudioDeserialiser : IAssetDeserialiser
{
    public Type ReturningType => typeof(StreamAudioData);

    public bool IsCandidate(in AssetMetadata assetMetadata)
    {
        return
            assetMetadata.MimeType.Equals("audio/vorbis", StringComparison.InvariantCultureIgnoreCase) ||
            assetMetadata.MimeType.Equals("audio/ogg", StringComparison.InvariantCultureIgnoreCase);
    }

    public object Deserialise(Stream stream, in AssetMetadata assetMetadata)
    {
        using var reader = new VorbisReader(stream, false);
        var data = VorbisFileReader.ReadMetadata(reader);
        return new StreamAudioData(() => new OggAudioStream(stream), data.SampleRate, data.NumChannels, data.SampleCount);
    }
}
