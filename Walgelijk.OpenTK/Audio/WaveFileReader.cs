using System;
using System.IO;
using System.Linq;

namespace Walgelijk.OpenTK;

public static class WaveFileReader
{
    public static AudioFileData Read(string path)
    {
        var result = new AudioFileData();
        using var file = File.OpenRead(path);
        var buffer = new byte[4];

        if (!ReadUntil(buffer, file, "RIFF"))
            throw new Exception("Input file is not a WAVE file: no RIFF header");

        if (!ReadUntil(buffer, file, "WAVE"))
            throw new Exception("Input file is not a WAVE file: format does not equal \"WAVE\"");

        if (!ReadUntil(buffer, file, "fmt "))
            throw new Exception("Input file is not a WAVE file: \"fmt \" chunk not found");

        if (file.Read(buffer, 0, 4) != 4 || BitConverter.ToInt32(buffer, 0) != 16)
            throw new Exception("Input file is not a valid WAVE file: fmt chunk does not equal 16");

        if (file.Read(buffer, 0, 2) != 2 || BitConverter.ToInt16(buffer, 0) != 1)
            throw new Exception("Input file is not a supported WAVE file: AudioFormat does not equal 1, data is compressed");

        // read channel count
        if (file.Read(buffer, 0, 2) != 2)
            throw new Exception("Input file is not a valid WAVE file: the number of channels could not be read");
        var numChannels = BitConverter.ToInt16(buffer, 0);
        if (numChannels <= 0 || numChannels > 2)
            throw new Exception("Input WAVE file has an invalid channel count. The number of channels must be 1 or 2");
        result.NumChannels = numChannels;

        // read sample rate
        if (file.Read(buffer, 0, 4) != 4)
            throw new Exception("Input file is not a valid WAVE file: the sample rate could not be read");
        var sampleRate = BitConverter.ToInt32(buffer, 0);
        if (sampleRate <= 0 || sampleRate > 96000)
            throw new Exception("Input WAVE file has an invalid sample rate. The sample rate must be greater than 0 and less than or equal to 96000");
        result.SampleRate = sampleRate;

        if (file.Read(buffer, 0, 4) != 4)
            throw new Exception("Input file is not a valid WAVE file: byte rate could not be read");

        if (file.Read(buffer, 0, 2) != 2)
            throw new Exception("Input file is not a valid WAVE file: block align could not be read");

        if (file.Read(buffer, 0, 2) != 2 || BitConverter.ToInt16(buffer, 0) != 16)
            throw new Exception($"Input file is not a valid WAVE file: the engine only supports 16 bit WAVE files, this file is {BitConverter.ToInt16(buffer, 0)} bit");

        // find data chunk
        if (!ReadUntil(buffer, file, "data"))
            throw new Exception("Input file is not a WAVE file: Data chunk could not be found");

        if (file.Read(buffer, 0, 4) != 4)
            throw new Exception($"Input file is not a valid WAVE file: the data chunk size could not be read");
        int dataChunkSize = BitConverter.ToInt32(buffer, 0);

        // read actual audio data
        byte[] data = new byte[dataChunkSize];
        int actuallyRead = file.Read(data, 0, dataChunkSize);
        if (actuallyRead != dataChunkSize)
            throw new Exception("Input file is not a valid WAVE file: the reported data chunk size does not match the actually read data size");

        result.Data = data;
        result.SampleCount = data.Length;
        file.Dispose();

        return result;
    }

    private static bool ReadUntil(byte[] buffer, FileStream file, ReadOnlySpan<byte> data)
    {
        var b = buffer.AsSpan();
        while (true)
        {
            var read = file.Read(buffer);
            if (read == 0) // eof
                return false;
            if (read != data.Length) // mustve hit the wall
                continue;
            if (b.SequenceEqual(data)) // found it!!
                return true;
        }
    }

    private static bool ReadUntil(byte[] buffer, FileStream file, ReadOnlySpan<char> data)
    {
        while (true)
        {
            var read = file.Read(buffer);
            if (read == 0) // eof
                return false;
            if (read != data.Length) // mustve hit the wall
                continue;
            if (EqualsString(buffer, data)) // found it!!
                return true;
        }
    }

    private static bool EqualsString(byte[] bytes, in ReadOnlySpan<char> value)
    {
        if (bytes.Length != value.Length)
            return false;
        for (int i = 0; i < bytes.Length; i++)
            if (bytes[i] != value[i])
                return false;
        return true;
    }
}
