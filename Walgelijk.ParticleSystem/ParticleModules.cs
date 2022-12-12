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
            Utilities.Hash(index * 24.2453f));
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
        particle.Color = Colors.Evaluate(particle.Life / particle.MaxLife);
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
        particle.Size = Size.Evaluate(particle.Life / particle.MaxLife);
    }
}
