using System;

namespace Walgelijk;

public class StreamAudioData : AudioData
{
    public readonly Func<IAudioStream> InputSourceFactory;

    public StreamAudioData(Func<IAudioStream> sourceFactory, int sampleRate, int channelCount, long sampleCount)
    {
        InputSourceFactory = sourceFactory;
        ChannelCount = channelCount;
        DisposeLocalCopyAfterUpload = false;
        SampleRate = sampleRate;
        SampleCount = sampleCount;

        if (SampleRate == 0 || ChannelCount == 0 || SampleCount == 0)
            Duration = TimeSpan.Zero;
        else
            Duration = TimeSpan.FromSeconds(SampleCount / (double)sampleRate);
    }

    /// <summary>
    /// This does nothing because the stream is managed by the audio renderer
    /// </summary>
    public override void DisposeLocalCopy()
    {
    }

    /// <summary>
    /// Returns an empty span because this object has no knowledge of the data being read other than its source
    /// </summary>
    public override ReadOnlyMemory<float>? GetData() => ReadOnlyMemory<float>.Empty;
}
