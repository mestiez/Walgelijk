using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Walgelijk.ParticleSystem;

public interface IParticleModule
{
    public bool Disabled { get; set; }

    /// <summary>
    /// Processes a single particle
    /// </summary>
    public void Process(int index, ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform);
}

public interface IParticleEmitter
{
    public bool Disabled { get; set; }

    /// <summary>
    /// Returns the amount of particles to emit
    /// </summary>
    public int Emit(in GameState gameState, ParticlesComponent component);
}

public interface IParticleInitialiser
{
    public bool Disabled { get; set; }

    /// <summary>
    /// Set properties of new particles
    /// </summary>
    public void Initialise(ref Particle particle, in GameState gameState, ParticlesComponent component, TransformComponent transform);
}

[RequiresComponents(typeof(TransformComponent))]
public class ParticlesComponent : Component
{
    public readonly int MaxParticleCount = 1000;
    public int CurrentParticleCount = 0;

    public readonly Particle[] RawParticleArray;

    public ParticlesComponent(int maxCount = 1000)
    {
        MaxParticleCount = maxCount;
        RawParticleArray = new Particle[MaxParticleCount];

        VertexBuffer = new VertexBuffer(PrimitiveMeshes.CenteredQuad.Vertices, PrimitiveMeshes.CenteredQuad.Indices, new VertexAttributeArray[]{
            new Matrix4x4AttributeArray(new Matrix4x4[MaxParticleCount]), // transform
            new Vector4AttributeArray(new Vector4[MaxParticleCount]), // color
        });

        RenderTask = new InstancedShapeRenderTask(VertexBuffer, material: Material);
    }

    public Material Material = Particle.DefaultMaterial;

    public readonly List<IParticleModule> Modules = new();
    public readonly List<IParticleEmitter> Emitters = new();
    public readonly List<IParticleInitialiser> Initalisers = new();

    public T? GetModuleByType<T>() where T : IParticleModule
    {
        foreach (var item in Modules)
            if (item is T tt)
                return tt;
        return default;
    }

    public T? GetEmitterByType<T>() where T : IParticleModule
    {
        foreach (var item in Emitters)
            if (item is T tt)
                return tt;
        return default;
    }

    public T? GetInitialiserByType<T>() where T : IParticleInitialiser
    {
        foreach (var item in Initalisers)
            if (item is T tt)
                return tt;
        return default;
    }

    //public Vec2Range Gravity = new(new Vector2(0, -9.81f));
    //public FloatRange SpawnRadius = new(0.2f, 1f);
    //public FloatRange LifeRange = new(1, 3);

    //public FloatRange StartSize = new(1);
    //public FloatRange StartRotation = new(0);
    //public Vec2Range StartVelocity = new(Vector2.One * -4, Vector2.One * 4);
    //public FloatRange StartRotationalVelocity = new(-4, 4);
    //public ColorRange StartColor = new(Colors.White);
    //public FloatRange Dampening = new(0.1f);
    //public FloatRange RotationalDampening = new(0.1f);

    //public FloatCurve SizeOverLife = new(new Curve<float>.Key(1, 1));
    //public ColorCurve ColorOverLife = new(new Curve<Color>.Key(Colors.White, 1));

    //public float? FloorLevel = null;
    //public float FloorBounceFactor = 0.4f;
    //public float FloorCollisionDampeningFactor = 0.4f;
    //public Hook<Particle> OnHitFloor = new();

    //public bool CircularStartVelocity = false;
    public float SimulationSpeed = 1;
    //public float EmissionRate = 150;
    public bool WorldSpace;
    public bool ScreenSpace; //TODO enum shit

    public readonly InstancedShapeRenderTask RenderTask;
    public readonly VertexBuffer VertexBuffer;

    public RenderOrder Depth = default;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Particle GenerateParticleObject(GameState state, TransformComponent transform)
    {
        var particle = new Particle()
        {
            Size = 1,
            Color = Colors.White,
            Active = true,
        };

        foreach (var initialiser in Initalisers)
        {
            if (initialiser.Disabled)
                continue;
            initialiser.Initialise(ref particle, state, this, transform);
        }

        // TODO worldspace shit
        //if (!WorldSpace)
        //    particle.Position += transform.Position;

        return particle;
    }
}
