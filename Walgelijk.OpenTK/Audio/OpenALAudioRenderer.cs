using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.OpenTK
{
    public class OpenALAudioRenderer : AudioRenderer
    {
        private ALDevice device;
        private ALContext context;
        private readonly bool canPlayAudio = false;
        private readonly List<TemporarySource> temporarySources = new();

        public override float Volume
        {
            get
            {
                AL.GetListener(ALListenerf.Gain, out var gain);
                return gain;
            }

            set => AL.Listener(ALListenerf.Gain, value);
        }


        public override bool Muted { get => Volume <= float.Epsilon; set => Volume = 0; }

        public override Vector3 ListenerPosition
        {
            get
            {
                AL.GetListener(ALListener3f.Position, out float x, out float depth, out float z);
                return new Vector3(x, z, depth);
            }

            set => AL.Listener(ALListener3f.Position, value.X, value.Z, value.Y);
        }

        public OpenALAudioRenderer()
        {
            Resources.RegisterType(typeof(AudioData), LoadSound);

            device = ALC.OpenDevice(null);

            if (device == ALDevice.Null)
                Logger.Warn("No audio device could be found", this);

            context = ALC.CreateContext(device, new ALContextAttributes());
            if (context == ALContext.Null)
                Logger.Warn("No audio context could be created", this);

            bool couldSetContext = ALC.MakeContextCurrent(context);

            canPlayAudio = device != ALDevice.Null && context != ALContext.Null && couldSetContext;

            if (!couldSetContext)
                Logger.Warn("The audio context could not be set", this);

            if (!canPlayAudio)
                Logger.Error("Failed to initialise the audio renderer because of all of the above", this);
        }

        private static void UpdateIfRequired(Sound sound, out int source)
        {
            source = AudioObjects.Sources.Load(sound);

            if (!sound.RequiresUpdate)
                return;

            sound.RequiresUpdate = false;
            AL.Source(source, ALSourceb.SourceRelative, !sound.Spatial);
            AL.Source(source, ALSourceb.Looping, sound.Looping);
            AL.Source(source, ALSourcef.RolloffFactor, sound.RolloffFactor);
            AL.Source(source, ALSourcef.Pitch, sound.Pitch);
        }

        public override AudioData LoadSound(string path)
        {
            var ext = path.AsSpan()[path.LastIndexOf('.')..];
            AudioFileData data;

            try
            {
                if (ext.SequenceEqual(".wav"))
                    data = WaveFileReader.Read(path);
                else if (ext.SequenceEqual(".ogg"))
                    data = VorbisFileReader.Read(path);
                else
                    throw new Exception($"This is not a supported audio file. Only Microsoft WAV and Ogg Vorbis can be decoded.");
            }
            catch (Exception e)
            {
                throw new AggregateException($"Failed to load WAVE file: {path}", e);
            }

            var audio = new AudioData(data.Data, data.SampleRate, data.NumChannels);
            return audio;
        }

        public override void Pause(Sound sound)
        {
            if (!canPlayAudio || sound.Data == null)
                return;

            UpdateIfRequired(sound, out int id);
            AL.SourcePause(id);
        }

        public override void Play(Sound sound, float volume = 1)
        {
            if (!canPlayAudio || sound.Data == null)
                return;

            UpdateIfRequired(sound, out int s);
            SetVolume(sound, 1);
            AL.SourcePlay(s);
        }

        public override void Play(Sound sound, Vector2 worldPosition, float volume = 1)
        {
            if (!canPlayAudio || sound.Data == null)
                return;

            UpdateIfRequired(sound, out int s);
            SetVolume(sound, 1);
            if (sound.Spatial)
                AL.Source(s, ALSource3f.Position, worldPosition.X, 0, worldPosition.Y);
            else
                Logger.Warn("Attempt to play a non-spatial sound in space!");
            AL.SourcePlay(s);
        }

        private int CreateTempSource(Sound sound, float volume, Vector2 worldPosition, float pitch)
        {
            var source = SourceCache.CreateSourceFor(sound);
            AL.Source(source, ALSourceb.SourceRelative, !sound.Spatial);
            AL.Source(source, ALSourceb.Looping, false);
            AL.Source(source, ALSourcef.Gain, volume);
            AL.Source(source, ALSourcef.Pitch, pitch);
            if (sound.Spatial)
                AL.Source(source, ALSource3f.Position, worldPosition.X, 0, worldPosition.Y);
            AL.SourcePlay(source);
            temporarySources.Add(new TemporarySource
            {
                CurrentLifetime = 0,
                Duration = (float)sound.Data.Duration.TotalSeconds,
                Sound = sound,
                Source = source
            });
            return source;
        }

        public override void PlayOnce(Sound sound, float volume = 1, float pitch = 1)
        {
            if (!canPlayAudio || sound.Data == null)
                return;

            UpdateIfRequired(sound, out _);
            CreateTempSource(sound, volume, default, pitch);
        }

        public override void PlayOnce(Sound sound, Vector2 worldPosition, float volume = 1, float pitch = 1)
        {
            if (!canPlayAudio || sound.Data == null)
                return;

            UpdateIfRequired(sound, out _);
            if (!sound.Spatial)
                Logger.Warn("Attempt to play a non-spatial sound in space!");
            CreateTempSource(sound, volume, worldPosition, pitch);
        }

        public override void Stop(Sound sound)
        {
            if (!canPlayAudio || sound.Data == null)
                return;

            UpdateIfRequired(sound, out int s);
            AL.SourceStop(s);
        }

        public override void StopAll()
        {
            if (!canPlayAudio)
                return;

            foreach (var sound in AudioObjects.Sources.GetAllLoaded())
                AL.SourceStop(sound);

            foreach (var item in temporarySources)
            {
                AL.SourceStop(item.Source);
                item.CurrentLifetime = float.MaxValue;
            }
        }

        public override void Release()
        {
            if (device != ALDevice.Null)
                ALC.CloseDevice(device);

            if (context != ALContext.Null)
            {
                ALC.MakeContextCurrent(ALContext.Null);
                ALC.DestroyContext(context);
            }
        }

        public override void Process(Game game)
        {
            if (!canPlayAudio)
                return;

            for (int i = temporarySources.Count - 1; i >= 0; i--)
            {
                var v = temporarySources[i];
                if (v.CurrentLifetime > v.Duration)
                {
                    AL.DeleteSource(v.Source);
                    temporarySources.Remove(v);
                }
            }

            foreach (var item in temporarySources)
            {
                item.CurrentLifetime += game.Time.UpdateDeltaTime;
            }
        }

        public override bool IsPlaying(Sound sound)
        {
            return AL.GetSourceState(AudioObjects.Sources.Load(sound)) == ALSourceState.Playing;
        }

        public override void SetVolume(Sound sound, float volume)
        {
            var s = AudioObjects.Sources.Load(sound);
            AL.Source(s, ALSourcef.Gain, volume);
        }

        public override void DisposeOf(AudioData audioData)
        {
            audioData.ForceClearData();
            AudioObjects.Buffers.Unload(audioData);
        }

        public override void DisposeOf(Sound sound)
        {
            AudioObjects.Sources.Unload(sound);
        }
    }
}
