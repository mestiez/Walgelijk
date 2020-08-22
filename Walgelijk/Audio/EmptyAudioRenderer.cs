using System;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Placeholder audio renderer that is used when none is set
    /// </summary>
    internal sealed class EmptyAudioRenderer : AudioRenderer
    {
        public override float Volume { get; set; }
        public override bool Muted { get; set; }
        public override Vector2 ListenerPosition { get; set; }

        public override Sound LoadSound(string path) { return new Sound(Array.Empty<float>(), 1, 1); }

        public override void Pause(ref Sound sound) { }

        public override void Play(ref Sound sound) { }

        public override void Play(ref Sound sound, Vector2 worldPosition) { }

        public override void PlayOnce(Sound sound, Vector2 worldPosition) { }

        public override void PlayOnce(Sound sound) { }

        public override void Stop(ref Sound sound) { }

        public override void StopAll() { }
    }
}
