using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Linq;
using System.Numerics;

namespace Walgelijk.NAudio
{
    public class NAudioRenderer : AudioRenderer
    {
        public const int SampleRate = 44100;
        public const int ChannelCount = 2;

        private readonly IWavePlayer output;
        private readonly CustomMixingSampleProvider mixer;
        private readonly AudioCache audioCache = new AudioCache();

        public NAudioRenderer()
        {
            mixer = new CustomMixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, ChannelCount));
            output = new WaveOutEvent();
            mixer.ReadFully = true;
            output.Init(mixer);
            output.Play();
            mixer.MixerInputEnded += OnSoundEnd;
        }

        private void OnSoundEnd(object sender, SampleProviderEventArgs e)
        {
            SoundSampleProvider soundSampleProvider = e.SampleProvider as SoundSampleProvider;
            var sound = soundSampleProvider.LoadedSound.Sound;
            if (audioCache.Has(sound))
                audioCache.Unload(sound);
        }

        public override float Volume { get => mixer.Volume; set => mixer.Volume = value; }
        public override bool Muted { get => mixer.Muted; set => mixer.Muted = value; }

        public override Vector2 ListenerPosition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Sound LoadSound(string path)
        {
            return NAudioLoader.LoadFromFile(path);
        }

        public override void Play(ref Sound sound)
        {
            var loaded = audioCache.Load(sound);
            if (mixer.MixerInputs.Contains(loaded.SampleProvider))
            {
                mixer.RemoveMixerInput(loaded.SampleProvider);
                audioCache.Unload(sound);
                Play(ref sound);
                return;
            }
            mixer.AddMixerInput(loaded.SampleProvider);
        }

        public override void PlayOnce(Sound sound)
        {
            var loaded = audioCache.Load(sound);
            mixer.AddMixerInput(new SoundSampleProvider(loaded));
        }

        public override void Play(ref Sound sound, Vector2 worldPosition)
        {
            var loaded = audioCache.Load(sound);
            throw new NotImplementedException();
        }

        public override void PlayOnce(Sound sound, Vector2 worldPosition)
        {
            var loaded = audioCache.Load(sound);
            throw new NotImplementedException();
        }

        public override void Pause(ref Sound sound)
        {
            var loaded = audioCache.Load(sound);
            mixer.RemoveMixerInput(loaded.SampleProvider);
        }

        public override void Stop(ref Sound sound)
        {
            var loaded = audioCache.Load(sound);
            mixer.RemoveMixerInput(loaded.SampleProvider);
            audioCache.Unload(sound);
        }

        public override void StopAll()
        {
            mixer.RemoveAllMixerInputs();
            audioCache.UnloadAll();
        }
    }
}
