using System;
using System.Collections.Generic;
using System.Numerics;

namespace Walgelijk.ParticleSystem
{
    public class ParticleSystem : Walgelijk.System
    {
        private IEnumerable<EntityWith<ParticlesComponent>> retrievedParticleSystems = null;

        public override void Update()
        {
            Profiler.StartTask("Particle system update");

            retrievedParticleSystems = Scene.GetAllComponentsOfType<ParticlesComponent>();
            foreach (var item in retrievedParticleSystems)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(item.Entity);
                var particles = item.Component;

                HandleEmission(particles, transform);
                UpdateParticleSystem(particles, transform);
            }

            Profiler.EndTask();
        }

        public override void Render()
        {
            if (retrievedParticleSystems == null) return;

            Profiler.StartTask("Particle system render");
            foreach (var item in retrievedParticleSystems)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(item.Entity);
                var particles = item.Component;

                RenderParticleSystem(particles, transform);
            }
            Profiler.EndTask();
        }

        private void HandleEmission(ParticlesComponent particles, TransformComponent transform)
        {
            if (particles.EmissionRate < 0.001f) return;
            float inverseRate = 1 / particles.EmissionRate;
            if (particles.Clock > inverseRate)
            {
                int particlesToSpawn = (int)MathF.Ceiling(particles.Clock / inverseRate);
                particles.Clock = 0;
                for (int i = 0; i < particlesToSpawn; i++)
                    CreateParticle(particles, transform);
            }
        }

        public void CreateParticle(ParticlesComponent particles, TransformComponent transform)
        {
            if (particles.CurrentParticleCount >= particles.MaxParticleCount) return;

            CreateParticle(particles, transform, particles.GenerateParticleObject());
        }

        public void CreateParticle(ParticlesComponent particles, TransformComponent transform, Particle particleToAdd)
        {
            if (particles.CurrentParticleCount >= particles.MaxParticleCount) return;

            var particle = particleToAdd;

            particle.Color = particle.InitialColor;
            particle.Size = particle.InitialSize;

            if (particles.CircularStartVelocity)
            {
                var v = particle.Velocity.Length();
                particle.Velocity = particle.Velocity / v * Utilities.RandomFloat(0, v);
            }

            if (particles.WorldSpace)
                particle.Position += transform.Position;

            particles.RawParticleArray[particles.CurrentParticleCount] = particle;

            particles.CurrentParticleCount++;
        }

        private void RemoveParticle(ParticlesComponent particles, int index)
        {
            if (particles.CurrentParticleCount <= 0) return;

            int maxCount = Math.Max(particles.CurrentParticleCount, particles.MaxParticleCount - 1);

            for (int i = index; i < maxCount; i++)
            {
                //TODO dit moet sneller. vervang particles die al dood zijn ipv alles opschuiven als een gek
                particles.RawParticleArray[i] = particles.RawParticleArray[i + 1];
            }

            particles.CurrentParticleCount--;
        }

        private void UpdateParticleSystem(ParticlesComponent particles, TransformComponent transform)
        {
            var dt = Time.UpdateDeltaTime * particles.SimulationSpeed;

            particles.Clock += dt;

            for (int i = 0; i < particles.CurrentParticleCount; i++)
            {
                var particle = particles.RawParticleArray[i];
                particle.Life += dt;

                if (particle.Life >= particle.MaxLife)
                    RemoveParticle(particles, i);
                else
                {
                    var positionData = Utilities.ApplyAcceleration(particle.Gravity, particle.Position, particle.Velocity, dt, particle.Dampening);
                    particle.Position = positionData.newPosition;
                    particle.Velocity = positionData.newVelocity;

                    var rotationData = Utilities.ApplyAcceleration(0, particle.Angle, particle.RotationalVelocity, dt, particle.RotationalDampening);
                    particle.RotationalVelocity = rotationData.newVelocity;
                    particle.Angle = rotationData.newPosition;

                    float lifePercentage = particle.Life / particle.MaxLife;

                    particle.Size = particle.InitialSize * particles.SizeOverLife.Evaluate(lifePercentage);
                    particle.Color = particle.InitialColor * particles.ColorOverLife.Evaluate(lifePercentage);

                    particles.RawParticleArray[i] = particle;
                }
            }
        }

        private void RenderParticleSystem(ParticlesComponent particles, TransformComponent transform)
        {
            var posArray = particles.VertexBuffer.GetAttribute(0);
            var colArray = particles.VertexBuffer.GetAttribute(1);

            for (int i = 0; i < particles.CurrentParticleCount; i++)
            {
                var particle = particles.RawParticleArray[i];

                Matrix4x4 model = Matrix4x4.CreateRotationZ(particle.Angle) * Matrix4x4.CreateScale(particle.Size) * Matrix4x4.CreateTranslation(particle.Position.X, particle.Position.Y, 0);

                posArray.SetAt(i, model);
                colArray.SetAt(i, (Vector4)particle.Color);
            }

            var task = particles.RenderTask;
            task.InstanceCount = particles.CurrentParticleCount;
            task.ModelMatrix = particles.WorldSpace ? Matrix4x4.Identity : transform.LocalToWorldMatrix;

            particles.VertexBuffer.ExtraDataHasChanged = true;

            RenderQueue.Add(particles.RenderTask, particles.Depth);
        }
    }
}
