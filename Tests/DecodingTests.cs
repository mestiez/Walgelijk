using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Tests;

[TestClass]
public class DecodingTests
{
    [TestMethod]
    public void Png()
    {
        ImageDecoder.Png.Decode(File.ReadAllBytes("test.png"), out var width, out var height);
        Assert.AreEqual(width, 566);
        Assert.AreEqual(height, 375);
    }
}
