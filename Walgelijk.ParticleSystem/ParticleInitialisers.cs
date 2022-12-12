using System.Numerics;

namespace Walgelijk.ParticleSystem.ParticleInitialisers;

public struct PointShapeInitialiser : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public Vector2 Point;

    public PointShapeInitialiser(Vector2 pos) : this()
    {
        Disabled = false;
        Point = pos;
    }

    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.Position += Point;
    }
}

public struct CircleShapeInitialiser : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public Vector2 Point;
    public FloatRange Radius;

    public FloatRange VelocityIntensity = new(0, 1);

    /// <summary>
    /// Spawn on edge?
    /// </summary>
    public bool OnEdge = false;

    public CircleShapeInitialiser(Vector2 point, FloatRange radius) : this()
    {
        Disabled = false;
        Point = point;
        Radius = radius;
    }

    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        var r = Radius.GetRandom();
        var v = VelocityIntensity.GetRandom();
        particle.Position += Point + Utilities.RandomPointInCircle(OnEdge ? r : 0, r);
        particle.Acceleration += Utilities.RandomPointInCircle(v, v);
    }
}

public struct RandomStartVelocity : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public Vec2Range Velocity = new(default);

    public RandomStartVelocity(Vec2Range velocity) : this()
    {
        Disabled = false;
        Velocity = velocity;
    }

    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.Acceleration += Velocity.GetRandom() ;
    }
}

public struct RandomStartRotVel : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public FloatRange RotationalVelocity = new(-4, 4);

    public RandomStartRotVel(FloatRange rotationalVelocity) : this()
    {
        RotationalVelocity = rotationalVelocity;
    }

    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.RotationalVelocity += RotationalVelocity.GetRandom();
    }
}

public struct RandomStartSize : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public FloatRange Size = new(0.1f, 1f);

    public RandomStartSize(FloatRange size) : this()
    {
        Size = size;
    }

    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.Size += Size.GetRandom();
    }
}

public struct RandomStartRotation : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public FloatRange Rotation = new(0.1f, 1f);

    public RandomStartRotation(FloatRange rotation) : this()
    {
        Rotation = rotation;
    }

    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.Rotation = Rotation.GetRandom();
    }
}

public struct RandomStartColor : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public ColorRange Color = new(Colors.Yellow, Colors.Blue);

    public RandomStartColor(ColorRange color) : this()
    {
        Color = color;
    }

    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.Color *= Color.GetRandom();
    }
}

public struct RandomDampening : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public FloatRange Dampening;
    public FloatRange RotationalDampening;

    public RandomDampening(FloatRange dampening, FloatRange rotationalDampening) : this()
    {
        Dampening = dampening;
        RotationalDampening = rotationalDampening;
    }
    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.Dampening += Dampening.GetRandom();
        particle.RotationalDampening += RotationalDampening.GetRandom();
    }
}

public struct RandomLifespan : IParticleInitialiser
{
    public bool Disabled { get; set; }
    public FloatRange LifeRange;

    public RandomLifespan(FloatRange lifeRange) : this()
    {
        LifeRange = lifeRange;
    }
    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.MaxLife += LifeRange.GetRandom();
    }
}