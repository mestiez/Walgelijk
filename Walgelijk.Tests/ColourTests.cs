using Microsoft.VisualStudio.TestTools.UnitTesting;
using Walgelijk;

namespace Tests;

[TestClass]
public class ColourTests
{
    [TestMethod]
    public void HSVtoRGBAndViceVersa()
    {
        for (int i = 0; i < 256; i++)
        {
            var color = Utilities.RandomColour();
            color.GetHsv(out var h, out var s, out var v);
            var back = Color.FromHsv(h, s, v, color.A);
            Assert.AreEqual(color.R, back.R, 0.01f);
            Assert.AreEqual(color.G, back.G, 0.01f);
            Assert.AreEqual(color.B, back.B, 0.01f);
            Assert.AreEqual(color.A, back.A, 0.01f);
        }
    }
}
