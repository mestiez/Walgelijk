using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace ImageDecoder;

public static class Png
{
    private static readonly byte[] magic =
    {
        0x89,

        0x50,
        0x4e,
        0x47,

        0x0D,
        0x0A,

        0x1A,
        0x0A,
    };

    private static readonly byte[] IHDR = "IHDR".Select(CharToByte).ToArray();
    private static readonly byte[] PLTE = "PLTE".Select(CharToByte).ToArray();
    private static readonly byte[] IDAT = "IDAT".Select(CharToByte).ToArray();
    private static readonly byte[] IEND = "IEND".Select(CharToByte).ToArray();
    private static byte CharToByte(char c) => (byte)c;

    public static byte[] Decode(byte[] raw, out int width, out int height)
    {
        //TODO gebruik stream ipv raw bytes
        int cursor = 0;
        var rawSpan = raw.AsSpan();
        width = 0;
        height = 0;

        byte bitDepth = 0;
        ColourType colorType = ColourType.TrueColourWithAlpha;
        byte compressionMethod = 0;
        byte filterMethod = 0;
        byte interlaceMethod = 0;
        int bytesPerPixel = 4;

        byte[] compressedData = Array.Empty<byte>();
        int compressedDataCursor = 0;

        cursor += magic.Length;
        if (!rawSpan[0..cursor].SequenceEqual(magic))
            throw new Exception("File is not a PNG image.");

        while (cursor < raw.Length)
        {
            var chunk = Chunk.FromBytes(raw, cursor);
            var data = rawSpan[chunk.DataSlice.Index..(chunk.DataSlice.Index + chunk.DataSlice.Length)];
            switch (chunk.Type)
            {
                case ChunkType.IHDR:
                    {
                        width = BinaryPrimitives.ReadInt32BigEndian(data[0..4]);
                        if (width == 0)
                            throw new Exception("Width cannot be 0");

                        height = BinaryPrimitives.ReadInt32BigEndian(data[4..8]);
                        if (height == 0)
                            throw new Exception("Height cannot be 0");

                        bitDepth = data[8];
                        if (bitDepth is not (/*1 or 2 or 4 or */8 or 16))
                            throw new Exception($"Bit depth value '{bitDepth}' is invalid. 8 or 16 are allowed.");

                        bytesPerPixel = bitDepth / 8 * 4;
                        colorType = (ColourType)data[9];

                        if (colorType != ColourType.TrueColourWithAlpha)
                            throw new Exception($"Unsupported color type: {colorType}. Only {ColourType.TrueColourWithAlpha} is supported");

                        compressionMethod = data[10];
                        filterMethod = data[11];
                        if (filterMethod != 0)
                            throw new Exception($"Filtermode {filterMethod} is not supported. Only 0 is supported.");

                        interlaceMethod = data[12];

                        compressedData = new byte[raw.Length - data.Length]; //total file size - IHDR chunk size

                        if (interlaceMethod != 0)
                            throw new Exception("There is no support for interlaced images");
                    }
                    break;
                case ChunkType.PLTE:
                    {

                    }
                    break;
                case ChunkType.IDAT:
                    {
                        if (compressedData.Length == 0)
                            throw new Exception("IDAT chunk before IHDR chunk. Invalid png file.");

                        Array.Copy(raw, chunk.DataSlice.Index, compressedData, compressedDataCursor, data.Length);
                        //data.CopyTo(compressedData.AsSpan()[compressedDataCursor..(compressedDataCursor + data.Length)]);

                        compressedDataCursor += data.Length;
                    }
                    break;
                case ChunkType.IEND:
                    cursor = raw.Length;
                    break;
                case ChunkType.Unknown:
                default:
                    break;
            }

            cursor += chunk.TotalLength;
            if (cursor >= raw.Length)
                break;
        }

        if (compressedData == null)
            throw new Exception("IEND chunk before IHDR chunk. Invalid png file.");

        using var stream = new MemoryStream(compressedData, 0, compressedDataCursor);
        using var decompressor = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress);

        var result = new byte[width * height * bytesPerPixel]; //je moet colorType ook ff doen voor die *4 want je weet niet of er 4 kanalen zijn
        int scanlineLength = width * bytesPerPixel + 1;
        int bytesInResultLine = width * bytesPerPixel;

        var buffer = new byte[scanlineLength];

