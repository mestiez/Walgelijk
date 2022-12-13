using System;
using System.Numerics;

namespace Walgelijk.ParticleSystem;

public class ParticleSystem : Walgelijk.System
{
    public override void Update()
    {
        var s = Scene.GetAllComponentsOfType<ParticlesComponent>();
        foreach (var particles in s)
        {
            var transform = Scene.GetComponentFrom<TransformComponent>(particles.Entity);

            EmitParticles(particles, transform);
            UpdateParticleSystem(particles, transform);
        }
    }

    public override void Render()
    {
        var s = Scene.GetAllComponentsOfType<ParticlesComponent>();
        foreach (var particles in s)
        {
            var transform = Scene.GetComponentFrom<TransformComponent>(particles.Entity);

            RenderParticleSystem(particles, transform);
        }
    }

    private void EmitParticles(ParticlesComponent particles, TransformComponent transform)
    {
        if (particles.Emitters.Count == 0 || particles.CurrentParticleCount >= particles.MaxParticleCount)
            return;

        int amount = 0;

        foreach (var emitter in particles.Emitters)
            if (!emitter.Disabled)
                amount += emitter.Emit(Game.State, particles);

        for (int i = 0; i < amount; i++)
            CreateParticle(particles, particles.GenerateParticleObject(Scene.Game.State, transform));
    }

    private static int GetFreeParticleIndex(ParticlesComponent particles)
    {
        for (int i = 0; i < particles.MaxParticleCount; i++)
            if (!particles.RawParticleArray[i].Active)
                return i;

        return -1;
    }

    private static void RemoveParticle(ParticlesComponent particles, int index)
    {
        if (particles.CurrentParticleCount <= 0) return;
        particles.RawParticleArray[index].Active = false;
        particles.CurrentParticleCount--;
    }

    private void UpdateParticleSystem(ParticlesComponent particles, TransformComponent transform)
    {
        var dt = Time.DeltaTime * particles.SimulationSpeed;

        for (int i = 0; i < particles.MaxParticleCount; i++)
        {
            var particle = particles.RawParticleArray[i];

            if (!particle.Active)
                continue;

            particle.Life += dt;

            if (particle.Life >= particle.MaxLife)
                RemoveParticle(particles, i);
            else
            {
                particle.NormalisedLife = particle.Life / particle.MaxLife;
                particle.RenderedSize = particle.Size;
                particle.RenderedColor = particle.Color;

                foreach (var item in particles.Modules)
                    item.Process(i, ref particle, Game.State, particles, transform);

                var positionData = Utilities.ApplyAcceleration(particle.Acceleration, particle.Position, particle.Velocity, dt, particle.Dampening);
                particle.Position = positionData.newPosition;
                particle.Velocity = positionData.newVelocity;

                var rotationData = Utilities.ApplyAcceleration(particle.RotationalAcceleration, particle.Rotation, particle.RotationalVelocity, dt, particle.RotationalDampening);
                particle.RotationalVelocity = rotationData.newVelocity;
                particle.Rotation = rotationData.newPosition;

                particle.Acceleration = Vector2.Zero;
                particle.RotationalAcceleration = 0;

                particles.RawParticleArray[i] = particle;
            }
        };
    }

    private void RenderParticleSystem(ParticlesComponent particles, TransformComponent transform)
    {
        var posArray = particles.VertexBuffer.GetAttribute<Matrix4x4AttributeArray>(0) ?? throw new Exception("Particle system vertex buffer attribute 0 is not of the correct type");
        var colArray = particles.VertexBuffer.GetAttribute<Vector4AttributeArray>(1) ?? throw new Exception("Particle system vertex buffer attribute 1 is not of the correct type");

        int activeIndex = 0;

        for (int i = 0; i < particles.MaxParticleCount; i++)
        {
            var particle = particles.RawParticleArray[i];

            if (!particle.Active)
                continue;

            var model = Matrix3x2.CreateRotation(particle.Rotation * Utilities.DegToRad)
                * Matrix3x2.CreateScale(particle.RenderedSize)
                * Matrix3x2.CreateTranslation(particle.Position.X, particle.Position.Y);

            posArray.Data[activeIndex] = new Matrix4x4(model);
            colArray.Data[activeIndex] = particle.RenderedColor;

            activeIndex++;
        }
        if (activeIndex != particles.CurrentParticleCount)
            Logger.Warn($"Set particles and current expected particle count are not equal: {particles.CurrentParticleCount} expected, actual {activeIndex}");

        var task = particles.RenderTask;
        task.InstanceCount = particles.CurrentParticleCount;
        task.ModelMatrix = particles.WorldSpace ? Matrix3x2.Identity : transform.LocalToWorldMatrix;

        particles.VertexBuffer.ExtraDataHasChanged = true;

        RenderQueue.Add(particles.RenderTask, particles.Depth);
    }

    public void CreateParticle(ParticlesComponent particles, in Particle particle)
    {
        int targetIndex = GetFreeParticleIndex(particles);
        if (targetIndex == -1)
            return;

        particles.RawParticleArray[targetIndex] = particle;
        particles.CurrentParticleCount++;
    }
}
