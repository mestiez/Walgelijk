using System.Numerics;
using Walgelijk.PortAudio.Effects;

namespace Walgelijk.PortAudio.Voices;

abstract class StreamVoice : IVoice
{
    public const int BufferSize = 2048 * 4;

    public readonly Sound Sound;
    public List<IEffect> Effects { get; } = [];

    protected readonly StreamBuffer StreamBuffer;

    public StreamVoice(Sound sound)
    {
        Sound = sound;
        if (Sound.Data is not StreamAudioData streamAudioData)
            throw new Exception("This voice only supports streaming audio data");

        StreamBuffer = new StreamBuffer(streamAudioData.InputSourceFactory(), BufferSize);
    }

    public double Time { get => StreamBuffer.Time; set => StreamBuffer.Time = value; }
    public Vector3 Position { get; set; }

    public SoundState State { get => Sound.State; set => Sound.State = value; }
    public float Volume { get => Sound.Volume; set => Sound.Volume = value; }
    public float Pitch { get => Sound.Pitch; set => Sound.Pitch = value; }
    public int ChannelCount => Sound.Data.ChannelCount;
    public AudioTrack? Track => Sound.Track;
    public abstract bool IsFinished { get; }
    public abstract bool Looping { get; set; }

    private readonly CircularBuffer<float> lastPlayedFrame = new CircularBuffer<float>(PortAudioRenderer.FramesPerBuffer * PortAudioRenderer.ChannelCount);
    private readonly float[] acc = new float[PortAudioRenderer.FramesPerBuffer * PortAudioRenderer.ChannelCount];
    private readonly SemaphoreSlim l = new(1);

    public void GetSamples(Span<float> frame)
    {
        if (State is not SoundState.Playing)
            return;

        var pitch = (Track?.Pitch ?? 1) * Pitch;
        if (pitch == 0)
            return; // avoid division by zero or no progression

        var volume = (Track?.Volume ?? 1) * Volume;

        if (StreamBuffer.Time >= Sound.Data.Duration.TotalSeconds)
        {
            if (Sound.Looping)
                StreamBuffer.Time = 0;
            else
            {
                Stop();
                return;
            }
        }

        Array.Clear(acc);

        // fill the buffer with the next samples
        StreamBuffer.GetSamples(acc, volume, pitch);

        var t = TimeSpan.FromSeconds(Time);
        foreach (var item in Effects)
            item.Process(acc, StreamBuffer.SampleIndex, t);

        l.Wait();
        try
        {
            lastPlayedFrame.Write(acc);
        }
        finally
        {
            l.Release();
        }

        acc.CopyTo(frame);
    }

    public abstract void Play();
    public abstract void Pause();
    public abstract void Stop();

    public int GetMostRecentlyPlayed(Span<float> frame)
    {
        l.Wait();
        try
        {
            int idx = 0;
            for (int i = 0; i < lastPlayedFrame.Capacity; i++)
            {
                frame[idx++] = lastPlayedFrame.Read();
                if (idx >= frame.Length)
                    return idx;
            }
            return idx;
        }
        finally
        {
            l.Release();
        }
    }
}
