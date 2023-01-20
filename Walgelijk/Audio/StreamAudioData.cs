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
