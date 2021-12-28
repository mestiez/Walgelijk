using SixLabors.ImageSharp;
using System;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.InteropServices;

namespace Walgelijk
{
    /// <summary>
    /// Utility struct responsible for decoding and loading image files
    /// </summary>
    public struct TextureLoader
    {
        /// <summary>
        /// Default import settings
        /// </summary>
        public static ImportSettings Settings;

        /// <summary>
        /// Loads texture from file
        /// </summary>
        public static Texture FromFile(string path, bool flipY = true, bool generateMipMaps = false)
        {
            using var image = Image.Load<Rgba32>(path, out _);
            return FromImageSharpImage(image, flipY, generateMipMaps);
        }

        /// <summary>
        /// Loads texture from <see cref="SixLabors.ImageSharp.Image"/>
        /// </summary>
        public static Texture FromImageSharpImage(Image<Rgba32> image, bool flipY = true, bool generateMipMaps = false)
        {
            Color[] pixels = new Color[image.Height * image.Width];
            CopyPixels(image.Frames.RootFrame, ref pixels, flipY);
            Texture tex = new(image.Width, image.Height, pixels, generateMipMaps, false);
            tex.WrapMode = Settings.WrapMode;
            tex.FilterMode = Settings.FilterMode;
            return tex;
        }

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

        /// <summary>
        /// Import settings options
        /// </summary>
        public struct ImportSettings
        {
            /// <summary>
            /// The filter mode to set by default
            /// </summary>
            public FilterMode FilterMode;

            /// <summary>
            /// The wrap mode to set by default
            /// </summary>
            public WrapMode WrapMode;

            /// <summary>
            /// The HDR flag value by default. <b>This is not applicable currently because the decoder can't read HDRIs yet</b>
            /// </summary>
            public bool HDR;
        }
    }
}
