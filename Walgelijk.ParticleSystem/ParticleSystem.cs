using System;
using System.Numerics;

namespace Walgelijk.ParticleSystem
{
    public class ParticleSystem : Walgelijk.System
    {
        public override void Update()
        {
            var s = Scene.GetAllComponentsOfType<ParticlesComponent>();
            foreach (var item in s)
            {
                var transform = Scene.GetComponentFast<TransformComponent>(item.Entity);
                var particles = item.Component;

                HandleEmission(particles, transform);
                UpdateParticleSystem(particles, transform);
            }
        }

        public override void Render()
        {
            var s = Scene.GetAllComponentsOfType<ParticlesComponent>();
            foreach (var item in s)
            {
                var transform = Scene.GetComponentFast<TransformComponent>(item.Entity);
                var particles = item.Component;

                RenderParticleSystem(particles, transform);
            }
        }

        private void HandleEmission(ParticlesComponent particles, TransformComponent transform)
        {
            if (particles.EmissionRate <= float.Epsilon) return;

            particles.EmissionDistributor.Rate = particles.EmissionRate;
            var cycles = particles.EmissionDistributor.CalculateCycleCount(Time.DeltaTime);

            for (int i = 0; i < cycles; i++)
                CreateParticle(particles, transform);
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
                    var positionData = Utilities.ApplyAcceleration(particle.Gravity, particle.Position, particle.Velocity, dt, particle.Dampening);

                    if (particles.FloorLevel.HasValue && particles.FloorLevel.Value > positionData.newPosition.Y)
                    {
                        positionData.newVelocity.Y *= -particles.FloorBounceFactor;
                        positionData.newVelocity.X *= 1 - particles.FloorCollisionDampeningFactor;
                        positionData.newPosition.Y = particles.FloorLevel.Value;
                        if (MathF.Abs(particle.Position.Y - particles.FloorLevel.Value) > 0.01f)
                            particles.OnHitFloor.Dispatch(particle);
                    }

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
            };
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

                var model = Matrix3x2.CreateRotation(particle.Angle * Utilities.DegToRad) 
                    * Matrix3x2.CreateScale(particle.Size)
                    * Matrix3x2.CreateTranslation(particle.Position.X, particle.Position.Y);

                posArray.SetAt(activeIndex, new Matrix4x4(model));
                colArray.SetAt(activeIndex, (Vector4)particle.Color);

                activeIndex++;
            }
            if (activeIndex != particles.CurrentParticleCount)
                Logger.Warn($"Set particles and current expected particle count are not equal: {particles.CurrentParticleCount} expected, actual {activeIndex}");

            var task = particles.RenderTask;
            task.InstanceCount = particles.CurrentParticleCount;
            task.ModelMatrix = particles.WorldSpace ? Matrix4x4.Identity : new Matrix4x4(transform.LocalToWorldMatrix);

            particles.VertexBuffer.ExtraDataHasChanged = true;

            RenderQueue.Add(particles.RenderTask, particles.Depth);
        }
    }
}
