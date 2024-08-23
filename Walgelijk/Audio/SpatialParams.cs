namespace Walgelijk;

/// <summary>
/// Options for spatial sounds
/// </summary>
public struct SpatialParams
{
    /// <summary>
    /// The midpoint of the rolloff curve. In other words, the distance at which the sound will be at half volume according to the distance model used.
    /// If this is 0, no attenuation will occur at all.
    /// Note how this value is ultimately transformed by <see cref="AudioRenderer.SpatialMultiplier"/>
    /// <br></br>
    /// Default is <c>1</c>
    /// </summary>
    public float ReferenceDistance;

    /// <summary>
    /// The distance at which the sound will no longer get any quieter. 
    /// If this is 0, no attenuation will occur at all.
    /// Note how this value is ultimately transformed by <see cref="AudioRenderer.SpatialMultiplier"/>
    /// <br></br>
    /// Default is <c>float.PositiveInfinity</c>
    /// </summary>
    public float MaxDistance;

    /// <summary>
    /// A higher rolloff factor will make the sound reduce in volume more quickly as the listener moves away from the source, while a lower rolloff factor makes the sound reduce in volume more slowly.
    /// If this is 0, no attenuation will occur at all.
    /// <br></br>
    /// Default is <c>1</c>
    /// </summary>
    public float RolloffFactor;

    public SpatialParams(float referenceDistance = 1, float maxDistance = float.PositiveInfinity, float rolloffFactor = 1)
    {
        ReferenceDistance = referenceDistance;
        MaxDistance = maxDistance;
        RolloffFactor = rolloffFactor;
    }

    public SpatialParams()
    {
        ReferenceDistance = 1;
        MaxDistance = float.PositiveInfinity;
        RolloffFactor = 1;
    }
}
