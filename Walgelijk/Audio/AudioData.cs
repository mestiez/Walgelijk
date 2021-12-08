using System;

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
        /// Audio data. This is null if <see cref="Streaming"/>
        /// </summary>
        public byte[] Data { get; private set; }

        /// <summary>
        /// Sample rate
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Total sample count
        /// </summary>
        public long SampleCount { get; }

        /// <summary>
        /// Duration of the sound
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Should the data be cleared from memory and only remain in the audio engine?
        /// </summary>
        public bool KeepInMemory { get; }

        /// <summary>
        /// Was this audio data configured to be streamed?
        /// </summary>
        public bool Streaming { get; }

        /// <summary>
        /// Create a sound from raw data
        /// </summary>
        public AudioData(byte[] data, int sampleRate, int channelCount, long sampleCount, bool keepInMemory = false, bool stream = false)
        {
            Streaming = stream;
            Data = data ?? Array.Empty<byte>();
            ChannelCount = channelCount;
            KeepInMemory = keepInMemory;
            SampleRate = sampleRate;
            SampleCount = sampleCount;

            if ((data?.Length == 0 && !stream) || sampleRate == 0 || channelCount == 0 || sampleCount == 0)
                Duration = TimeSpan.Zero;
            else
                Duration = TimeSpan.FromSeconds(sampleCount / ChannelCount / (double)sampleRate);

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
