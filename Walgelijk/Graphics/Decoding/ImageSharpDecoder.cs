using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Walgelijk;

/// <summary>
/// Decodes the most common image formats (PNG, JPEG, BMP, etc.)
/// </summary>
public class ImageSharpDecoder : IImageDecoder
{
    private static readonly byte[][] supportedHeaders =
    {
        "BM".ToByteArray(),//BPM
        "GIF".ToByteArray(),
        "GIF8".ToByteArray(),
        "P1".ToByteArray(),//PBM
        new byte[]{ 0xFF, 0xD8 },//JPEG
        new byte[]{ 137, 80, 78, 71, 13, 10, 26, 10 },//PNG
        new byte[]{ 0x89, 0x50, 0x4e, 0x47 },//PNG
        new byte[]{ 0x4d, 0x4d, 0x00, 0x2a },//TIFF
        new byte[]{ 0x49, 0x49, 0x2a, 0x00 },//TIFF
        new byte[]{ 0x00, 0x00 },//TGA (success lol)
        "RIFF".ToByteArray(),//WebP (RIFF)
    };

    public DecodedImage Decode(in ReadOnlySpan<byte> bytes, bool flipY)
    {
        using var image = Image.Load<Rgba32>(bytes, out _);
        var colors = new Color[image.Width * image.Height];
        CopyPixels(image.Frames.RootFrame, ref colors, flipY);
        return new DecodedImage(image.Width, image.Height, colors);
    }

    public DecodedImage Decode(in byte[] bytes, int count, bool flipY) => Decode(bytes.AsSpan(0, count), flipY);

    //TODO dit is lelijk
    /// <summary>
    /// Copies pixels from <see cref="SixLabors.ImageSharp.Image"/> to an array
    /// </summary>
    public static void CopyPixels(ImageFrame<Rgba32> image, ref Color[] destination, bool flipY = true)
    {
#if false
            if (image.TryGetSinglePixelSpan(out var all))
            {
                if (all.Length > destination.Length)
                {
                    Logger.Warn("TextureLoader.CopyPixels destination is not large enough for given image");
                    return;
                }

                for (int p = 0; p < Math.Min(all.Length, destination.Length); p++)
                {
                    var transformedIndex = flipY ? all.Length - 1 - p : p;
                    var c = all[transformedIndex];
                    destination[p] = new Color(c.R, c.G, c.B, c.A);
                }
                return;
            }
#endif
        int i = 0;
        for (int yy = 0; yy < image.Height; yy++)
        {
            int y = flipY ? (image.Height - 1 - yy) : yy;
            Span<Rgba32> pixelRowSpan = image.PixelBuffer.DangerousGetRowSpan(y);
            for (int x = 0; x < image.Width; x++)
            {
                if (i >= destination.Length)
                {
                    Logger.Warn("TextureLoader.CopyPixels destination is not large enough for given image");
                    return;
                }

                var c = pixelRowSpan[x];
                destination[i] = new Color(c.R, c.G, c.B, c.A);
                i++;
            }
        }
    }

    public bool CanDecode(in string filename)
    {
        return
            e(filename, ".png") ||
            e(filename, ".jpeg") ||
            e(filename, ".jpg") ||
            e(filename, ".bmp") ||
            e(filename, ".webp") ||
            e(filename, ".tga") ||
            e(filename, ".tif") ||
            e(filename, ".tiff") ||
            e(filename, ".pbm") ||
            e(filename, ".gif");

        static bool e(in string filename, in string ex) => filename.EndsWith(ex, StringComparison.InvariantCultureIgnoreCase);
    }

    public bool CanDecode(ReadOnlySpan<byte> raw)
    {
        foreach (var item in supportedHeaders)
            if (raw.StartsWith(item))
                return true;
        return false;
    }
}
