using Walgelijk.PortAudio.Effects;

namespace Walgelijk.PortAudio.Voices;

internal class OneShotStreamVoice : StreamVoice
{
    public override bool IsFinished => isFisished;
    public override bool Looping { get => false; set { } }

    private bool isFisished = false;

    public OneShotStreamVoice(Sound sound) : base(sound)
    {
    }

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
        isFisished = true;
        State = SoundState.Stopped;
    }
}
