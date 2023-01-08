using OpenTK.Audio.OpenAL;

namespace Walgelijk.OpenTK;

public class SourceCache : Cache<Sound, SourceHandle>
{
    protected override SourceHandle CreateNew(Sound raw) => CreateSourceFor(raw);

    protected override void DisposeOf(SourceHandle loaded) => AL.DeleteSource(loaded);

    public Sound GetSoundFor(SourceHandle handle)
    {
        foreach (var item in Loaded)
            if (item.Value == handle)
                return item.Key;
        throw new global::System.Exception("Attempt to get a sound for a handle that does not exist");
    }

    internal static SourceHandle CreateSourceFor(Sound sound)
    {
        switch (sound.Data)
        {
            case FixedAudioData fixedData:
                {
                    var buffer = AudioObjects.FixedBuffers.Load(fixedData);
                    var source = AL.GenSource();
                    AL.Source(source, ALSourcei.Buffer, buffer);
                    return source;
                }
            case StreamAudioData streamData:
                var s = AL.GenSource();
                AudioObjects.OggStreamers.Load((s, sound));
                return s; 
        }

        throw new global::System.Exception("AudioData of type " + sound.Data.GetType().Name + $" is not supported. Only {nameof(FixedAudioData)} and {nameof(StreamAudioData)} are understood.");
    }
}
