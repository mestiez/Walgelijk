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

    public int SampleIndex;

    protected FixedVoice(Sound sound)
    {
        if (sound.Data is not FixedAudioData fixedAudioData)
            throw new Exception("This voice only supports fixed audio data");

        Data = FixedBufferCache.Shared.Load(fixedAudioData);
    }

    public double Time
    {
        get => SampleIndex * (PortAudioRenderer.SecondsPerSample / ChannelCount);

        set
        {
            int sampleIndex = int.Clamp((int)(value / (PortAudioRenderer.SecondsPerSample / ChannelCount)), 0, Data.Length - 1);
            SampleIndex = sampleIndex;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void GetSamples(Span<float> frame)
    {
        if (State is not SoundState.Playing)
            return;

        var volume = (Track?.Volume ?? 1) * Volume;

        for (int i = 0; i < frame.Length; i++)
        {
            var nextSample = SampleIndex + 1;

            if (!Looping && nextSample >= Data.Length)
            {
                Stop();
                return;
            }

            SampleIndex = nextSample % Data.Length;
            var v = Data[SampleIndex] * volume;
            frame[i] += v;
            if (ChannelCount == 1) // mono, so we should copy our sample to the next channel
                frame[++i] += v;
        }
    }

    public abstract void Play();
    public abstract void Pause();
    public abstract void Stop();
}
