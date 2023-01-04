using System;
using System.IO;

namespace Walgelijk;

public class StreamAudioData : AudioData, IDisposable
{
    public const int BufferSize = 1024;

    public readonly FileInfo File;
    public int Cursor = 0;

    private readonly Stream stream;
    private readonly byte[] buffer = new byte[BufferSize];

    public long TotalSize => stream.Length;

    public StreamAudioData(string path) : base(Array.Empty<byte>(), 0, 0, 0, true)
    {
        File = new FileInfo(path) ;
        stream = new FileStream(File.FullName, FileMode.Open, FileAccess.Read);
    }

    public ReadOnlySpan<byte> Read(int amount)
    {
        var c = stream.Read(buffer, 0, amount);
        Cursor += c;
        return buffer.AsSpan(0, c);
    }

    public void Dispose()
    {
        stream.Dispose();
    }
}

//public interface ISampleSource
//{
//    /// <summary>
//    /// Returns a span with samples. The returned span could be smaller than the requested amount. If the span is empty (length 0), 
//    /// </summary>
//    /// <param name="amount"></param>
//    /// <returns></returns>
//    public ReadOnlySpan<byte> Read(int amount);
//}

/// <summary>
/// Object that contains sound data
/// </summary>
public class AudioData : IExternal<byte>
{
    /// <summary>
    /// Raw data for the audio. This doesn't necessarily contain all audio data as it could be used as a streaming buffer
    /// </summary>
    protected byte[] Data;

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
    /// Should the data be cleared from memory and only remain in the audio engine?
    /// </summary>
    public bool DisposeLocalCopyAfterUpload { get; }

    /// <summary>
    /// Create a sound from raw data
    /// </summary>
    public AudioData(byte[] data, int sampleRate, int channelCount, long sampleCount, bool keepInMemory = false)
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

    /// <summary>
    /// Remove the data from memory, indicating that the data copy in this structure is no longer needed. This will not affect the actual audio engine.
    /// </summary>
    public void DisposeLocalCopy()
    {
        Data = null;
    }

    /// <summary>
    /// Get a readonly collection of the raw audio data. This can be null if it has been disposed.
    /// </summary>
    public ReadOnlyMemory<byte>? GetData() => Data;
}
