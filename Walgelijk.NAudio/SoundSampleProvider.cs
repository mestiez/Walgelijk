using NAudio.Wave;
using System;

namespace Walgelijk.NAudio
{
    public class SoundSampleProvider : ISampleProvider
    {
        public readonly LoadedSound LoadedSound;
        private long position;

        public SoundSampleProvider(LoadedSound sound)
        {
            this.LoadedSound = sound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            //Ik heb geen idee wat hier gaande is maar audio is zooooooo saai dat ik het gewoon gebruik
            long length = LoadedSound.Sound.Data.LongLength;
            var availableSamples = length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(LoadedSound.Sound.Data, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            if (LoadedSound.Sound.Looping && position + count >= length)
                position = 0;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return LoadedSound.WaveFormat; } }
    }
}