        int resultIndex = 0;
        int decompressorIndex = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte getAt(ReadOnlySpan<byte> buffer, int index) => index < 0 ? byte.MinValue : buffer[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte clamp(int v) => v < byte.MinValue ? byte.MinValue : (v >= byte.MaxValue ? byte.MaxValue : (byte)v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static byte clampf(float v) => v < byte.MinValue ? byte.MinValue : (v >= byte.MaxValue ? byte.MaxValue : (byte)v);

        while (true)
        {
            int read = decompressor.Read(buffer);
            if (read <= 0)
                break;

            var filter = buffer[0];
            for (int indexInBuffer = 1; indexInBuffer < read; indexInBuffer += 4)
            {
                Rgba8 target = new(255, 0, 255, 255);
                target.R = buffer[indexInBuffer + 0];
                target.G = buffer[indexInBuffer + 1];
                target.B = buffer[indexInBuffer + 2];
                target.A = buffer[indexInBuffer + 3];

                switch (filter)
                {
                    //None
                    case 0:
                        break;
                    //Sub
                    case 1:
                        {
                            Rgba8 left = new(
                                getAt(result, resultIndex + 0 - bytesPerPixel),
                                getAt(result, resultIndex + 1 - bytesPerPixel),
                                getAt(result, resultIndex + 2 - bytesPerPixel),
                                getAt(result, resultIndex + 3 - bytesPerPixel));

                            target.R = (byte)((target.R + left.R) % 256);
                            target.G = (byte)((target.G + left.G) % 256);
                            target.B = (byte)((target.B + left.B) % 256);
                            target.A = (byte)((target.A + left.A) % 256);
                        }
                        break;
                    //Up
                    case 2:
                        {
                            Rgba8 above = new(
                                getAt(result, resultIndex + 0 - bytesInResultLine),
                                getAt(result, resultIndex + 1 - bytesInResultLine),
                                getAt(result, resultIndex + 2 - bytesInResultLine),
                                getAt(result, resultIndex + 3 - bytesInResultLine));

                            target.R = (byte)((target.R + above.R) % 256);
                            target.G = (byte)((target.G + above.G) % 256);
                            target.B = (byte)((target.B + above.B) % 256);
                            target.A = (byte)((target.A + above.A) % 256);
                        }
                        break;
                    //Average
                    case 3:
                        {
                            Rgba8 above = new(
                                getAt(result, resultIndex + 0 - bytesInResultLine),
                                getAt(result, resultIndex + 1 - bytesInResultLine),
                                getAt(result, resultIndex + 2 - bytesInResultLine),
                                getAt(result, resultIndex + 3 - bytesInResultLine));

                            Rgba8 left = new(
                                getAt(result, resultIndex + 0 - bytesPerPixel),
                                getAt(result, resultIndex + 1 - bytesPerPixel),
                                getAt(result, resultIndex + 2 - bytesPerPixel),
                                getAt(result, resultIndex + 3 - bytesPerPixel));

                            target.R += clampf((above.R + left.R) * 0.5f);
                            target.G += clampf((above.G + left.G) * 0.5f);
                            target.B += clampf((above.B + left.B) * 0.5f);
                            target.A += clampf((above.A + left.A) * 0.5f);
                        }
                        break;
                    //Paeth
                    case 4:
                        break;
                    default:
                        break;
                }

                result[resultIndex + 0] = target.R;
                result[resultIndex + 1] = target.G;
                result[resultIndex + 2] = target.B;
                result[resultIndex + 3] = target.A;

                resultIndex += 4;
            }

            decompressorIndex += read;

            //if (resultIndex >= result.Length)
            //    break;
        }

        stream.Dispose();
        decompressor.Dispose();

        return result;

        //byte filterSub(int x, int bpp) => (byte)((Raw(x) + Raw(x - bpp)) % 256);
        //byte filterUp(int x) => (byte)((Raw(x) + Prior(x)) % 256);
    }

    private struct Chunk
    {
        public ChunkType Type;
        public int Checksum;
        public (int Index, int Length) DataSlice;

        public int TotalLength => 4 + 4 + 4 + DataSlice.Length;

        public Chunk(ChunkType type, (int index, int length) dataSlice, int checksum)
        {
            Type = type;
            Checksum = checksum;
            DataSlice = dataSlice;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chunk FromBytes(byte[] bytes, int index)
        {
            var span = bytes.AsSpan()[index..];
            var length = BinaryPrimitives.ReadInt32BigEndian(span[0..4]);
            var checksum = BinaryPrimitives.ReadInt32BigEndian(span[(8 + length)..(8 + 4 + length)]);
            var typeBytes = span[4..8];

            var type = ChunkType.Unknown;
            if (typeBytes.SequenceEqual(IHDR)) //1229472850
                type = ChunkType.IHDR;
            else if (typeBytes.SequenceEqual(PLTE)) //1347179589
                type = ChunkType.PLTE;
            else if (typeBytes.SequenceEqual(IDAT)) //1229209940
                type = ChunkType.IDAT;
            else if (typeBytes.SequenceEqual(IEND)) //1229278788
                type = ChunkType.IEND;
            //else
            //{
            //    Console.Write("Unknown chunk: ");
            //    for (int i = 0; i < typeBytes.Length; i++)
            //        Console.Write((char)typeBytes[i]);
            //    Console.WriteLine();
            //}

            return new Chunk(type, (8 + index, length), checksum);
        }
    }

    private enum ColourType : byte
    {
        Greyscale = 0,
        TrueColour = 2,
        IndexedColour = 3,
        GreyscaleWithAlpha = 4,
        TrueColourWithAlpha = 6,
    }

    private enum ChunkType
    {
        Unknown,
        /// <summary>
        /// Width, height, bit depth, color type, compression method, filter method, interlace method
        /// </summary>
        IHDR,
        /// <summary>
        /// Palette
        /// </summary>
        PLTE,
        /// <summary>
        /// The image data
        /// </summary>
        IDAT,
        /// <summary>
        /// Image end
        /// </summary>
        IEND,

    }

    private ref struct Rgba8
    {
        public byte R, G, B, A;

        public Rgba8(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}