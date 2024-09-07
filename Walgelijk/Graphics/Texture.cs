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
    private WrapMode wrapMode = WrapMode.Repeat;
    private bool disposed = false;

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
        this.RawData = new Color[width * height];
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

    public int GetIndexFrom(int x, int y)
    {
        if (x < 0 || x >= Width)
            throw new ArgumentOutOfRangeException($"X ({x}) is out of range (0, {Width - 1})");

        if (y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException($"Y ({y}) is out of range (0, {Height - 1})");

        int index = y * Width + x;
        return index;
    }

    public void GetCoordinatesFromIndex(int index, out int x, out int y)
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
        if (!disposed)
        {
            Game.Main?.Window?.Graphics?.Delete(this);
            GC.SuppressFinalize(this);
        }
        disposed = true;
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
