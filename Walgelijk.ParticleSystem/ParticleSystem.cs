using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Walgelijk.ParticleSystem
{
    public class ParticleSystem : Walgelijk.System
    {
        private IEnumerable<EntityWith<ParticlesComponent>> retrievedParticleSystems = null;

        public override void Update()
        {
            retrievedParticleSystems = Scene.GetAllComponentsOfType<ParticlesComponent>();
            foreach (var item in retrievedParticleSystems)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(item.Entity);
                var particles = item.Component;

                HandleEmission(particles, transform);
                UpdateParticleSystem(particles, transform);
            }
        }

        public override void Render()
        {
            if (retrievedParticleSystems == null) return;

            foreach (var item in retrievedParticleSystems)
            {
                var transform = Scene.GetComponentFrom<TransformComponent>(item.Entity);
                var particles = item.Component;

                RenderParticleSystem(particles, transform);
            }
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
            if (particles.CurrentParticleCount >= particles.MaxParticleCount) 
                return;

            int targetIndex = GetFreeParticleIndex(particles);
            if (targetIndex == -1)
                return;

            var particle = particleToAdd;

            particle.Active = true;
            particle.Color = particle.InitialColor;
            particle.Size = particle.InitialSize;

            if (particles.CircularStartVelocity)
            {
                var v = particle.Velocity.Length();
                particle.Velocity = particle.Velocity / v * Utilities.RandomFloat(0, v);
            }

            if (particles.WorldSpace)
                particle.Position += transform.Position;

            particles.RawParticleArray[targetIndex] = particle;

            particles.CurrentParticleCount++;
        }

        private int GetFreeParticleIndex(ParticlesComponent particles)
        {
            for (int i = 0; i < particles.MaxParticleCount; i++)
                if (!particles.RawParticleArray[i].Active)
                    return i;

            return -1;
        }

        private void RemoveParticle(ParticlesComponent particles, int index)
        {
            if (particles.CurrentParticleCount <= 0) return;

            particles.RawParticleArray[index].Active = false;

            //int maxCount = Math.Min(particles.CurrentParticleCount, particles.MaxParticleCount - 1);
            //for (int i = index; i < maxCount; i++)
            //{
            //    particles.RawParticleArray[i] = particles.RawParticleArray[i + 1];
            //}

            particles.CurrentParticleCount--;
        }

        private void UpdateParticleSystem(ParticlesComponent particles, TransformComponent transform)
        {
            var dt = Time.UpdateDeltaTime * particles.SimulationSpeed;

            particles.Clock += dt;

            //Parallel.For(0, particles.MaxParticleCount, (i) =>
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
            };//);
        }

        private void RenderParticleSystem(ParticlesComponent particles, TransformComponent transform)
        {
            var posArray = particles.VertexBuffer.GetAttribute(0);
            var colArray = particles.VertexBuffer.GetAttribute(1);

            int activeIndex = 0;

            for (int i = 0; i < particles.MaxParticleCount; i++)
            {
                var particle = particles.RawParticleArray[i];

                if (!particle.Active)
                    continue;

                Matrix4x4 model = Matrix4x4.CreateRotationZ(particle.Angle) * Matrix4x4.CreateScale(particle.Size) * Matrix4x4.CreateTranslation(particle.Position.X, particle.Position.Y, 0);

                posArray.SetAt(activeIndex, model);
                colArray.SetAt(activeIndex, (Vector4)particle.Color);

                activeIndex++;
            }

            if (activeIndex != particles.CurrentParticleCount)
                throw new Exception("hoe kan dit nou");

            var task = particles.RenderTask;
            task.InstanceCount = particles.CurrentParticleCount;
            task.ModelMatrix = particles.WorldSpace ? Matrix4x4.Identity : transform.LocalToWorldMatrix;

            particles.VertexBuffer.ExtraDataHasChanged = true;

            RenderQueue.Add(particles.RenderTask, particles.Depth);
        }
    }
}
