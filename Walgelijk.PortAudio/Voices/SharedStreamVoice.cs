namespace Walgelijk.PortAudio.Voices;

internal class SharedStreamVoice : StreamVoice
{
    public SharedStreamVoice(Sound sound) : base(sound)
    {
    }

    public override bool IsFinished => Time >= Sound.Data.Duration.TotalSeconds - double.Epsilon;
    public override bool Looping { get => Sound.Looping; set => Sound.Looping = value; }

    public override void Play()
    {
        Sound.State = SoundState.Playing;
    }

    public override void Pause()
    {
        Sound.State = SoundState.Paused;
    }

    public override void Stop()
    {
        Sound.State = SoundState.Stopped;
        StreamBuffer.Clear();
    }
}
