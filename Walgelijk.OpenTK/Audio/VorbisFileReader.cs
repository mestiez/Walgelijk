using System;
using System.IO;

namespace Walgelijk.OpenTK
{
    public struct VorbisFileReader
    {
#if false
        private static readonly float[] streamingBufferFloatData = new float[OpenALAudioRenderer.StreamingBufferSize / 2];

        public static int StreamRead(string path, int sourceStartIndex, ref AudioFileData audioFileData)
        {
            var absolutePath = Path.GetFullPath(path);
            var vorbis = AudioObjects.VorbisReaderCache.Load(absolutePath);

            if (audioFileData.Data == null)
            {
                var newData = ReadOnlyMetadata(vorbis);
                audioFileData.Data = new byte[OpenALAudioRenderer.StreamingBufferSize];
                audioFileData.NumChannels = newData.NumChannels;
                audioFileData.SampleRate = newData.SampleRate;
            }

            int count = vorbis.ReadSamples(streamingBufferFloatData, sourceStartIndex, streamingBufferFloatData.Length);
            int read = Math.Min(count, streamingBufferFloatData.Length * 2);

            for (long i = 0; i < read; i++)
            {
                var sample = (short)(streamingBufferFloatData[i] * short.MaxValue);
                audioFileData.Data[i * 2] = (byte)sample;
                audioFileData.Data[i * 2 + 1] = (byte)(sample >> 8);
            }

            return read;
        }
#endif

        public static AudioFileData ReadMetadata(NVorbis.VorbisReader reader)
        {
            if (reader.Channels != 1 && reader.Channels != 2)
                throw new Exception("Input Vorbis file has an invalid channel count. The number of channels must be 1 or 2");

            if (reader.SampleRate <= 0 || reader.SampleRate > 96000)
                throw new Exception("Input Vorbis file has an invalid sample rate. The sample rate must be more than 0 and less than or equal to 96000");

            var data = new AudioFileData
            {
                NumChannels = (short)reader.Channels,
                SampleRate = reader.SampleRate,
                SampleCount = reader.TotalSamples
            };

            return data;
        }

        public static AudioFileData Read(string path)
        {
            var absolutePath = Path.GetFullPath(path);
            using var vorbis = new NVorbis.VorbisReader(absolutePath);

            var data = ReadMetadata(vorbis);

            var sampleCount = vorbis.TotalSamples * vorbis.Channels;

            data.Data = new byte[sampleCount * 2]; // 16 bits per sample, in paren van bytes
            var floatData = new float[sampleCount];

            int cPos = 0;
            while (true)
            {
                int count = vorbis.ReadSamples(floatData, cPos, floatData.Length);
                cPos += count;
                if (count <= 0 || count >= floatData.Length)
                    break;
            }

            for (long i = 0; i < sampleCount; i++)
            {
                var sample = (short)(floatData[i] * short.MaxValue);
                data.Data[i * 2] = (byte)sample;
                data.Data[i * 2 + 1] = (byte)(sample >> 8);
            }

            return data;
        }
    }
}
