using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Represents a texture
    /// </summary>
    public class Texture : IReadableTexture
    {
        /// <summary>
        /// Width of the texture in pixels
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the texture in pixels
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Size of the image. This returns a <see cref="Vector2"/> with <see cref="Width"/> and <see cref="Height"/>
        /// </summary>
        public Vector2 Size => new Vector2(Width, Height);

        /// <summary>
        /// Wrap mode
        /// </summary>
        public WrapMode WrapMode
        {
            get => wrapMode;
            set
            {
                wrapMode = value;
                if (value != wrapMode)
                    NeedsUpdate = true;
            }
        }
        /// <summary>
        /// Filter mode. Determines how pixels are interpolated between
        /// </summary>
        public FilterMode FilterMode
        {
            get => filterMode;
            set
            {
                filterMode = value;
                if (value != filterMode)
                    NeedsUpdate = true;
            }
        }
        /// <summary>
        /// Whether the texture has generated mipmaps upon load
        /// </summary>
        public bool GenerateMipmaps { get; }

        /// <summary>
        /// Whether the texture can store HDR image data
        /// </summary>
        public bool HDR { get; }

        /// <summary>
        /// Whether or not the renderer needs to send new information to the GPU
        /// </summary>
        public bool NeedsUpdate { get; protected set; }

        private readonly Color[] pixels;
        private FilterMode filterMode = FilterMode.Nearest;
        private WrapMode wrapMode = WrapMode.Clamp;

        /// <summary>
        /// Create a texture from a series of pixels
        /// </summary>
        public Texture(int width, int height, Color[] pixels, bool generateMipmaps = true)
        {
            Width = width;
            Height = height;
            this.pixels = pixels;
            this.GenerateMipmaps = generateMipmaps;
        }

        /// <summary>
        /// Create an empty texture
        /// </summary>
        public Texture(int width, int height, bool generateMipmaps = true)
        {
            Width = width;
            Height = height;
            this.pixels = null;
            this.GenerateMipmaps = generateMipmaps;
        }

        /// <summary>
        /// Load an image from a path
        /// </summary>
        public static Texture Load(string path, bool flipY = true, bool generateMipmaps = true)
        {
            return TextureLoader.FromFile(path, flipY, generateMipmaps);

            //var bitmap = Image.FromFile(path) as Bitmap;

            //Color[] pixels = new Color[bitmap.Height * bitmap.Width];

            ////int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            ////byte[] rgbValues = new byte[bytes];

            ////global::System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            ////for (int counter = 2; counter < rgbValues.Length; counter += 3)
            ////    rgbValues[counter] = 255;

            //int i = 0;
            //for (int y = 0; y < bitmap.Height; y++)
            //    for (int x = 0; x < bitmap.Width; x++)
            //    {
            //        var pixel = bitmap.GetPixel(x, flipY ? (bitmap.Height - 1 - y) : y);
            //        //var r = rgbValues[i];
            //        //var g = rgbValues[i + 1];
            //        //var b = rgbValues[i + 2];
            //        //var a = rgbValues[i + 4];

            //        pixels[i] = new Color(pixel.R, pixel.G, pixel.B, pixel.A);

            //        i++;
            //    }

            //return new Texture(bitmap.Width, bitmap.Height, pixels, generateMipmaps);
        }

        /// <summary>
        /// Get an immutable array of all pixels
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<Color>? GetPixels()
        {
            if (pixels == null)
                return null;

            return pixels.ToImmutableArray();
        }

        /// <summary>
        /// Get a pixel
        /// </summary>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            int index = GetIndexFrom(x, y);
            return pixels[index];
        }

        /// <summary>
        /// Set a pixel to a colour
        /// </summary>
        public void SetPixel(int x, int y, Color color)
        {
            int index = GetIndexFrom(x, y);
            pixels[index] = color;
            //TODO deze doet nog niks met de GPU
        }

        private int GetIndexFrom(int x, int y)
        {
            if (x < 0 || x >= Width)
                throw new ArgumentOutOfRangeException($"X ({x}) is out of range (0, {Width - 1})");

            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException($"Y ({y}) is out of range (0, {Height - 1})");

            int index = y * Width + x;
            return index;
        }

        private void GetCoordinatesFromIndex(int index, out int x, out int y)
        {
            if (index >= Width * Height)
                throw new ArgumentOutOfRangeException($"Index ({index}) is out of range ({Width * Height})");

            x = index % Width;
            y = (int)MathF.Floor(index / Width);
        }

        /// <summary>
        /// Force an update
        /// </summary>
        public void ForceUpdate()
        {
            NeedsUpdate = true;
        }

        /// <summary>
        /// 1x1 texture with a single white pixel
        /// </summary>
        public static Texture White { get; } = new Texture(1, 1, new[] { Color.White });
    }
}
