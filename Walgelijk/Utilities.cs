using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Walgelijk
{
    public struct Utilities
    {
        private static readonly Random rand = new Random();

        public const float RadToDeg = 180f / MathF.PI;
        public const float DegToRad = MathF.PI / 180f;

        /// <summary>
        /// Linearly interpolate between two floats
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return a * (1 - t) + b * t;
        }

        /// <summary>
        /// Linearly interpolate between two floats
        /// </summary>
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(
                Lerp(a.X, b.X, t),
                Lerp(a.Y, b.Y, t)
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
        /// Returns a random int in a range
        /// </summary>
        public static int RandomInt(int min = 0, int max = 100)
        {
            return rand.Next(min, max);
        }

        /// <summary>
        /// Returns a random byte
        /// </summary>
        public static int RandomByte()
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
        /// Return a random entry in a <see cref="ICollection{T}"/>
        /// </summary>
        public static T PickRandom<T>(IList<T> collection)
        {
            return collection[RandomInt(0, collection.Count)];
        }
    }
}
