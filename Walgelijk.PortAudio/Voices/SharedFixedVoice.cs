using Walgelijk.PortAudio.Effects;

namespace Walgelijk.PortAudio.Voices;

internal class SharedFixedVoice : FixedVoice
{
    private readonly Sound Sound;

    public SharedFixedVoice(Sound sound) : base(sound)
    {
        Sound = sound;
    }

    public override SoundState State { get => Sound.State; set => Sound.State = value; }
    public override bool IsFinished => Time >= Sound.Data.Duration.TotalSeconds - double.Epsilon;
    public override float Volume { get => Sound.Volume; set => Sound.Volume = value; }
    public override float Pitch { get => Sound.Pitch; set => Sound.Pitch = value; }
    public override bool Looping { get => Sound.Looping; set => Sound.Looping = value; }
    public override int ChannelCount => Sound.Data.ChannelCount;
    public override AudioTrack? Track => Sound.Track;

    public override void Pause()
    {
        Sound.State = SoundState.Paused;
    }

    public override void Play()
    {
        Sound.State = SoundState.Playing;
    }

    public override void Stop()
    {
        Sound.State = SoundState.Stopped;
        SampleIndex = 0;
    }
}
