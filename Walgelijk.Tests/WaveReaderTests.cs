using Microsoft.VisualStudio.TestTools.UnitTesting;
using Walgelijk.OpenTK;

namespace Tests;

[TestClass]
public class WaveReaderTests
{
    [TestMethod]
    public void MonoMicrosoft()
    {
        var file = WaveFileReader.Read("mono_microsoft.wav");
        Assert.AreEqual(1, file.NumChannels);
        Assert.AreEqual(48_000, file.SampleRate);
        Assert.AreEqual(26_356, file.SampleCount);
        Assert.AreEqual(file.SampleCount, file.Data.Length);
    }    
    
    [TestMethod]
    public void StereoMicrosoft()
    {
        var file = WaveFileReader.Read("stereo_microsoft.wav");
        Assert.AreEqual(2, file.NumChannels);
        Assert.AreEqual(44_100, file.SampleRate);
        Assert.AreEqual(114_176, file.SampleCount);
        Assert.AreEqual(file.SampleCount, file.Data.Length);
    }  
    
    [TestMethod]
    public void MonoJunkChunk()
    {
        var file = WaveFileReader.Read("mono_junk_chunk.wav");
        Assert.AreEqual(1, file.NumChannels);
        Assert.AreEqual(44_100, file.SampleRate);
        Assert.AreEqual(33_918, file.SampleCount);
        Assert.AreEqual(file.SampleCount, file.Data.Length);
    }
}
