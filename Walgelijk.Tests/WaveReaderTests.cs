using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Walgelijk.OpenTK;

namespace Tests;

[TestClass]
public class WaveReaderTests
{
    private Walgelijk.CommonAssetDeserialisers.Audio.WaveFixedAudioDeserialiser d;

    [TestInitialize]
    public void Init()
    {
        d = new();
    }

    [TestMethod]
    public void MonoMicrosoft()
    {
        var file = d.Deserialise(() => File.OpenRead("mono_microsoft.wav"), default);
        Assert.AreEqual(1, file.ChannelCount);
        Assert.AreEqual(48_000, file.SampleRate);
        Assert.AreEqual(28_687, file.SampleCount);
        Assert.AreEqual(file.SampleCount, file.Data.Length / 2);
    }    
    
    [TestMethod]
    public void StereoMicrosoft()
    {
        var file = d.Deserialise(() => File.OpenRead("stereo_microsoft.wav"), default);

        Assert.AreEqual(2, file.ChannelCount);
        Assert.AreEqual(44_100, file.SampleRate);
        Assert.AreEqual(228_352, file.SampleCount);
        Assert.AreEqual(file.SampleCount, file.Data.Length / 2);
    }  
    
    [TestMethod]
    public void MonoJunkChunk()
    {
        var file = d.Deserialise(() => File.OpenRead("mono_junk_chunk.wav"), default);

        Assert.AreEqual(1, file.ChannelCount);
        Assert.AreEqual(44_100, file.SampleRate);
        Assert.AreEqual(33_918, file.SampleCount);
        Assert.AreEqual(file.SampleCount, file.Data.Length / 2);
    }    

    [TestMethod]
    public void BitRateFailure()
    {
        var e = Assert.ThrowsException<Exception>(() =>
        {
            var file = d.Deserialise(() => File.OpenRead("24bitwave.wav"), default);

        });
        Assert.IsTrue(e.Message.Contains("24"), "Won't load wave files that aren't 16 bit");
    }
}
