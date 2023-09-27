namespace Walgelijk.OpenTK;

internal readonly struct TemporarySourceArgs
{
    public readonly int Source;
    public readonly Sound Sound;
    public readonly float Duration;
    public readonly float Volume;
    public readonly AudioTrack? Track;

    public TemporarySourceArgs(int source, Sound sound, float duration, float volume, AudioTrack track)
    {
        Source = source;
        Sound = sound;
        Duration = duration;
        Volume = volume;
        Track = track;
    }
}
