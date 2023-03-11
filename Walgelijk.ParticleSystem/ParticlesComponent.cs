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
            new Vector4AttributeArray(new Vector4[MaxParticleCount]), // uv (x offset, y offset, x scale ,y scale)
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

    public float SimulationSpeed = 1;
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
            UvOffset = new Vector4(0, 0, 1, 1)
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
