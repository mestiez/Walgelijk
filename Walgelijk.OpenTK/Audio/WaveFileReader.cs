using System;
using System.IO;

namespace Walgelijk.OpenTK
{
    public struct WaveFileReader
    {
        public static AudioFileData Read(string path)
        {
            AudioFileData result = new();
            byte[] buffer = new byte[4];
            using var file = File.OpenRead(path);

            if (file.Read(buffer, 0, 4) != 4 || !bytesMatchString(buffer, "RIFF"))
                throw new Exception("Input file is not a WAVE file: no RIFF header");

            if (file.Read(buffer, 0, 4) != 4)
                throw new Exception("Input file is not a WAVE file: the file size could not be read");

            if (file.Read(buffer, 0, 4) != 4 || !bytesMatchString(buffer, "WAVE"))
                throw new Exception("Input file is not a WAVE file: Format does not equal \"WAVE\"");

            if (file.Read(buffer, 0, 4) != 4 || !bytesMatchString(buffer, "fmt "))
                throw new Exception("Input file is not a WAVE file: Subchunk1ID does not equal \"fmt \"");

            if (file.Read(buffer, 0, 4) != 4 || BitConverter.ToInt32(buffer, 0) != 16)
                throw new Exception("Input file is not a valid WAVE file: Subchunk1Size does not equal 16");

            if (file.Read(buffer, 0, 2) != 2 || BitConverter.ToInt16(buffer, 0) != 1)
                throw new Exception("Input file is not a valid WAVE file: AudioFormat does not equal 1, data is compressed");

            //NumChannels
            if (file.Read(buffer, 0, 2) != 2)
                throw new Exception("Input file is not a valid WAVE file: the number of channels could not be read");
            var numChannels = BitConverter.ToInt16(buffer, 0);
            if (numChannels <= 0 || numChannels > 2)
                throw new Exception("Input WAVE file has an invalid channel count. The number of channels must be 1 or 2");
            result.NumChannels = numChannels;

            //SampleRate
            if (file.Read(buffer, 0, 4) != 4)
                throw new Exception("Input file is not a valid WAVE file: the sample rate could not be read");
            var sampleRate = BitConverter.ToInt32(buffer, 0);
            if (sampleRate <= 0 || sampleRate > 96000)
                throw new Exception("Input WAVE file has an invalid sample rate. The sample rate must be more than 0 and less than or equal to 96000");
            result.SampleRate = sampleRate;

            if (file.Read(buffer, 0, 4) != 4)
                throw new Exception("Input file is not a valid WAVE file: byte rate could not be read");

            if (file.Read(buffer, 0, 2) != 2)
                throw new Exception("Input file is not a valid WAVE file: block align could not be read");

            if (file.Read(buffer, 0, 2) != 2 || BitConverter.ToInt16(buffer, 0) != 16)
                throw new Exception($"Input file is not a valid WAVE file: the engine only supports 16 bit WAVE files, this file is {BitConverter.ToInt16(buffer, 0)} bit");

            if (file.Read(buffer, 0, 4) != 4 || !bytesMatchString(buffer, "data"))
                throw new Exception("Input file is not a WAVE file: Data chunk could not be found");

            if (file.Read(buffer, 0, 4) != 4)
                throw new Exception($"Input file is not a valid WAVE file: the data chunk size could not be read");
            int dataChunkSize = BitConverter.ToInt32(buffer, 0);

            byte[] data = new byte[dataChunkSize];
            int actuallyRead = file.Read(data, 0, dataChunkSize);
            if (actuallyRead != dataChunkSize)
                throw new Exception("Input file is not a valid WAVE file: the reported data chunk size does not match the actually read data size");

            result.Data = data;
            result.SampleCount = data.Length;
            file.Dispose();

            return result;

            static bool bytesMatchString(byte[] bytes, ReadOnlySpan<char> value)
            {
                if (bytes.Length != value.Length)
                    return false;
                for (int i = 0; i < bytes.Length; i++)
                    if (bytes[i] != value[i])
                        return false;
                return true;
            }
        }
    }
}
