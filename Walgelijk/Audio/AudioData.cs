using System;

namespace Walgelijk
{
    /// <summary>
    /// Object that contains sound data
    /// </summary>
    public class AudioData : IExternal<byte>
    {
        private byte[] data;

        /// <summary>
        /// Number of channels
        /// </summary>
        public int ChannelCount { get; }

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
        /// Use <see cref="DisposeLocalCopyAfterUpload"/> instead
        /// </summary>
        [Obsolete("Use DisposeLocalCopyAfterUpload instead")]
        public bool KeepInMemory => DisposeLocalCopyAfterUpload;

        /// <summary>
        /// Was this audio data configured to be streamed?
        /// </summary>
        public bool Streaming { get; }

        /// <summary>
        /// Should the data be cleared from memory and only remain in the audio engine?
        /// </summary>
        public bool DisposeLocalCopyAfterUpload { get; }

        /// <summary>
        /// Create a sound from raw data
        /// </summary>
        public AudioData(byte[] data, int sampleRate, int channelCount, long sampleCount, bool keepInMemory = false, bool stream = false)
        {
            Streaming = stream;
            this.data = data ?? Array.Empty<byte>();
            ChannelCount = channelCount;
            DisposeLocalCopyAfterUpload = keepInMemory;
            SampleRate = sampleRate;
            SampleCount = sampleCount;

            if ((data?.Length == 0 && !stream) || sampleRate == 0 || channelCount == 0 || sampleCount == 0)
                Duration = TimeSpan.Zero;
            else
                Duration = TimeSpan.FromSeconds(sampleCount / ChannelCount / (double)sampleRate);

        }

        /// <summary>
        /// Use <see cref="DisposeLocalCopy()"/> instead
        /// </summary>
        [Obsolete("Use DisposeLocalCopy instead")]
        public void ForceClearData() => DisposeLocalCopy();

        /// <summary>
        /// Remove the data from memory, indicating that the data copy in this structure is no longer needed. This will not affect the actual audio engine.
        /// </summary>
        public void DisposeLocalCopy()
        {
            data = null;
        }

        /// <summary>
        /// Get a readonly collection of the raw audio data. This can be null if it has been disposed.
        /// </summary>
        public ReadOnlyMemory<byte>? GetData() => data;
    }
}
