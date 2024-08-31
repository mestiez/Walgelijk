using System.Numerics;

namespace Walgelijk.PortAudio.Voices;

internal class SharedStreamVoice : IVoice
{
    public readonly Sound Sound;

    private readonly StreamBuffer stream;
    private readonly float[] readBuffer = new float[2048];

    public SharedStreamVoice(Sound sound)
    {
        Sound = sound;
        if (Sound.Data is not StreamAudioData streamAudioData)
            throw new Exception("This voice only supports streaming audio data");

        stream = new StreamBuffer(streamAudioData.InputSourceFactory(), readBuffer);
    }

    public double Time { get => stream.Time; set => stream.Time = value; }
    public Vector3 Position { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsFinished => Time > Sound.Data.Duration.TotalSeconds;

    public void GetSamples(Span<float> frame)
    {

    }

    public void Pause()
    {
    }

    public void Play()
    {
    }

    public void Resume()
    {
    }

    public void Stop()
    {
    }
}
