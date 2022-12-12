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

    public static readonly Material DefaultMaterial = ParticleMaterialInitialiser.CreateDefaultMaterial();

    public Vector2 Acceleration;
    public float RotationalAcceleration;

    public void Delete() => Life = MaxLife + 1;
}
