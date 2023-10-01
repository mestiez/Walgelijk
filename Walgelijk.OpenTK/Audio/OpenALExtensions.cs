using OpenTK.Audio.OpenAL;

namespace Walgelijk.OpenTK;

internal static class OpenALExtensions
{
    public static ALSourceState GetSourceState(this SourceHandle sid)
    {
        AL.GetSource(sid, ALGetSourcei.SourceState, out int s);

        return (ALSourceState)s;
    }
}