using Walgelijk.Auburn;

namespace Walgelijk
{
    /// <summary>
    /// Noise utility struct that uses <see cref="FastNoiseLite"/> (<u>https://github.com/Auburn/FastNoise</u>) to generate noise
    /// </summary>
    public struct Noise
    {
        private static readonly FastNoiseLite CubicValue = new();
        private static readonly FastNoiseLite Perlin = new();
        private static readonly FastNoiseLite Simplex = new();
        private static readonly FastNoiseLite Cellular = new();
        private static readonly FastNoiseLite Fractal = new();

        static Noise()
        {
            CubicValue.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
            Perlin.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            Simplex.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
            Cellular.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            Fractal.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

            const float freq = 20;
            CubicValue.SetFrequency(freq);
            Perlin.SetFrequency(freq);
            Simplex.SetFrequency(freq);
            Cellular.SetFrequency(freq);
            Fractal.SetFrequency(freq);

            Fractal.SetFractalType(FastNoiseLite.FractalType.FBm);
        }

        /// <summary>
        /// Get value noise with cubic interpolation
        /// </summary>
        public static float GetValue(float x, float y, float z) => CubicValue.GetNoise(x, y, z);

        /// <summary>
        /// Get Perlin noise
        /// </summary>
        public static float GetPerlin(float x, float y, float z) => Perlin.GetNoise(x, y, z);

        /// <summary>
        /// Get OpenSimplex noise
        /// </summary>
        public static float GetSimplex(float x, float y, float z) => Simplex.GetNoise(x, y, z);

        /// <summary>
        /// Get cellular noise
        /// </summary>
        public static float GetCellular(float x, float y, float z) => Cellular.GetNoise(x, y, z);

        /// <summary>
        /// Get fractal simplex noise
        /// </summary>
        public static float GetFractal(float x, float y, float z, int octaves)
        {
            Fractal.SetFractalOctaves(octaves);
            return Fractal.GetNoise(x, y, z);
        }
    }
}
