using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Empty audio renderer that is used when none is set
    /// </summary>
    internal sealed class EmptyAudioRenderer : AudioRenderer
    {
        public override float Volume { get; set; }
        public override bool Muted { get; set; }
        public override Vector3 ListenerPosition { get; set; }

        public override AudioData LoadSound(string path, bool streaming = false) { return new AudioData(Array.Empty<byte>(), 0, 0, 0); }

        public override void Pause(Sound sound) { }

        public override void Stop(Sound sound) { }

        public override void StopAll() { }

        public override void Release() { }

        public override void Process(Game game) { }

        public override bool IsPlaying(Sound sound) => false;

        public override void DisposeOf(AudioData audioData) => audioData.DisposeLocalCopy();

        public override void DisposeOf(Sound sound) { }

        public override void SetAudioDevice(string device) { }

        public override string GetCurrentAudioDevice() => string.Empty;

        public override IEnumerable<string> EnumerateAvailableAudioDevices()
        {
            yield break;
        }

        public override void Play(Sound sound, float volume = 1) { }

        public override void PlayOnce(Sound sound, float volume = 1, float pitch = 1, AudioTrack? track = null) { }

        public override void Play(Sound sound, Vector2 worldPosition, float volume = 1l) { }

        public override void PlayOnce(Sound sound, Vector2 worldPosition, float volume = 1, float pitch = 1, AudioTrack? track = null) { }

        public override void PauseAll() { }

        public override void PauseAll(AudioTrack track) { }

        public override void ResumeAll() { }

        public override void StopAll(AudioTrack track) { }

        public override void UpdateTracks() { }

        public override void ResumeAll(AudioTrack track) { }

        public override void SetTime(Sound sound, float seconds) { }

        public override float GetTime(Sound sound) => 0;
    }
}
