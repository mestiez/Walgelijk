using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// The global audio renderer 
    /// </summary>
    public abstract class AudioRenderer
    {
        /// <summary>
        /// Master volume. Ranges from 0.0 to 1.0
        /// </summary>
        public abstract float Volume { get; set; }

        /// <summary>
        /// Mute all audio
        /// </summary>
        public abstract bool Muted { get; set; }

        /// <summary>
        /// Set the audio device. This may reset the audio context. NULL to fallback to the default audio device.
        /// </summary>
        public abstract void SetAudioDevice(string device);

        /// <summary>
        /// Returns the audio device that's currently being used
        /// </summary>
        public abstract string GetCurrentAudioDevice();

        /// <summary>
        /// Enumerate through the available audio devices
        /// </summary>
        public abstract IEnumerable<string> EnumerateAvailableAudioDevices();

        /// <summary>
        /// Position of the listener in world space
        /// </summary>
        public abstract Vector3 ListenerPosition { get; set; }

        /// <summary>
        /// Returns true if the sound is currently being played. It does not consider temporary sources created using <see cref="PlayOnce(Sound)"/>
        /// </summary>
        public abstract bool IsPlaying(Sound sound);

        /// <summary>
        /// Simply play a sound (or resumes, if paused)
        /// </summary>
        public abstract void Play(Sound sound, float volume = 1);

        /// <summary>
        /// Play a sound once and let it overlap itself
        /// </summary>
        public abstract void PlayOnce(Sound sound, float volume = 1, float pitch = 1);

        /// <summary>
        /// Play sound at a position in the world (or resumes, if paused)
        /// </summary>
        public abstract void Play(Sound sound, Vector2 worldPosition, float volume = 1);

        /// <summary>
        /// Play sound at a position in the world and let it overlap itself
        /// </summary>
        public abstract void PlayOnce(Sound sound, Vector2 worldPosition, float volume = 1, float pitch = 1);

        /// <summary>
        /// Stop a specific sound
        /// </summary>
        /// <param name="sound"></param>
        public abstract void Stop(Sound sound);

        /// <summary>
        /// Set the volume of a specific sound. Does not affect sounds created using <see cref="PlayOnce(Sound, float)"/>
        /// </summary>
        public abstract void SetVolume(Sound sound, float volume);

        /// <summary>
        /// Pause a specific sound
        /// </summary>
        /// <param name="sound"></param>
        public abstract void Pause(Sound sound);

        /// <summary>
        /// Stop all sounds
        /// </summary>
        public abstract void StopAll();

        /// <summary>
        /// Load a sound from file
        /// </summary>
        /// <returns></returns>
        public abstract AudioData LoadSound(string path, bool streaming = false);

        /// <summary>
        /// Release all resources used by the audio engine
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// This is called every frame by the main loop and allows the renderer to process things that it needs to process
        /// </summary>
        public abstract void Process(Game game);

        /// <summary>
        /// Release memory used by the given <see cref="AudioData"/>. <see cref="Sound"/>s using this data will become unusable.
        /// </summary>
        public abstract void DisposeOf(AudioData audioData);

        /// <summary>
        /// Release memory used by a sound. <b>This will not dispose of its <see cref="AudioData"/>!</b>
        /// </summary>
        public abstract void DisposeOf(Sound sound);
    }
}
