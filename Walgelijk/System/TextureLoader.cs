using SixLabors.ImageSharp;
using System;
using SixLabors.ImageSharp.PixelFormats;

namespace Walgelijk
{
    public struct TextureLoader
    {
        public static Texture FromFile(string path, bool flipY = true, bool generateMipMaps = false)
        {
            var image = Image.Load<Rgba32>(path, out var format);

            Color[] pixels = new Color[image.Height * image.Width];
            int i = 0;
            for (int yy = 0; yy < image.Height; yy++)
            {
                int y = flipY ? (image.Height - 1 - yy) : yy;
                Span<Rgba32> pixelRowSpan = image.GetPixelRowSpan(y);
                for (int x = 0; x < image.Width; x++)
                {
                    var c = pixelRowSpan[x];
                    pixels[i] = new Color(c.R, c.G, c.B, c.A);
                    i++;
                }
            }

            Texture tex = new Texture(image.Width, image.Height, pixels, generateMipMaps);
            return tex;
        }
    }
}
