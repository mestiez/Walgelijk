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
        /// Simply play a sound (or resumes, if paused)
        /// </summary>
        public abstract void Play(ref Sound sound);

        /// <summary>
        /// Play a sound once and let it overlap itself
        /// </summary>
        public abstract void PlayOnce(Sound sound);

        /// <summary>
        /// Play sound at a position in the world (or resumes, if paused)
        /// </summary>
        public abstract void Play(ref Sound sound, Vector2 worldPosition);

        /// <summary>
        /// Play sound at a position in the world and let it overlap itself
        /// </summary>
        public abstract void PlayOnce(Sound sound, Vector2 worldPosition);

        /// <summary>
        /// Stop a specific sound
        /// </summary>
        /// <param name="sound"></param>
        public abstract void Stop(ref Sound sound);

        /// <summary>
        /// Pause a specific sound
        /// </summary>
        /// <param name="sound"></param>
        public abstract void Pause(ref Sound sound);

        /// <summary>
        /// Stop all sounds
        /// </summary>
        public abstract void StopAll();

        /// <summary>
        /// Load a sound from file
        /// </summary>
        /// <returns></returns>
        public abstract Sound LoadSound(string path);
    }
}
