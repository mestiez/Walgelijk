using System;

namespace Walgelijk;

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
            Duration = TimeSpan.FromSeconds(sampleCount / ChannelCount / (double)sampleRate);
    }

    public override void DisposeLocalCopy()
    {
        Data = Array.Empty<byte>();
    }

    public override ReadOnlyMemory<byte>? GetData() => Data;

    /// <summary>
    /// Beep audio data
    /// </summary>
    public static readonly FixedAudioData Beep = AudioGen.SignalGenerator(AudioGen.Sine(0.2f, 750), 44100, TimeSpan.FromSeconds(0.5f)).FadeOut();
}
