using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Represents a texture
/// </summary>
public class Texture : IReadableTexture, IDisposable
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
    public bool NeedsUpdate { get; set; }

    /// <summary>
    /// Direct access to the pixel data. This may be null if <see cref="DisposeLocalCopy"/> was called.
    /// </summary>
    public Color[]? RawData;

    private FilterMode filterMode = FilterMode.Nearest;
    private WrapMode wrapMode = WrapMode.Clamp;

    /// <summary>
    /// Create a texture from a series of pixels
    /// </summary>
    public Texture(int width, int height, Color[] pixels, bool generateMipmaps = true, bool HDR = false)
    {
        Width = width;
        Height = height;
        this.HDR = HDR;
        this.RawData = pixels;
        this.GenerateMipmaps = generateMipmaps;
    }

    /// <summary>
    /// Create an empty texture
    /// </summary>
    public Texture(int width, int height, bool generateMipmaps = true, bool HDR = false)
    {
        Width = width;
        Height = height;
        this.HDR = HDR;
        this.RawData = null;
        this.GenerateMipmaps = generateMipmaps;
    }

    /// <summary>
    /// Load an image from a path using <see cref="TextureLoader"/>
    /// </summary>
    public static Texture Load(string path, bool flipY = true, bool generateMipmaps = true)
    {
        return TextureLoader.FromFile(path, flipY, generateMipmaps);
    }

    /// <summary>
    /// Remove the pixels that are stored on CPU memory
    /// </summary>
    public void DisposeLocalCopy()
    {
        if (RawData != null)
        {
            RawData = null;
            GC.Collect();
        }
    }

    /// <summary>
    /// Get a pixel
    /// </summary>
    public Color GetPixel(int x, int y)
    {
        if (RawData == null)
            throw new Exception("No CPU side pixel array available");

        int index = GetIndexFrom(x, y);
        return RawData[index];
    }

    /// <summary>
    /// Set a pixel to a colour
    /// </summary>
    public void SetPixel(int x, int y, Color color)
    {
        if (RawData == null)
            throw new Exception("No CPU side pixel array available");

        int index = GetIndexFrom(x, y);
        RawData[index] = color;
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
    /// Delete the texture from the GPU
    /// </summary>
    public void Dispose()
    {
        Game.Main.Window.Graphics.Delete(this);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Get an immutable collection of all pixels
    /// </summary>
    /// <returns></returns>
    public ReadOnlyMemory<Color>? GetData()
    {
        if (RawData == null)
            return ReadOnlyMemory<Color>.Empty;

        return RawData;
    }

    /// <summary>
    /// Should all pixels be discarded after this object has been uploaded to the GPU?
    /// </summary>
    public bool DisposeLocalCopyAfterUpload { get; set; } = true;

    /// <summary>
    /// 1x1 texture with a single white pixel
    /// </summary>
    public static Texture White { get; } = new Texture(1, 1, new[] { Color.White });

    /// <summary>
    /// 8x8 red-black frowning face texture, usually used to indicate something has gone wrong
    /// </summary>
    public static Texture ErrorTexture { get; } = new Texture(8, 8,
        new[] {
            Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red,
            Colors.Red, Colors.Black, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Black, Colors.Red,
            Colors.Red, Colors.Black, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Black, Colors.Red,
            Colors.Red, Colors.Red, Colors.Black, Colors.Black, Colors.Black, Colors.Black, Colors.Red, Colors.Red,
            Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red,
            Colors.Red, Colors.Black, Colors.Black, Colors.Red, Colors.Red, Colors.Black, Colors.Black, Colors.Red,
            Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red,
            Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red, Colors.Red,
        }, false, false);
}

/// <summary>
/// Provides texture generation functions
/// </summary>
public struct TexGen
{
    /// <summary>
    /// Generate a simple checkerboard texture
    /// </summary>
    public static Texture Checkerboard(int width, int height, int boxSize, Color a, Color b)
    {
        var pixels = new Color[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var f = (x % (boxSize * 2)) >= boxSize;
                f ^= (y % (boxSize * 2)) >= boxSize;
                pixels[i++] = Utilities.Lerp(a, b, f ? 1 : 0);
            }

        return new Texture(width, height, pixels, false, false);
    }

    /// <summary>
    /// Generate a simple noise texture
    /// </summary>
    public static Texture Noise(int width, int height, float freq, float z, Color a, Color b)
    {
        var pixels = new Color[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var f = Walgelijk.Noise.GetFractal(x * freq / width, y * freq / height, z, 3) * 0.5f + 0.5f;
                pixels[i++] = Utilities.Lerp(a, b, f);
            }

        return new Texture(width, height, pixels, false, false);
    }

    /// <summary>
    /// Generate a flat colour texture
    /// </summary>
    public static Texture Colour(int width, int height, Color color)
    {
        var pixels = new Color[width * height];
        Array.Fill(pixels, color);

        return new Texture(width, height, pixels, false, false);
    }


    /// <summary>
    /// Generate a simple gradient texture
    /// </summary>
    public static Texture Gradient(int width, int height, GradientType type, Color a, Color b)
    {
        var pixels = new Color[width * height];
        int i = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var p = new Vector2(x / (float)width, y / (float)width);
                float f = type switch
                {
                    GradientType.Horizontal => p.X,
                    GradientType.Vertical => p.Y,
                    GradientType.Diagonal => (p.X + p.Y) * 0.5f,
                    GradientType.Radial => Utilities.MapRange(-MathF.PI, MathF.PI, 0, 1, MathF.Atan2((p.Y - 0.5f) * .5f, (p.X - 0.5f) * .5f)),
                    GradientType.Spherical => Vector2.Distance(p, new(0.5f)) * 2f,
                    GradientType.Manhattan => MathF.Max(MathF.Abs(.5f - p.Y), MathF.Abs(.5f - p.X)) * 2f,
                    _ => throw new NotImplementedException(),
                };
                pixels[i++] = Utilities.Lerp(a, b, f);
            }

        return new Texture(width, height, pixels, false, false);
    }


    public enum GradientType
    {
        Horizontal,
        Vertical,
        Diagonal,
        Radial,
        Spherical,
        Manhattan,
    }
}
