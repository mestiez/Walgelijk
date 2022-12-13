using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Walgelijk.ParticleSystem.Modules;

public struct GravityModule : IParticleModule
{
    public bool Disabled { get; set; }
    public Vec2Range Gravity = new(new Vector2(0, -9.81f));

    public GravityModule(Vec2Range gravity)
    {
        Gravity = gravity;
        Disabled = false;
    }

    public void Process(int index, ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.Acceleration += Utilities.Lerp(
           Gravity.Min,
           Gravity.Max,
            Utilities.Hash(index * 24.2453f)); // waarom? omdat particles een "random" gravity moeten hebben die wel consequent blijft tijdens het leven van een particle
    }
}

public struct ColorOverTimeModule : IParticleModule
{
    public bool Disabled { get; set; }
    public ColorCurve Colors;

    public ColorOverTimeModule(ColorCurve colors)
    {
        Colors = colors;
        Disabled = false;
    }

    public void Process(int index, ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.RenderedColor *= Colors.Evaluate(particle.NormalisedLife);
    }
}

public struct SizeOverTimeModule : IParticleModule
{
    public bool Disabled { get; set; }
    public FloatCurve Size;

    public SizeOverTimeModule(FloatCurve size)
    {
        Size = size;
        Disabled = false;
    }

    public void Process(int index, ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        particle.RenderedSize *= Size.Evaluate(particle.NormalisedLife);
    }
}

public struct NoiseModule : IParticleModule
{
    public bool Disabled { get; set; } = false;

    public float Frequency = 0.5f;
    public float Evolution = 0.2f;
    public FloatRange VelocityIntensity = new(0, 1);
    public FloatRange RotVelIntensity = new(0, 0);

    public FloatCurve InfluenceOverTime = new(new Curve<float>.Key(1, 0));
    public float Influence = 1;

    public NoiseModule()
    {
    }

    public void Process(int index, ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform)
    {
        var influence = Influence * InfluenceOverTime.Evaluate(particle.NormalisedLife);

        var x = Noise.GetSimplex(
            particle.Position.X * Frequency,
            particle.Position.Y * Frequency,
            Evolution * (gameState.Time.SecondsSinceLoad + index * 382.5923f));

        var y = Noise.GetSimplex(
            particle.Position.X * Frequency,
            particle.Position.Y * Frequency,
            4892.2938f - Evolution * -(gameState.Time.SecondsSinceLoad + index * 382.5923f));

        particle.Acceleration.X += x * influence;
        particle.Acceleration.Y += y * influence;
    }
}
