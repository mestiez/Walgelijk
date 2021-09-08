using System;
using System.Collections.Generic;
using System.Linq;

namespace Walgelijk
{
    /// <summary>
    /// Object that contains sound data
    /// </summary>
    public class AudioData
    {
        /// <summary>
        /// Number of channels
        /// </summary>
        public int ChannelCount { get; }

        /// <summary>
        /// Audio data
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Sample rate
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Duration of the sound
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Should the data be cleared from memory and only remain in the audio engine?
        /// </summary>
        public bool KeepInMemory { get; }

        /// <summary>
        /// Create a sound from raw data
        /// </summary>
        public AudioData(byte[] data, int sampleRate, int channelCount, bool keepInMemory = false)
        {
            Data = data ?? Array.Empty<byte>();
            ChannelCount = channelCount;
            KeepInMemory = keepInMemory;
            SampleRate = sampleRate;

            if (data?.Length == 0 || sampleRate == 0 || channelCount == 0)
                Duration = TimeSpan.Zero;
            else
                Duration = TimeSpan.FromSeconds(Data.Length / ChannelCount / (double)sampleRate);
        }

        /// <summary>
        /// Remove the data from memory, indicating that the data copy in this structure is no longer needed. This will not affect the actual audio engine.
        /// </summary>
        public void ForceClearData()
        {
            Data = null;
        }
    }
}
