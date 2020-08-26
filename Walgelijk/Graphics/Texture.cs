using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Represents a texture
    /// </summary>
    public class Texture
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
        public WrapMode WrapMode { get; set; } = WrapMode.Clamp;        
        /// <summary>
        /// Filter mode. Determines how pixels are interpolated between
        /// </summary>
        public FilterMode FilterMode { get; set; } = FilterMode.Nearest;

        private readonly Color[] pixels;

        /// <summary>
        /// Create a texture from a series of pixels
        /// </summary>
        public Texture(int width, int height, Color[] pixels)
        {
            Width = width;
            Height = height;
            this.pixels = pixels;
        }

        /// <summary>
        /// Load an image from a path
        /// </summary>
        public static Texture Load(string path, bool flipY = true)
        {
            var bitmap = Image.FromFile(path) as Bitmap;

            Color[] pixels = new Color[bitmap.Height * bitmap.Width];

            //TODO bitmapdata technique

            int i = 0;
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, flipY ? (bitmap.Height - 1 - y) : y);
                    pixels[i] = new Color(pixel.R, pixel.G, pixel.B, pixel.A);
                    i++;
                }

            return new Texture(bitmap.Width, bitmap.Height, pixels);
        }

        /// <summary>
        /// Get an immutable array of all pixels
        /// </summary>
        /// <returns></returns>
        public ImmutableArray<Color> GetPixels()
        {
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
            if (index >= Width* Height)
                throw new ArgumentOutOfRangeException($"Index ({index}) is out of range ({Width*Height})");

            x = index % Width;
            y = (int)MathF.Floor(index / Width);
        }
    }

    /// <summary>
    /// Wrap mode for textures
    /// </summary>
    public enum WrapMode
    {
        /// <summary>
        /// Extends the edge pixels
        /// </summary>
        Clamp,
        /// <summary>
        /// Repeats the UV
        /// </summary>
        Repeat,
        /// <summary>
        /// Mirrors the UV
        /// </summary>
        Mirror
    }

    /// <summary>
    /// Filter mode for textures
    /// </summary>
    public enum FilterMode
    {
        /// <summary>
        /// Nearest pixel sampling. Results in pixelated images
        /// </summary>
        Nearest,
        /// <summary>
        /// Linear pixel interpolation. Results in somewhat smooth images
        /// </summary>
        Linear
    }
}
