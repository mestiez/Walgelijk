using OpenTK.Audio.OpenAL;

namespace Walgelijk.OpenTK;

internal static class OpenALExtensions
{
    public static ALSourceState GetALState(this SourceHandle sid)
    {
        return (ALSourceState)AL.GetSource(sid, ALGetSourcei.SourceState);
    }
}