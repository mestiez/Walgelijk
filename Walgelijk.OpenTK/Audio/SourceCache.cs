using OpenTK.Audio.OpenAL;

namespace Walgelijk.OpenTK
{
    public class SourceCache : Cache<Sound, int>
    {
        protected override int CreateNew(Sound raw) => CreateSourceFor(raw);

        protected override void DisposeOf(int loaded) => AL.DeleteSource(loaded);

        internal static int CreateSourceFor(Sound sound)
        {
            var buffer = AudioObjects.Buffers.Load(sound.Data);
            var source = AL.GenSource();
            AL.Source(source, ALSourcei.Buffer, buffer);
            return source;
        }
    }
}
