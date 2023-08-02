using OpenTK.Audio.OpenAL;

namespace Walgelijk.OpenTK;

public class TemporarySource
{
    public int Source;
    public Sound? Sound;
    public float Duration;
    public float CurrentLifetime;
    public float Volume;
    public AudioTrack? Track;
}

internal static class OpenALExtensions
{
    public static ALSourceState GetSourceState(this SourceHandle sid)
    {
        AL.GetSource(sid, ALGetSourcei.SourceState, out int s);
        return (ALSourceState)s;
    }
}