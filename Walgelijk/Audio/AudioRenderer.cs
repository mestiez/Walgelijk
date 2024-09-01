using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk;

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
    /// Normalised orientation vectors of the listener in world space. Determines the position of your two "ears".
    /// </summary>
    public abstract (Vector3 Forward, Vector3 Up) ListenerOrientation { get; set; }

    /// <summary>
    /// Global multiplier for any spatial input. E.g any <c>worldPosition</c> parameter, or the listener position. This is used to align the audio world with your scene scale.
    /// 1 by default.
    /// </summary>
    public float SpatialMultiplier = 1;

    /// <summary>
    /// The distance model used for audio falloff
    /// </summary>
    public abstract AudioDistanceModel DistanceModel { get; set; }

    /// <summary>
    /// Returns true if the sound is currently being played. It does not consider temporary sources created using <see cref="PlayOnce(Sound, float?, float?, AudioTrack?)"/> or <see cref="PlayOnce(Sound, Vector3, float?, float?, AudioTrack?)"/>
    /// </summary>
    public abstract bool IsPlaying(Sound sound);

    /// <summary>
    /// Simply play a sound (or resumes, if paused)
    /// </summary>
    public abstract void Play(Sound sound, float volume = 1);

    /// <summary>
    /// Play a sound once and let it overlap itself
    /// </summary>
    public abstract void PlayOnce(Sound sound, float volume = 1, float pitch = 1, AudioTrack? track = null);

    /// <summary>
    /// Play sound at a position in the world (or resumes, if paused)
    /// </summary>
    public abstract void Play(Sound sound, Vector3 worldPosition, float volume = 1);

    /// <summary>
    /// Play sound at a position in the world and let it overlap itself
    /// </summary>
    public abstract void PlayOnce(Sound sound, Vector3 worldPosition, float volume = 1, float pitch = 1, AudioTrack? track = null);

    /// <summary>
    /// Populates the given array with the most recently played samples of the given sound.
    /// Returns the amount of samples.
    /// </summary>
    public abstract int GetCurrentSamples(Sound sound, Span<float> arr);

    /// <summary>
    /// Set world position of the sound (if it is spatial)
    /// </summary>
    public abstract void SetPosition(Sound sound, Vector3 worldPosition);

    /// <summary>
    /// Stop a specific sound
    /// </summary>
    public abstract void Stop(Sound sound);

    /// <summary>
    /// Pause a specific sound
    /// </summary>
    public abstract void Pause(Sound sound);

    /// <summary>
    /// Pause all sounds
    /// </summary>
    public abstract void PauseAll();

    /// <summary>
    /// Pause all sounds in a track
    /// </summary>
    public abstract void PauseAll(AudioTrack track);

    /// <summary>
    /// Resume all sounds 
    /// </summary>
    public abstract void ResumeAll();

    /// <summary>
    /// Resume all sounds in a track
    /// </summary>
    public abstract void ResumeAll(AudioTrack track);

    /// <summary>
    /// Stop all sounds
    /// </summary>
    public abstract void StopAll();

    /// <summary>
    /// Stop all sounds in the given track
    /// </summary>
    public abstract void StopAll(AudioTrack track);

    /// <summary>
    /// Set time in seconds for the sound if it is playing
    /// </summary>
    public abstract void SetTime(Sound sound, float seconds);

    /// <summary>
    /// Get time in seconds for the sound if it is playing
    /// </summary>
    public abstract float GetTime(Sound sound);

    /// <summary>
    /// Release all resources used by the audio engine
    /// </summary>
    public abstract void Release();

    /// <summary>
    /// This is called every frame by the main loop and allows the renderer to process things that it needs to process
    /// </summary>
    public abstract void Process(float dt);

    /// <summary>
    /// Release memory used by the given <see cref="AudioData"/>. <see cref="Sound"/>s using this data will become unusable.
    /// </summary>
    public abstract void DisposeOf(AudioData audioData);

    /// <summary>
    /// Release memory used by a sound. <b>This will not dispose of its <see cref="AudioData"/>!</b>
    /// </summary>
    public abstract void DisposeOf(Sound sound);

    /// <summary>
    /// Synchronise all tracks with their updated settings. Called by the loop.
    /// </summary>
    public abstract void UpdateTracks();
}
