﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Walgelijk
{
    /// <summary>
    /// Utility struct full of useful function
    /// </summary>
    public struct Utilities
    {
        private static readonly Random rand = new();

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpAngle(float a, float b, float t)
        {
            return a + DeltaAngle(a, b) * t;
        }

        /// <summary>
        /// Linearly interpolate between two floats
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t)
        {
            return a * (1 - t) + b * t;
        }

        /// <summary>
        /// Linearly interpolate between two colors or 4 dimensional vectors
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
        {
            return new(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t),
                Lerp(a.Z, b.Z, t),
                Lerp(a.W, b.W, t)
                );
        }

        /// <summary>
        /// Linearly interpolate between two vectors
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t)
                );
        }

        /// <summary>
        /// Linearly interpolate between two vectors
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RandomFloat(float min = 0, float max = 1)
        {
            return Lerp(min, max, (float)rand.NextDouble());
        }

        /// <summary>
        /// Returns a random point in a circle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RandomPointInCircle(float minRadius = 0, float maxRadius = 1)
        {
            Vector2 pos = Vector2.Normalize(new Vector2(
                RandomFloat(-1, 1),
                RandomFloat(-1, 1)
                ));

            return pos * RandomFloat(minRadius, maxRadius);
        }

        /// <summary>
        /// Returns a random int in a range (inclusive min, exclusive max)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RandomInt(int min = 0, int max = 100)
        {
            return rand.Next(min, max);
        }

        /// <summary>
        /// Returns a random byte
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RandomByte()
        {
            return (byte)rand.Next(0, 256);
        }

        /// <summary>
        /// Returns a colour where the RGB components are random
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color RandomColour(float alpha = 1)
        {
            return new Color(RandomFloat(), RandomFloat(), RandomFloat(), alpha);
        }

        /// <summary>
        /// Clamp a value within a range
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float x, float min = 0, float max = 1)
        {
            return MathF.Max(min, MathF.Min(x, max));
        }

        /// <summary>
        /// Clamp a value within a range
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int x, int min = 0, int max = 1)
        {
            return Math.Max(min, Math.Min(x, max));
        }

        /// <summary>
        /// Modulus
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Mod(float a, float b)
        {
            return a - MathF.Floor(a / b) * b;
        }

        /// <summary>
        /// Smallest difference between two angles in degrees
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Return a random entry in a <see cref="IEnumerable{T}"/>
        /// </summary>
        public static T PickRandom<T>(IEnumerable<T> collection)
        {
            return collection.ElementAt(RandomInt(0, collection.Count()));
        }

        /// <summary>
        /// Return a random value from the given parameters
        /// </summary>
        public static T PickRandom<T>(T first, T second)
        {
            return RandomFloat() > 0.5f ? first : second;
        }

        /// <summary>
        /// Return a random value from the given parameters
        /// </summary>
        public static T PickRandom<T>(T first, T second, T third)
        {
            return RandomInt(0, 3) switch
            {
                0 => first,
                1 => second,
                _ => third,
            };
        }

        /// <summary>
        /// Return a random value from the given parameters
        /// </summary>
        public static T PickRandom<T>(T first, T second, T third, T fourth)
        {
            return RandomInt(0, 4) switch
            {
                0 => first,
                1 => second,
                2 => third,
                _ => fourth,
            };
        }

        /// <summary>
        /// Return a random value from the given parameters
        /// </summary>
        public static T PickRandom<T>(T first, T second, T third, T fourth, T fifth)
        {
            return RandomInt(0, 5) switch
            {
                0 => first,
                1 => second,
                2 => third,
                3 => fourth,
                _ => fifth,
            };
        }

        /// <summary>
        /// Returns a normalised <see cref="Vector2"/> corresponding to the given angle in degrees. 
        /// 0° gives (1, 0). 90° gives (0, 1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 AngleToVector(float degrees)
        {
            float rad = degrees * DegToRad;
            return new Vector2(MathF.Cos(rad), MathF.Sin(rad));
        }

        /// <summary>
        /// Returns an angle in degrees corresponding to the given normalised <see cref="Vector2"/>. 
        /// (1, 0) gives 0°. (0, 1) gives 90° 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float VectorToAngle(Vector2 vector)
        {
            return MathF.Atan2(vector.Y, vector.X) * RadToDeg;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpDt(float lerpFactor, float deltaTime)
        {
            if (deltaTime <= float.Epsilon)
                return 0;
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
                return 1;
            var v = Clamp(1 - MathF.Pow(1 - lerpFactor, deltaTime), 0, 1);
            return float.IsNaN(v) ? 0 : v;
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothApproach(float pastPosition, float pastTargetPosition, float targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            var v = (targetPosition - pastTargetPosition) / t;
            var f = pastPosition - pastTargetPosition + v;
            return NanFallback(targetPosition - v + f * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SmoothApproach(Vector2 pastPosition, Vector2 pastTargetPosition, Vector2 targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            var v = (targetPosition - pastTargetPosition) / t;
            var f = pastPosition - pastTargetPosition + v;
            return NanFallback(targetPosition - v + f * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothApproach(Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            var v = (targetPosition - pastTargetPosition) / t;
            var f = pastPosition - pastTargetPosition + v;
            return NanFallback(targetPosition - v + f * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SmoothApproach(Vector4 pastPosition, Vector4 pastTargetPosition, Vector4 targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            var v = (targetPosition - pastTargetPosition) / t;
            var f = pastPosition - pastTargetPosition + v;
            return NanFallback(targetPosition - v + f * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target angle degrees given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothAngleApproach(float pastPosition, float pastTargetPosition, float targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            var v = (DeltaAngle(pastTargetPosition, targetPosition)) / t;
            var f = DeltaAngle(pastTargetPosition, pastPosition) + v;
            return NanFallback(targetPosition - v + f * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target angle degrees given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothAngleApproach(float pastPosition, float targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            return NanFallback(targetPosition + DeltaAngle(pastPosition, targetPosition) * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothApproach(float pastPosition, float targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            return NanFallback(targetPosition + (pastPosition - targetPosition) * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SmoothApproach(Vector2 pastPosition, Vector2 targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            return NanFallback(targetPosition + (pastPosition - targetPosition) * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SmoothApproach(Vector3 pastPosition, Vector3 targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            return NanFallback(targetPosition + (pastPosition - targetPosition) * MathF.Exp(-t));
        }

        /// <summary>
        /// Smoothly approaches a value to a target value given a speed and dt
        /// <br></br>
        /// By luispedrofonseca
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SmoothApproach(Vector4 pastPosition, Vector4 targetPosition, float speed, float deltaTime)
        {
            var t = deltaTime * speed;
            return NanFallback(targetPosition + (pastPosition - targetPosition) * MathF.Exp(-t));
        }

        /// <summary>
        /// Returns the given fallback value (0 by default) if the given input value is NaN
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NanFallback(float v, float fallback = 0) => float.IsNaN(v) ? fallback : v;

        /// <summary>
        /// Returns the given fallback value (0 by default) if the given input value is NaN
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NanFallback(Vector2 v, float fallback = 0) => new Vector2(NanFallback(v.X), NanFallback(v.Y));

        /// <summary>
        /// Returns the given fallback value (0 by default) if the given input value is NaN
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NanFallback(Vector3 v, float fallback = 0) => new Vector3(NanFallback(v.X), NanFallback(v.Y), NanFallback(v.Z));

        /// <summary>
        /// Returns the given fallback value (0 by default) if the given input value is NaN
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 NanFallback(Vector4 v, float fallback = 0) => new Vector4(NanFallback(v.X), NanFallback(v.Y), NanFallback(v.Z), NanFallback(v.W));

        /// <summary>
        /// Snap <paramref name="x"/> to a grid of size <paramref name="snapSize"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Snap(float x, float snapSize) => MathF.Round(x / snapSize) * snapSize;

        /// <summary>
        /// Snap <paramref name="x"/> to a grid of size <paramref name="snapSize"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Snap(int x, int snapSize) => (int)(MathF.Round(x / snapSize) * snapSize);

        /// <summary>
        /// Snap <paramref name="x"/> to a grid of size <paramref name="snapSize"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Snap(Vector2 x, float snapSize) => new Vector2(Snap(x.X, snapSize), Snap(x.Y, snapSize));

        /// <summary>
        /// Snap <paramref name="x"/> to a grid of size <paramref name="snapSize"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Snap(Vector3 x, float snapSize) => new Vector3(Snap(x.X, snapSize), Snap(x.Y, snapSize), Snap(x.Z, snapSize));

        /// <summary>
        /// Are the two given character spans the same, regardless of casing? 
        /// </summary>
        public static bool TextEqualsCaseInsensitive(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
                if (char.ToLowerInvariant(a[i]) != char.ToLowerInvariant(b[i]))
                    return false;

            return true;
        }
    }
}
