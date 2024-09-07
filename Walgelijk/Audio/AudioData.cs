using System;

namespace Walgelijk;

/// <summary>
/// Object that contains sound data ranging from -1 to 1
/// </summary>
public abstract class AudioData : IExternal<float>
{
    /// <summary>
    /// Number of channels
    /// </summary>
    public int ChannelCount { get; init; }

    /// <summary>
    /// Sample rate
    /// </summary>
    public int SampleRate { get; init; }

    /// <summary>
    /// Total sample count (per channel)
    /// </summary>
    public long SampleCount { get; init; }

    /// <summary>
    /// Duration of the sound
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Should the data be cleared from memory and only remain in the audio engine?
    /// </summary>
    public bool DisposeLocalCopyAfterUpload { get; init; }

    /// <summary>
    /// Remove the data from memory, indicating that the data copy in this structure is no longer needed. This will not affect the actual audio engine.
    /// </summary>
    public abstract void DisposeLocalCopy();

    /// <summary>
    /// Get a readonly collection of the raw audio data. This can be null if it has been disposed.
    /// </summary>
    public abstract ReadOnlyMemory<float>? GetData();
}
