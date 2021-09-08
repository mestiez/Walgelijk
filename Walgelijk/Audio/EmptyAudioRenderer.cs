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

        public override AudioData LoadSound(string path) { return new AudioData(Array.Empty<byte>(), 0, 0); }

        public override void Pause(Sound sound) { }

        public override void Play(Sound sound) { }

        public override void Play(Sound sound, Vector2 worldPosition) { }

        public override void PlayOnce(Sound sound, Vector2 worldPosition) { }

        public override void PlayOnce(Sound sound) { }

        public override void Stop(Sound sound) { }

        public override void StopAll() { }

        public override void Release() { }

        public override void Process(Game game) { }
    }
}
