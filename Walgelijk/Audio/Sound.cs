using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;

namespace Walgelijk
{
    /// <summary>
    /// Object that contains sound data
    /// </summary>
    public struct Sound
    {
        /// <summary>
        /// Number of channels
        /// </summary>
        public int Channels { get; }

        /// <summary>
        /// Audio data
        /// </summary>
        public float[] Data { get; }

        /// <summary>
        /// Sample rate
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Duration of the sound
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Determines if the sound is looping
        /// </summary>
        public bool Looping { get; set; }

        /// <summary>
        /// Create a sound from raw data
        /// </summary>
        public Sound(IEnumerable<float> data, int sampleRate, int channelCount)
        {
            Data = data?.ToArray() ?? Array.Empty<float>();
            Channels = channelCount;
            SampleRate = sampleRate;
            Looping = false;
            Duration = TimeSpan.FromSeconds(Data.Length / Channels / (double)sampleRate);
        }
    }
}
