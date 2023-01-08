using System;
using System.IO;

namespace Walgelijk;

public class StreamAudioData : AudioData
{
    public readonly FileInfo File;

    public StreamAudioData(string path, int sampleRate, int channelCount, long sampleCount)
    {
        File = new FileInfo(path);
        ChannelCount = channelCount;
        DisposeLocalCopyAfterUpload = false;
        SampleRate = sampleRate;
        SampleCount = sampleCount;

        if (SampleRate == 0 || ChannelCount == 0 || SampleCount == 0)
            Duration = TimeSpan.Zero;
        else
            Duration = TimeSpan.FromSeconds(SampleCount /*/ ChannelCount*/ / (double)sampleRate);
    }

    public FileStream CreateStream() => new FileStream(File.FullName, FileMode.Open, FileAccess.Read);

    ///// <summary>
    ///// Returns a span with samples. The returned span could be smaller than the requested amount. 
    ///// If the span is empty (length 0), the stream has ended.
    ///// </summary>
    //public ReadOnlySpan<byte> Read(int amount)
    //{
    //    var c = stream.Read(buffer, 0, amount);
    //    if (c == 0)
    //        return ReadOnlySpan<byte>.Empty;
    //    Cursor += c;
    //    return buffer.AsSpan(0, c);
    //}

    /// <summary>
    /// This does nothing because the stream is managed by the audio renderer
    /// </summary>
    public override void DisposeLocalCopy()
    {
    }

    /// <summary>
    /// Returns an empty span because this object has no knowledge of the data being read other than its source
    /// </summary>
    public override ReadOnlyMemory<byte>? GetData() => ReadOnlyMemory<byte>.Empty;
}

public class FixedAudioData : AudioData
{
    /// <summary>
    /// Raw data for the audio. This doesn't necessarily contain all audio data as it could be used as a streaming buffer.
    /// </summary>
    public byte[] Data;

    /// <summary>
    /// Create a sound from raw data
    /// </summary>
    public FixedAudioData(byte[] data, int sampleRate, int channelCount, long sampleCount, bool keepInMemory = false)
    {
        Data = data;
        ChannelCount = channelCount;
        DisposeLocalCopyAfterUpload = keepInMemory;
        SampleRate = sampleRate;
        SampleCount = sampleCount;

        if (data?.Length == 0 || sampleRate == 0 || channelCount == 0 || sampleCount == 0)
            Duration = TimeSpan.Zero;
        else
            Duration = TimeSpan.FromSeconds(sampleCount /*/ ChannelCount*/ / (double)sampleRate);
    }

    public override void DisposeLocalCopy()
    {
        Data = Array.Empty<byte>();
    }

    public override ReadOnlyMemory<byte>? GetData() => Data;
}

/// <summary>
/// Object that contains sound data
/// </summary>
public abstract class AudioData : IExternal<byte>
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
    /// Total sample count
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
    public abstract ReadOnlyMemory<byte>? GetData();
}
