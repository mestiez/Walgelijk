using System.Numerics;

namespace Walgelijk.ParticleSystem
{
    public struct Particle
    {
        public bool Active;

        public Vector2 Position;
        public float Angle;

        public Color InitialColor;
        public Color Color;

        public float InitialSize;
        public float Size;

        public Vector2 Velocity;
        public float RotationalVelocity;

        public Vector2 Gravity;

        public float Life;
        public float MaxLife;

        public float Dampening;
        public float RotationalDampening;

        public static readonly Material DefaultMaterial = ParticleMaterialInitialiser.CreateDefaultMaterial();
    }
}
