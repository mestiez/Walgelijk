using System.Numerics;

namespace Walgelijk.ParticleSystem;

public struct Particle
{
    public bool Active;
    public Vector2 Position;
    public float Rotation;
    public Color Color;
    public float Size;
    public Vector2 Velocity;
    public float RotationalVelocity;
    public float Life;
    public float MaxLife;
    public float Dampening;
    public float RotationalDampening;
    public Vector2 Acceleration;
    public float RotationalAcceleration;

    /// <summary>
    /// Size that can be processed by modules and is eventually rendered. It is set to the initial Size every frame, before module processing
    /// </summary>
    public float RenderedSize;
    /// <summary>
    /// Colour that can be processed by modules and is eventually rendered. It is set to the initial Color every frame, before module processing
    /// </summary>
    public Color RenderedColor;

    /// <summary>
    /// Set the Life to exceed MaxLife so that it gets deleted in the next update
    /// </summary>
    public void Delete() => Life = MaxLife + 1;
    /// <summary>
    /// Life / MaxLife
    /// </summary>
    public float NormalisedLife;

    public static readonly Material DefaultMaterial = ParticleMaterialInitialiser.CreateDefaultMaterial();
}

