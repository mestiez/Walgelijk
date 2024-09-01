using System.Numerics;

namespace Walgelijk.PortAudio.Voices;

internal class SharedStreamVoice : IVoice
{
    public const int BufferSize = 2048;

    public readonly Sound Sound;

    private readonly StreamBuffer stream;

    public SharedStreamVoice(Sound sound)
    {
        Sound = sound;
        if (Sound.Data is not StreamAudioData streamAudioData)
            throw new Exception("This voice only supports streaming audio data");

        stream = new StreamBuffer(streamAudioData.InputSourceFactory(), BufferSize);
    }

    public double Time { get => stream.Time; set => stream.Time = value; }
    public Vector3 Position { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsFinished => Time >= Sound.Data.Duration.TotalSeconds;

    public void GetSamples(Span<float> frame)
    {
        if (Sound.State == SoundState.Playing)
        {
            if (stream.Time >= Sound.Data.Duration.TotalSeconds)
            {
                if (Sound.Looping)
                    stream.Time = 0;
                else
                {
                    Stop();
                    return;
                }
            }

            stream.GetSamples(frame);
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
        stream.Clear();
    }
}
