using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using Walgelijk.PortAudio.Effects;

namespace Walgelijk.PortAudio.Voices;

internal abstract class FixedVoice : IVoice
{
    public readonly ImmutableArray<float> Data;

    public List<IEffect> Effects { get; } = [];
    public Vector3 Position { get; set; }

    public abstract SoundState State { get; set; }
    public abstract bool IsFinished { get; }
    public abstract float Volume { get; set; }
    public abstract float Pitch { get; set; }
    public abstract bool Looping { get; set; }
    public abstract int ChannelCount { get; }
    public abstract AudioTrack? Track { get; }

    public double SampleIndex;

    protected FixedVoice(Sound sound)
    {
        if (sound.Data is not FixedAudioData fixedAudioData)
            throw new Exception("This voice only supports fixed audio data");

        Data = FixedBufferCache.Shared.Load(fixedAudioData);
    }

    internal FixedVoice(ImmutableArray<float> data)
    {
        Data = data;
    }

    public double Time
    {
        get => SampleIndex * PortAudioRenderer.SecondsPerSample;

        set
        {
            double totalFrames = Data.Length / ChannelCount;
            SampleIndex = double.Clamp(value / PortAudioRenderer.SecondsPerSample, 0, totalFrames - 1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void GetSamples(Span<float> frame)
    {
        if (State is not SoundState.Playing)
            return;

        var pitch = (Track?.Pitch ?? 1) * Pitch;
        if (pitch == 0)
            return; // avoid division by zero or no progression

        var volume = (Track?.Volume ?? 1) * Volume;

        var totalChannels = ChannelCount;
        var totalFrames = Data.Length / totalChannels;

        int frameCount = frame.Length / totalChannels;

        double sampleIndex = SampleIndex; 

        for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
        {
            if (sampleIndex >= totalFrames)
            {
                if (Looping)
                    sampleIndex %= totalFrames; 
                else
                {
                    Stop();
                    return;
                }
            }

            var currentFrameIndex = (int)sampleIndex;
            var nextFrameIndex = currentFrameIndex + 1;

            if (nextFrameIndex >= totalFrames)
                nextFrameIndex = Looping ? 0 : currentFrameIndex;

            var fraction = (float)(sampleIndex - currentFrameIndex);

            for (int channel = 0; channel < totalChannels; channel++)
            {
                var dataIndex = (currentFrameIndex * totalChannels) + channel;
                var nextDataIndex = (nextFrameIndex * totalChannels) + channel;

                var sampleCurrent = Data[dataIndex];
                var sampleNext = Data[nextDataIndex];

                float sampleInterpolated = float.Lerp(sampleCurrent, sampleNext, fraction);
                float sampleValue = sampleInterpolated * volume;

                var outputIndex = (frameIdx * totalChannels) + channel;

                frame[outputIndex] += sampleValue;
            }

            sampleIndex += pitch;
        }

        SampleIndex = sampleIndex;

        var t = TimeSpan.FromSeconds(Time);
        foreach (var item in Effects)
            item.Process(frame, (ulong)SampleIndex, t);
    }

    public abstract void Play();
    public abstract void Pause();
    public abstract void Stop();
}
