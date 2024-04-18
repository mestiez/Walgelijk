namespace Walgelijk;

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
    /// Spatial settings for this sound. If null, the sound won't be spatial (used for e.g user interface sounds).
    /// </summary>
    public SpatialParams? SpatialParams = new(1, float.PositiveInfinity, 1);

    /// <summary>
    /// Pitch adjustment
    /// </summary>
    public float Pitch = 1;

    /// <summary>
    /// Volume adjustment
    /// </summary>
    public float Volume = 1;

    /// <summary>
    /// Currrent sound state
    /// </summary>
    public SoundState State = SoundState.Idle;

    /// <summary>
    /// The audio track this sound is playing on
    /// </summary>
    public AudioTrack? Track;

    public Sound(AudioData data, bool loops = false, SpatialParams? spatialParams = null, AudioTrack? track = null)
    {
        if (data.ChannelCount != 1 && spatialParams.HasValue)
            throw new global::System.Exception("The audio data is stereo but spatial parameters are present. These two traits are not compatible.");
        Data = data;
        Looping = loops;
        SpatialParams = spatialParams;
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
    public static readonly Sound Beep = new Sound(FixedAudioData.Beep, false, null);

    /// <summary>
    /// Calls <see cref="AudioRenderer.DisposeOf(Sound)"/>
    /// </summary>
    ~Sound()
    {
        // TODO this is kind of hacky
        Game.Main?.AudioRenderer?.DisposeOf(this);
    }
}
