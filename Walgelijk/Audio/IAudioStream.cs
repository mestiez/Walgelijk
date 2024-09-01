using System;

namespace Walgelijk;

/// <summary>
/// Provides all functionality needed for the audio backend to play the stream of audio
/// </summary>
public interface IAudioStream : IDisposable
{
    /// <summary>
    /// Current position in the stream
    /// </summary>
    public long Position { get; set; }

    /// <summary>
    /// Populates the given buffer with the next batch of samples and returns the amount of samples read 
    /// </summary>
    public int ReadSamples(Span<float> buffer);

    /// <summary>
    /// Current time position in the stream
    /// </summary>
    public TimeSpan TimePosition { get; set; }

    /// <summary>
    /// True if the stream has reached the end
    /// </summary>
    public bool HasEnded { get; }
}