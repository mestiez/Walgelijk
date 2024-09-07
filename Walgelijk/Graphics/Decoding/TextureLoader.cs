using System;
using System.Collections.Generic;
using System.IO;

namespace Walgelijk;

/// <summary>
/// Utility class responsible for decoding and loading image files
/// </summary>
public static class TextureLoader
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
        new SkiaSharpDecoder()
    };

    /// <summary>
    /// Loads texture from file
    /// </summary>
    public static Texture FromFile(string path, bool flipY = true, bool? generateMipMaps = null)
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
                    t = new Texture(img.Width, img.Height, img.Colors, generateMipMaps ?? Settings.GenerateMipMaps, false);
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

    public static Texture FromStream(Stream input, bool flipY = true, bool? generateMipMaps = null)
    {
        using var reader = new MemoryStream();
        input.CopyTo(reader);
        return FromBytes(reader.ToArray(), flipY, generateMipMaps);
    }

    /// <summary>
    /// Loads texture from file
    /// </summary>
    public static Texture FromBytes(ReadOnlySpan<byte> bytes, bool flipY = true, bool? generateMipMaps = null)
    {
        foreach (var item in Decoders)
        {
            if (!item.CanDecode(bytes))
                continue;

            Texture? t = null;

            try
            {
                var img = item.Decode(bytes, flipY);
                t = new Texture(img.Width, img.Height, img.Colors, generateMipMaps ?? Settings.GenerateMipMaps, false);
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
            throw new Exception("No suitable decoder found for raw texture data");
        else
            Logger.Error("No suitable decoder found for raw texture data");

        return Texture.ErrorTexture;
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

    /// <summary>
    /// Whether or not to generate mip maps by default
    /// </summary>
    public bool GenerateMipMaps;
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

    /// <summary>
    /// Returns true for files this decoder can decode based on its raw data
    /// </summary>
    public bool CanDecode(ReadOnlySpan<byte> raw);
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
