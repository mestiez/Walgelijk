using NAudio.Wave;

namespace Walgelijk.NAudio
{
    public struct LoadedSound
    {
        public Sound Sound;
        public WaveFormat WaveFormat;
        public SoundSampleProvider SampleProvider;
    }
}
