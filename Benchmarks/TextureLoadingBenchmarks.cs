using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Walgelijk;

namespace Tests;

[SimpleJob(RuntimeMoniker.Net60)]
public class TextureLoadingBenchmarks
{
    public const string PngImage = "test.png";
    public const string QoiImage = "test.qoi";

    [Benchmark]
    public void Png()
    {
        var loaded = TextureLoader.FromFile(PngImage);
        loaded.DisposeLocalCopy();
    }

    [Benchmark]
    public void Qio()
    {
        var loaded = TextureLoader.FromFile(QoiImage);
        loaded.DisposeLocalCopy();
    }

    //[Benchmark]
    //public void StbImageDecoder()
    //{
    //    var img = Stbi.LoadFromMemory(File.ReadAllBytes(TestImagePath), 4);
    //    var colors = new Walgelijk.Color[img.Width * img.Height];

    //    for (int i = 0; i < colors.Length; i++)
    //    {
    //        var rgba = img.Data[(i * 4)..(i * 4 + 4)];
    //        colors[i] = new Walgelijk.Color(rgba[0], rgba[1], rgba[2], rgba[3]);
    //    }

    //    var loaded = new Texture(img.Width, img.Height, colors, false, false);
    //    img.Dispose();
    //}

    //[Benchmark]
    //public void BigGustaveDecoder()
    //{
    //    using var stream = File.OpenRead(TestImagePath);
    //    Png image = Png.Open(stream);

    //    var colors = new Walgelijk.Color[image.Width * image.Height];
    //    var loaded = new Texture(image.Width, image.Height, colors, false, false);

    //    for (int x = 0; x < image.Width; x++)
    //        for (int y = 0; y < image.Height; y++)
    //        {
    //            var p = image.GetPixel(x, y);
    //            var c = new Walgelijk.Color(
    //                p.R,
    //                p.G,
    //                p.B,
    //                p.A
    //                );
    //            loaded.SetPixel(x, y, c);
    //        }
    //}

    //[Benchmark]
    //public void WalgelijkImageDecoder()
    //{
    //    var img = ImageDecoder.Png.Decode(File.ReadAllBytes(TestImagePath), out int width, out int height).AsSpan();
    //    var colors = new Walgelijk.Color[width * height];

    //    for (int i = 0; i < colors.Length; i++)
    //    {
    //        var rgba = img[(i * 4)..(i * 4 + 4)];
    //        colors[i] = new Walgelijk.Color(rgba[0], rgba[1], rgba[2], rgba[3]);
    //    }

    //    var loaded = new Texture(width, height, colors, false, false);
    //}
}
