using System.Numerics;
using System.Runtime.CompilerServices;

namespace Walgelijk.PortAudio.Voices;

internal class SharedFixedVoice : IVoice
{
    public readonly Sound Sound;
    public readonly float[] Data; 

    public SharedFixedVoice(Sound sound)
    {
        Sound = sound;
        if (Sound.Data is not FixedAudioData fixedAudioData)
            throw new Exception("This voice only supports fixed audio data");

        Data = FixedBufferCache.Shared.Load(fixedAudioData);
    }

    public double Time
    {
        get => SampleIndex * (PortAudioRenderer.SecondsPerSample / Sound.Data.ChannelCount);

        set
        {
            uint sampleIndex = uint.Clamp((uint)(value / (PortAudioRenderer.SecondsPerSample / Sound.Data.ChannelCount)), 0, (uint)Data.Length - 1);
            SampleIndex = sampleIndex;
        }
    }

    public uint SampleIndex;

    public Vector3 Position { get; set; }
    public bool IsVirtual { get; set; }

    public bool IsFinished => Time > Sound.Data.Duration.TotalSeconds;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetSamples(Span<float> frame)
    {
        if (Sound.State is not SoundState.Playing)
            return;

        var trackVolume = Sound.Track?.Volume ?? 1;

        for (int i = 0; i < frame.Length; i++)
        {
            var nextSample = SampleIndex + 1;

            if (!Sound.Looping && nextSample >= Data.Length)
            {
                Stop();
                return;
            }

            SampleIndex = nextSample % (uint)Data.Length;
            var v = Data[SampleIndex] * trackVolume;

            frame[i] += v;
            if (Sound.Data.ChannelCount == 1) // mono, so we should copy our sample to the next channel
                frame[++i] += v;
        }
    }

    public void Play()
    {
        Sound.State = SoundState.Playing;
    }

    public void Pause()
    {
        Sound.State = SoundState.Paused;
    }

    public void Stop()
    {
        Sound.State = SoundState.Stopped;
        SampleIndex = 0;
    }
}
