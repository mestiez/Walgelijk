using System.Buffers.Binary;
using ComponentAce.Compression.Libs.zlib;

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
        int cursor = 0;
        var rawSpan = raw.AsSpan();
        width = 0;
        height = 0;

        byte bitDepth = 0;
        ColourType colorType = ColourType.TrueColourWithAlpha;
        byte compressionMethod = 0;
        byte filterMethod = 0;
        byte interlaceMethod = 0;

        //http://www.libpng.org/pub/png/spec/1.2/PNG-Chunks.html
        //if so, they must appear consecutively with no other intervening chunks 
        //hoeft dus geen list
        byte[]? compressedData = null;
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
                    width = BinaryPrimitives.ReadInt32BigEndian(data[0..4]);
                    if (width == 0)
                        throw new Exception("Width cannot be 0");

                    height = BinaryPrimitives.ReadInt32BigEndian(data[4..8]);
                    if (height == 0)
                        throw new Exception("Height cannot be 0");

                    bitDepth = data[8];
                    if (bitDepth is not (1 or 2 or 4 or 8 or 16))
                        throw new Exception($"Bit depth value '{bitDepth}' is invalid. 1, 2, 4, 8, 16 are allowed.");

                    colorType = (ColourType)data[9];
                    compressionMethod = data[10];
                    filterMethod = data[11];
                    interlaceMethod = data[12];

                    compressedData = new byte[width * height * (bitDepth / 8 * 4)]; 

                    if (interlaceMethod != 0)
                        throw new Exception("There is no support for interlaced images");
                    break;
                case ChunkType.PLTE:
                    break;
                case ChunkType.IDAT:
                    {
                        if (compressedData == null)
                            throw new Exception("IDAT chunk before IHDR chunk. Invalid png file.");
                        data.CopyTo(compressedData.AsSpan()[compressedDataCursor..(compressedDataCursor + data.Length)]);
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
        using var decompressor = new ZInputStream(stream);
        var result = new byte[width * height * (bitDepth / 8 * 4)]; //je moet colorType ook ff doen voor die *4 want je weet niet of er 4 kanalen zijn
        var buffer = new byte[5];
        int index = 0;
        while (true)
        {
            int read = decompressor.Read(buffer);
            if (read <= 0)
                break;

            byte filter = buffer[0];

            result[index + 0] = buffer[1];
            result[index + 1] = buffer[2];
            result[index + 2] = buffer[3];
            result[index + 3] = buffer[4];
            index += 4;
            if (index >= result.Length)
                break;
        }

        stream.Dispose();
        decompressor.Dispose();

        return result;
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
}