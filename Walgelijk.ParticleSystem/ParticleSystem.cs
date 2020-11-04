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

            if (particles.Clock > 1 / particles.EmissionRate)
            {
                particles.Clock = 0;
                CreateParticle(particles, transform);
            }
        }

        public void CreateParticle(ParticlesComponent particles, TransformComponent transform)
        {
            if (particles.CurrentParticleCount >= particles.MaxParticleCount) return;

            Particle particle = new Particle
            {
                Angle = particles.StartRotation.GetRandom(),
                Position = Utilities.RandomPointInCircle(particles.SpawnRadius.Min, particles.SpawnRadius.Max),
                Velocity = particles.StartVelocity.GetRandom(),
                MaxLife = particles.LifeRange.GetRandom(),
                InitialColor = particles.StartColor.GetRandom(),
                Gravity = particles.Gravity.GetRandom(),
                InitialSize = particles.StartSize.GetRandom(),
                RotationalVelocity = particles.StartRotationalVelocity.GetRandom(),
                Dampening = particles.Dampening.GetRandom(),
                RotationalDampening = particles.RotationalDampening.GetRandom(),

                Life = 0,
            };

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
            for (int i = 0; i < particles.CurrentParticleCount; i++)
            {
                var particle = particles.RawParticleArray[i];

                Matrix4x4 model = Matrix4x4.CreateRotationZ(particle.Angle) * Matrix4x4.CreateScale(particle.Size) * Matrix4x4.CreateTranslation(particle.Position.X, particle.Position.Y, 0);

                particles.InstanceData.RawModelArray[i] = model;
                particles.InstanceData.RawColorArray[i] = particle.Color;

                //TODO instancing data naar vertex buffer en dan dat de implementatie van de graphics renderer het als vertex attributes doet en al
            }

            var task = particles.RenderTask;
            task.InstanceCount = particles.CurrentParticleCount;
            task.ModelMatrix = particles.WorldSpace ? Matrix4x4.Identity : transform.LocalToWorldMatrix;

            RenderQueue.Add(particles.RenderTask, particles.Depth);
        }
    }
}
