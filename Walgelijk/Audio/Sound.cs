namespace Walgelijk;

/// <summary>
/// States that a sound can be in
/// </summary>
public enum SoundState
{
    /// <summary>
    /// Untouched by audio engine.
    /// </summary>
    Idle,
    /// <summary>
    /// Currently playing.
    /// </summary>
    Playing,
    /// <summary>
    /// Not playing but not stopped either. This sound can be resumed.
    /// </summary>
    Paused,
    /// <summary>
    /// 
    /// </summary>
    Stopped
}

/// <summary>
/// A sound that can be played. It does not contain audio data, but is instead linked to an <see cref="AudioData"/>
/// </summary>
public class Sound
{
    /// <summary>
    /// Reference to the actual audio data that this sound plays. This is a shared object, so other sounds that share this data will also sound different if you manipulate it
    /// </summary>
    public AudioData Data;

    /// <summary>
    /// Determines if the sound should loop
    /// </summary>
    public bool Looping;

    /// <summary>
    /// Rolloff factor if this sound is played in space
    /// </summary>
    public float RolloffFactor = 1;

    /// <summary>
    /// Pitch adjustment
    /// </summary>
    public float Pitch = 1;

    /// <summary>
    /// Volume adjustment
    /// </summary>
    public float Volume = 1;

    /// <summary>
    /// Is the sound affected by the listener position? True by default. Set to false for things like UI SFX.
    /// </summary>
    public bool Spatial = true;

    /// <summary>
    /// Currrent sound state
    /// </summary>
    public SoundState State = SoundState.Idle;

    /// <summary>
    /// The audio track this sound is playing on
    /// </summary>
    public AudioTrack? Track;

    public Sound(AudioData data, bool loops = false, bool spatial = true, AudioTrack? track = null)
    {
        Data = data;
        Looping = loops;
        Spatial = spatial;
        Track = track;
    }

    /// <summary>
    /// Should the audio engine update this sound instance? Should be set to true after a property change.
    /// </summary>
    public bool RequiresUpdate = true;

    /// <summary>
    /// Sets <see cref="RequiresUpdate"/> to true, forcing the audio engine to synchronise the property change
    /// </summary>
    public void ForceUpdate()
    {
        RequiresUpdate = true;
    }

    /// <summary>
    /// Beep sound
    /// </summary>
    public static readonly Sound Beep = new Sound(FixedAudioData.Beep, false, false);
}
