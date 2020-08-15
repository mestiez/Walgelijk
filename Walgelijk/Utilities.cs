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
        public static int RandomInt(int min = 0, int max = 1)
        {
            return rand.Next(min, max);
        }

        /// <summary>
        /// Returns a random int in a range
        /// </summary>
        public static int RandomByte(byte min = 0, byte max = 1)
        {
            return (byte)rand.Next(0, 256);
        }
    }
}
