using NAudio.Wave;

namespace Walgelijk.NAudio
{
    public class AudioCache : Cache<Sound, LoadedSound>
    {
        protected override LoadedSound CreateNew(Sound raw)
        {
            var loaded = new LoadedSound
            {
                Sound = raw,
                WaveFormat = new WaveFormat(raw.SampleRate, raw.Channels)
            };

            loaded.SampleProvider = new SoundSampleProvider(loaded);

            return loaded;
        }

        protected override void DisposeOf(LoadedSound loaded) { }
    }
}
