﻿using System;
using System.Buffers;

namespace Walgelijk;

/// <summary>Decoder of the "Quite OK Image" (QOI) format.</summary>
public class QOIDecoder : IImageDecoder
{
    private static readonly byte[] magic = "qoif".ToByteArray();

    /// <summary>Decodes the given QOI file contents.</summary>
    /// <remarks>Returns <see langword="true" /> if decoded successfully.</remarks>
    /// <param name="encoded">QOI file contents. Only the first <c>encodedSize</c> bytes are accessed.</param>
    /// <param name="encodedSize">QOI file length.</param>
    private bool Decode(in ReadOnlySpan<byte> encoded, int encodedSize, out bool alpha, out int width, out int height, out int[]? pixels)
    {
        width = 0;
        height = 0;
        alpha = false;
        pixels = [];

        if (encoded == null || encodedSize < 23 || encoded[0] != 113 || encoded[1] != 111 || encoded[2] != 105 || encoded[3] != 102)
            return false;

        bool linearColorspace;
        pixels = null;
        int[] index = new int[64];

        width = encoded[4] << 24 | encoded[5] << 16 | encoded[6] << 8 | encoded[7];
        height = encoded[8] << 24 | encoded[9] << 16 | encoded[10] << 8 | encoded[11];
        if (width <= 0 || height <= 0 || height > 2147483647 / width)
            return false;
        switch (encoded[12])
        {
            case 3:
                alpha = false;
                break;
            case 4:
                alpha = true;
                break;
            default:
                return false;
        }
        switch (encoded[13])
        {
            case 0:
                linearColorspace = false;
                break;
            case 1:
                linearColorspace = true;
                break;
            default:
                return false;
        }
        int pixelsSize = width * height;
        pixels = ArrayPool<int>.Shared.Rent(pixelsSize);
        //int[] pixels = new int[pixelsSize];
        encodedSize -= 8;
        int encodedOffset = 14;
        Array.Clear(index);
        int pixel = -16777216;
        for (int pixelsOffset = 0; pixelsOffset < pixelsSize;)
        {
            if (encodedOffset >= encodedSize)
                return false;
            int e = encoded[encodedOffset++];
            switch (e >> 6)
            {
                case 0:
                    pixels[pixelsOffset++] = pixel = index[e];
                    continue;
                case 1:
                    pixel = (pixel & -16777216) | ((pixel + (((e >> 4) - 4 - 2) << 16)) & 16711680) | ((pixel + (((e >> 2 & 3) - 2) << 8)) & 65280) | ((pixel + (e & 3) - 2) & 255);
                    break;
                case 2:
                    e -= 160;
                    int rb = encoded[encodedOffset++];
                    pixel = (pixel & -16777216) | ((pixel + ((e + (rb >> 4) - 8) << 16)) & 16711680) | ((pixel + (e << 8)) & 65280) | ((pixel + e + (rb & 15) - 8) & 255);
                    break;
                default:
                    if (e < 254)
                    {
                        e -= 191;
                        if (pixelsOffset + e > pixelsSize)
                            return false;
                        Array.Fill(pixels, pixel, pixelsOffset, e);
                        pixelsOffset += e;
                        continue;
                    }
                    if (e == 254)
                    {
                        pixel = (pixel & -16777216) | encoded[encodedOffset] << 16 | encoded[encodedOffset + 1] << 8 | encoded[encodedOffset + 2];
                        encodedOffset += 3;
                    }
                    else
                    {
                        pixel = encoded[encodedOffset + 3] << 24 | encoded[encodedOffset] << 16 | encoded[encodedOffset + 1] << 8 | encoded[encodedOffset + 2];
                        encodedOffset += 4;
                    }
                    break;
            }
            pixels[pixelsOffset++] = index[((pixel >> 16) * 3 + (pixel >> 8) * 5 + (pixel & 63) * 7 + (pixel >> 24) * 11) & 63] = pixel;
        }
        if (encodedOffset != encodedSize)
            return false;
        return true;
    }

    public DecodedImage Decode(in ReadOnlySpan<byte> bytes, bool flipY)
    {
        if (Decode(bytes, bytes.Length, out var alpha, out var width, out var height, out var pixels) && pixels != null)
        {
            var colors = new Color[width * height];

            for (int i = 0; i < width * height; i++)
            {
                var v = flipY ? pixels[(i % width) + (height - 1 - i / width) * width] : pixels[i];
                colors[i] =
                    new((byte)((v & 0xFF_00_00) >> 48),
                        (byte)((v & 0xFF_00) >> 40),
                        (byte)(v & 0xFF),
                        (byte)(((uint)v & 0xFF_00_00_00) >> 56));
            }
            return new DecodedImage(width, height, colors);
        }
        throw new Exception("Image could not be decoded because it isn't valid");
    }

    public DecodedImage Decode(in byte[] bytes, int count, bool flipY) => Decode(bytes.AsSpan(0, count), flipY);

    public bool CanDecode(in string filename) => filename.EndsWith(".qoi", StringComparison.InvariantCultureIgnoreCase);

    public bool CanDecode(ReadOnlySpan<byte> raw) => raw.StartsWith(magic);
}