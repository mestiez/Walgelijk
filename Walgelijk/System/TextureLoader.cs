using SixLabors.ImageSharp;
using System;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace Walgelijk
{
    public struct TextureLoader
    {
        public static Texture FromFile(string path, bool flipY = true, bool generateMipMaps = false)
        {
            using var image = Image.Load<Rgba32>(path, out _);
            return FromImageSharpImage(image, flipY, generateMipMaps);
        }

        public static Texture FromImageSharpImage(Image<Rgba32> image, bool flipY = true, bool generateMipMaps = false)
        {
            Color[] pixels = new Color[image.Height * image.Width];
            CopyPixels(image.Frames.RootFrame, ref pixels, flipY);
            Texture tex = new(image.Width, image.Height, pixels, generateMipMaps);
            return tex;
        }

        //TODO dit is lelijk
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
                Span<Rgba32> pixelRowSpan = image.GetPixelRowSpan(y);
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
    }
}
