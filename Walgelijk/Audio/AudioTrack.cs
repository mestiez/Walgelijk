using System;

namespace Walgelijk;

/// <summary>
/// Track of sounds that.
/// </summary>
public class AudioTrack
{
    private bool muted = false;
    private float volume = 1;
    private float pitch = 1;

    /// <summary>
    /// Have the properties of this track changed? 
    /// </summary>
    public bool RequiresUpdate = true;

    public AudioTrack(float volume = 1, float pitch = 1, bool muted = false)
    {
        Volume = volume;
        Muted = muted;
        Pitch = pitch;
    }

    /// <summary>
    /// Mute flag.
    /// </summary>
    public bool Muted
    {
        get => muted;

        set
        {
            RequiresUpdate = muted != value;
            muted = value;
        }
    }

    /// <summary>
    /// Volume from 0.0 to 1.0, inclusive
    /// </summary>
    public float Volume
    {
        get => volume;

        set
        {
            if (RequiresUpdate = MathF.Abs(volume - value) > 0.00001f)
                volume = value;
        }
    }

    /// <summary>
    /// Pitch value where 1 is no pitch change
    /// </summary>
    public float Pitch
    {
        get => pitch;

        set
        {
            if (RequiresUpdate = MathF.Abs(pitch - value) > 0.00001f)
                pitch = value;
        }
    }
}
