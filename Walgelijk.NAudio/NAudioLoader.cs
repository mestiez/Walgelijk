using NAudio.Wave;
using System.Collections.Generic;
using System.Linq;

namespace Walgelijk.NAudio
{
    /// <summary>
    /// Loads audio into memory from external sources
    /// </summary>
    public static class NAudioLoader
    {
        //KEIHARD (zo goed als) gejat van de NAUdio blog
        /// <summary>
        /// Load sound from disk
        /// </summary>
        public static Sound LoadFromFile(string path)
        {
            using var reader = new AudioFileReader(path);

            var format = reader.WaveFormat;
            int channels = format.Channels;
            int sampleRate = format.SampleRate;
            var data = new List<float>();
            var buffer = new float[channels * sampleRate];

            int samplesRead;
            while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                data.AddRange(buffer.Take(samplesRead));
            }

            Sound sound = new Sound(data, sampleRate, channels);
            return sound;
        }
    }
}
