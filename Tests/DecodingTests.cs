using Microsoft.VisualStudio.TestTools.UnitTesting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using Walgelijk;

namespace Tests;

[TestClass]
public class DecodingTests
{
    private byte[] input;
    private byte[] groundTruth;

    [TestInitialize]
    public void Setup()
    {
        input = File.ReadAllBytes("test.png");

        using var image = Image.Load<Rgba32>("test.png", out _);

        Rgba32[] colors = new Rgba32[image.Width * image.Height];
        image.Frames.RootFrame.CopyPixelDataTo(colors);
        groundTruth = new byte[image.Width * image.Height * 4];
        for (int i = 0; i < colors.Length; i++)
        {
            var c = colors[i];
            groundTruth[i * 4 + 0] = c.R;
            groundTruth[i * 4 + 1] = c.G;
            groundTruth[i * 4 + 2] = c.B;
            groundTruth[i * 4 + 3] = c.A;
        }

        image.Dispose();
    }

    private void SaveResult(byte[] result, int width, int height)
    {
        Image<Rgba32> img = new(width, height);
        int index = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var color = result.AsSpan()[index..(index + 4)];
                img[x, y] = new Rgba32(color[0], color[1], color[2], color[3]);
                index += 4;
            }

        img.SaveAsPng("output.png");
        img.Dispose();
    }

    [TestMethod]
    public void Png()
    {
        var result = ImageDecoder.Png.Decode(input, out var width, out var height);
        Assert.AreEqual(566, width);
        Assert.AreEqual(375, height);
        Assert.AreEqual(result.Length, groundTruth.Length);

        SaveResult(result, width, height);

        Assert.IsTrue(result.AsSpan().SequenceEqual(groundTruth), "Image contents do not match. Decoding failed.");
    }
}
