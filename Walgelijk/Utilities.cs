using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Walgelijk
{
    public struct Utilities
    {
        private static readonly Random rand = new Random();

        /// <summary>
        /// Radians to degrees constant ratio
        /// </summary>
        public const float RadToDeg = 180f / MathF.PI;
        /// <summary>
        /// Degrees to radians constant ratio
        /// </summary>
        public const float DegToRad = MathF.PI / 180f;

        /// <summary>
        /// Linearly interpolate between two angles in degrees
        /// </summary>
        public static float LerpAngle(float a, float b, float t)
        {
            return a + DeltaAngle(a, b) * t;
        }

        /// <summary>
        /// Linearly interpolate between two floats
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return a * (1 - t) + b * t;
        }

        /// <summary>
        /// Linearly interpolate between two colors or 4 dimensional vectors
        /// </summary>
        public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
        {
            return a * (1 - t) + b * t;
        }

        /// <summary>
        /// Linearly interpolate between two vectors
        /// </summary>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t)
                );
        }

        /// <summary>
        /// Linearly interpolate between two vectors
        /// </summary>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t),
                Lerp(a.Z, b.Z, t)
                );
        }

        /// <summary>
        /// Returns a random float in a range
        /// </summary>
        public static float RandomFloat(float min = 0, float max = 1)
        {
            return Lerp(min, max, (float)rand.NextDouble());
        }

        /// <summary>
        /// Returns a random point in a circle
        /// </summary>
        public static Vector2 RandomPointInCircle(float minRadius = 0, float maxRadius = 1)
        {
            Vector2 pos = Vector2.Normalize(new Vector2(
                RandomFloat(-1, 1),
                RandomFloat(-1, 1)
                ));

            return pos * RandomFloat(minRadius, maxRadius);
        }

        /// <summary>
        /// Returns a random int in a range
        /// </summary>
        public static int RandomInt(int min = 0, int max = 100)
        {
            return rand.Next(min, max);
        }

        /// <summary>
        /// Returns a random byte
        /// </summary>
        public static byte RandomByte()
        {
            return (byte)rand.Next(0, 256);
        }

        /// <summary>
        /// Returns a colour where the RGB components are random
        /// </summary>
        public static Color RandomColour(float alpha = 1)
        {
            return new Color(RandomFloat(), RandomFloat(), RandomFloat(), alpha);
        }

        /// <summary>
        /// Clamp a value within a range
        /// </summary>
        /// <returns></returns>
        public static float Clamp(float x, float min = 0, float max = 1)
        {
            return MathF.Max(min, MathF.Min(x, max));
        }

        /// <summary>
        /// Clamp a value within a range
        /// </summary>
        /// <returns></returns>
        public static int Clamp(int x, int min = 0, int max = 1)
        {
            return Math.Max(min, Math.Min(x, max));
        }

        /// <summary>
        /// Modulus
        /// </summary>
        public static float Mod(float a, float b)
        {
            return a - MathF.Floor(a / b) * b;
        }

        /// <summary>
        /// Smallest difference between two angles in degrees
        /// </summary>
        public static float DeltaAngle(float source, float target)
        {
            return Mod(target - source + 180, 360) - 180;
        }

        /// <summary>
        /// Return a random entry in a <see cref="ICollection{T}"/>
        /// </summary>
        public static T PickRandom<T>(IList<T> collection)
        {
            return collection[RandomInt(0, collection.Count)];
        }

        /// <summary>
        /// Returns a normalised <see cref="Vector2"/> corresponding to the given angle in degrees. 
        /// 0° gives (1, 0). 90° gives (0, 1)
        /// </summary>
        public static Vector2 AngleToVector(float degrees)
        {
            float rad = degrees * DegToRad;
            return new Vector2(MathF.Cos(rad), MathF.Sin(rad));
        }

        /// <summary>
        /// Linearly map a value in a range onto another range
        /// </summary>
        /// <param name="a1">Source lower bound</param>
        /// <param name="a2">Source upper bound</param>
        /// <param name="b1">Destination lower bound</param>
        /// <param name="b2">Destination upper bound</param>
        /// <param name="s">The value to remap</param>
        /// <returns>Remapped value <paramref name="s"/></returns>
        public static float MapRange(float a1, float a2, float b1, float b2, float s)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        /// <summary>
        /// Apply a constant acceleration to the given 2D position and 2D velocity, considering a time step.
        /// </summary>
        /// <param name="acceleration">The acceleration</param>
        /// <param name="currentPos">The initial position</param>
        /// <param name="currentVelocity">The initial velocity</param>
        /// <param name="deltaTime">The time step</param>
        /// <param name="dampening">Optional dampening parameter (0 - 1)</param>
        /// <returns>A struct with the new position and new velocity</returns>
        public static (Vector2 newPosition, Vector2 newVelocity) ApplyAcceleration(Vector2 acceleration, Vector2 currentPos, Vector2 currentVelocity, float deltaTime, float dampening = 1)
        {
            currentVelocity *= MathF.Pow(1 - dampening, deltaTime);

            var newVel = deltaTime * acceleration + currentVelocity;
            var newPos = 0.5f * acceleration * deltaTime * deltaTime + currentVelocity * deltaTime + currentPos;

            return (newPos, newVel);
        }

        /// <summary>
        /// Apply a constant acceleration to the given 1D position and 1D velocity, considering a time step.
        /// </summary>
        /// <param name="acceleration">The acceleration</param>
        /// <param name="currentPos">The initial position</param>
        /// <param name="currentVelocity">The initial velocity</param>
        /// <param name="deltaTime">The time step</param>
        /// <param name="dampening">Optional dampening parameter (0 - 1)</param>
        /// <returns>A struct with the new position and new velocity</returns>
        public static (float newPosition, float newVelocity) ApplyAcceleration(float acceleration, float currentPos, float currentVelocity, float deltaTime, float dampening = 1)
        {
            currentVelocity *= MathF.Pow(1 - dampening, deltaTime);

            var newVel = deltaTime * acceleration + currentVelocity;
            var newPos = 0.5f * acceleration * deltaTime * deltaTime + currentVelocity * deltaTime + currentPos;

            return (newPos, newVel);
        }

        /// <summary>
        /// Returns the lerp factor adjusted for the given time step
        /// </summary>
        public static float GetLerpFactorDeltaTime(float lerpFactor, float deltaTime)
        {
            return 1 - MathF.Pow(1 - lerpFactor, deltaTime);
        }
    }
}
