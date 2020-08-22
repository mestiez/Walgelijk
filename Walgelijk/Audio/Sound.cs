using System;
using System.Collections.Generic;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Object that contains sound data
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// Number of channels
        /// </summary>
        public int Channels { get; }

        /// <summary>
        /// Raw PCM data
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Sample rate
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Duration of the sound
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Create a sound from PCM data
        /// </summary>
        public Sound(byte[] pcm, int sampleRate, int channelCount)
        {
            Data = pcm;
            Channels = channelCount;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// Load raw PCM data from path
        /// </summary>
        public static Sound Loud(string path)
        {
            byte[] data = null;
            int sampleRate = 0;
            int channelCount = 0;

            return new Sound(data, sampleRate, channelCount);
        }
    }
}
