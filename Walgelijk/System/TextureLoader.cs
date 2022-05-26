using System;
using System.Collections.Generic;
using System.IO;

namespace Walgelijk;

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
    /// List of image decoders. This list is walked through for every image that is loaded, from top to bottom. You can manipulate this list in any way you like.
    /// </summary>
    public static List<IImageDecoder> Decoders = new()
    {
        new QOIDecoder(),
        new ImageSharpDecoder()
    };

    /// <summary>
    /// Loads texture from file
    /// </summary>
    public static Texture FromFile(string path, bool flipY = true, bool generateMipMaps = false)
    {
        if (File.Exists(path))
        {
            foreach (var item in Decoders)
            {
                if (!item.CanDecode(path))
                    continue;

                Texture? t = null;

                try
                {
                    var img = item.Decode(File.ReadAllBytes(path), flipY);
                    t = new Texture(img.Width, img.Height, img.Colors, generateMipMaps, false);
                    t.FilterMode = Settings.FilterMode;
                    t.WrapMode = Settings.WrapMode;
                    return t;
                }
                catch (Exception e)
                {
                    t?.DisposeLocalCopy();

                    if (Game.Main?.DevelopmentMode ?? true)
                        throw;
                    Logger.Error("Image decoder failure:" + e.Message);
                    continue;
                }
            }

            if (Game.Main?.DevelopmentMode ?? true)
                throw new Exception("No suitable decoder found for " + path);
            else
                Logger.Error("No suitable decoder found for " + path);
        }
        else
        {
            if (Game.Main?.DevelopmentMode ?? true)
                throw new Exception("Texture file not found: " + path);
            else
                Logger.Error("Texture file not found: " + path);
        }

        return Texture.ErrorTexture;

        //return FromImageSharpImage(image, flipY, generateMipMaps);
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

/// <summary>
/// Interface for decoders used by the <see cref="TextureLoader"/>
/// </summary>
public interface IImageDecoder
{
    /// <summary>
    /// Decode the given byte array and return a flattened 2D pixel grid. Throws an exception if the image can't be loaded.
    /// </summary>
    public DecodedImage Decode(in ReadOnlySpan<byte> bytes, bool flipY);
    /// <summary>
    /// Decode the given byte array and return a flattened 2D pixel grid. Throws an exception if the image can't be loaded.
    /// </summary>
    public DecodedImage Decode(in byte[] bytes, int count, bool flipY);

    /// <summary>
    /// Returns true for files this decoder can decode based on their filename
    /// </summary>
    public bool CanDecode(in string filename);
}

/// <summary>
/// Returned by an image decoder
/// </summary>
public readonly struct DecodedImage
{
    public readonly int Width;
    public readonly int Height;
    public readonly Color[] Colors;

    public DecodedImage(int width, int height, Color[] colors)
    {
        Width = width;
        Height = height;
        Colors = colors;
    }
}
