using System.Numerics;

namespace Walgelijk.ParticleSystem
{
    [RequiresComponents(typeof(TransformComponent))]
    public class ParticlesComponent
    {
        public readonly int MaxParticleCount = 1000;
        public int CurrentParticleCount = 0;

        public Particle[] RawParticleArray;

        public ParticlesComponent(int maxCount = 1000)
        {
            MaxParticleCount = maxCount;
            RawParticleArray = new Particle[MaxParticleCount];

            VertexBuffer = new VertexBuffer();
            RenderTask = new ShapeRenderTask(VertexBuffer);
        }

        public Material Material = Particle.DefaultMaterial;

        public Vec2Range Gravity = new Vec2Range(new Vector2(0, -9.81f));
        public FloatRange SpawnRadius = new FloatRange(0.2f, 1f);
        public FloatRange LifeRange = new FloatRange(1, 3);

        public FloatRange StartSize = new FloatRange(1);
        public FloatRange StartRotation = new FloatRange(0);
        public Vec2Range StartVelocity = new Vec2Range(Vector2.One * -4, Vector2.One * 4);
        public FloatRange StartRotationalVelocity = new FloatRange(-4, 4);
        public ColorRange StartColor = new ColorRange(Color.White);
        public FloatRange Dampening = new FloatRange(0.1f);
        public FloatRange RotationalDampening = new FloatRange(0.1f);

        public FloatCurve SizeOverLife = new FloatCurve(new Curve<float>.Key(1, 1));

        public float SimulationSpeed = 1;
        public bool WorldSpace;
        public float EmissionRate = 150;
        public bool CircularStartVelocity = false;

        public ShapeRenderTask RenderTask;
        public VertexBuffer VertexBuffer;

        public float Clock;
    }
}
