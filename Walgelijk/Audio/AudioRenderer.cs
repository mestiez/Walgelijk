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
        /// Position of the listener in world space
        /// </summary>
        public abstract Vector2 ListenerPosition { get; set; }

        /// <summary>
        /// Returns true if the sound is currently being played. It does not consider temporary sources created using <see cref="PlayOnce(Sound)"/>
        /// </summary>
        public abstract bool IsPlaying(Sound sound);

        /// <summary>
        /// Simply play a sound (or resumes, if paused)
        /// </summary>
        public abstract void Play(Sound sound);

        /// <summary>
        /// Play a sound once and let it overlap itself
        /// </summary>
        public abstract void PlayOnce(Sound sound);

        /// <summary>
        /// Play sound at a position in the world (or resumes, if paused)
        /// </summary>
        public abstract void Play(Sound sound, Vector2 worldPosition);

        /// <summary>
        /// Play sound at a position in the world and let it overlap itself
        /// </summary>
        public abstract void PlayOnce(Sound sound, Vector2 worldPosition);

        /// <summary>
        /// Stop a specific sound
        /// </summary>
        /// <param name="sound"></param>
        public abstract void Stop(Sound sound);

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
        public abstract AudioData LoadSound(string path);

        /// <summary>
        /// Release all resources used by the audio engine
        /// </summary>
        public abstract void Release();

        /// <summary>
        /// This is called every frame by the main loop and allows the renderer to process things that it needs to process
        /// </summary>
        public abstract void Process(Game game);
    }
}
