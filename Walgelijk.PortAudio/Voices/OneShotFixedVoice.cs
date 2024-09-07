namespace Walgelijk.PortAudio.Voices;

internal class OneShotFixedVoice : FixedVoice
{
    public override bool IsFinished => isFinished;
    private bool isFinished;
    private double duration;

    public OneShotFixedVoice(Sound sound) : base(sound)
    {
        duration = sound.Data.Duration.TotalSeconds;
        ChannelCount = sound.Data.ChannelCount;
        Track = sound.Track;
    }

    public override float Volume { get; set; }
    public override float Pitch { get; set; }
    public override SoundState State { get; set; }

    public override bool Looping { get => false; set { } }
    public override int ChannelCount { get; }
    public override AudioTrack? Track { get; }

    public override void Pause()
    {
        State = SoundState.Paused;
    }

    public override void Play()
    {
        State = SoundState.Playing;
    }

    public override void Stop()
    {
        State = SoundState.Stopped;
        isFinished = true;
    }
}
