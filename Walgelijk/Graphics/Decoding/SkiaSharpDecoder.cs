using SkiaSharp;
using System;

namespace Walgelijk;

/// <summary>
/// Decodes the most common image formats (PNG, JPEG, BMP, etc.)
/// </summary>
public class SkiaSharpDecoder : IImageDecoder
{
    private static readonly byte[][] supportedHeaders =
    {
        "BM".ToByteArray(),//BPM
        "GIF".ToByteArray(),
        "GIF8".ToByteArray(),
        "P1".ToByteArray(),//PBM
        [0xFF, 0xD8],//JPEG
        [ 137, 80, 78, 71, 13, 10, 26, 10 ],//PNG
        [ 0x89, 0x50, 0x4e, 0x47 ],//PNG
        [ 0x4d, 0x4d, 0x00, 0x2a ],//TIFF
        [ 0x49, 0x49, 0x2a, 0x00 ],//TIFF
        [ 0x00, 0x00 ],//TGA (success lol)
        "RIFF".ToByteArray(),//WebP (RIFF)
    };

    public DecodedImage Decode(in ReadOnlySpan<byte> bytes, bool flipY)
    {
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data);
        var codecInfo = codec.Info with
        {
            AlphaType = SKAlphaType.Unpremul,
        };
        using var image = new SKBitmap(codecInfo with { ColorType = SKColorType.Rgba8888 });
        codec.GetPixels(image.Info, image.GetPixels());

        var colors = new Color[image.Width * image.Height];
        CopyPixels(image, ref colors, flipY);
        return new DecodedImage(image.Width, image.Height, colors);
    }

    public DecodedImage Decode(in byte[] bytes, int count, bool flipY) => Decode(bytes.AsSpan(0, count), flipY);

    //TODO dit is lelijk
    /// <summary>
    /// Copies pixels from <see cref="SkiaSharp.SKBitmap"/> to an array
    /// </summary>
    public static void CopyPixels(SkiaSharp.SKBitmap image, ref Color[] destination, bool flipY = true)
    {
        var pixels = image.GetPixelSpan();

        if (pixels.Length != destination.Length * 4) // 4 components per pixel
            throw new Exception($"Image pixel format error: expected 4 bytes per pixel, got {pixels.Length / destination.Length}");

        int pi = 0;

        for (int i = 0; i < destination.Length; i++)
        {
            GetCoordinatesFromIndex(i, out int x, out int y);
            if (flipY)
                y = image.Height - y - 1;
            var tI = GetIndexFrom(x, y);

            destination[tI] = new Color(pixels[pi++], pixels[pi++], pixels[pi++], pixels[pi++]);
        }

        int GetIndexFrom(int x, int y)
        {
            int index = y * image.Width + x;
            return index;
        }

        void GetCoordinatesFromIndex(int index, out int x, out int y)
        {
            x = index % image.Width;
            y = (int)float.Floor(index / image.Width);
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
