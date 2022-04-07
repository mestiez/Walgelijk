namespace Walgelijk;

/// <summary>
/// Utility struct full of easing curve functions
/// </summary>
public struct Easings
{
    /// <summary>
    /// Cubic easing functions
    /// </summary>
    public struct Cubic
    {
        /// <summary>
        /// Remaps a linear value from 0 to 1 to an ease-in ease-out cubic curve
        /// </summary>
        public static float InOut(float x)
        {
            float a = -2 * x + 2;
            return x < 0.5f ? 4 * x * x * x : 1 - (a * a * a) / 2;
        }

        /// <summary>
        /// Remaps a linear value from 0 to 1 to an ease-in cubic curve
        /// </summary>
        public static float In(float x) => x * x * x;

        /// <summary>
        /// Remaps a linear value from 0 to 1 to an ease-out cubic curve
        /// </summary>
        public static float Out(float x) => 1 - (1 - x) * (1 - x) * (1 - x);
    }

    /// <summary>
    /// Quadtratic easing functions
    /// </summary>
    public struct Quad
    {
        /// <summary>
        /// Remaps a linear value from 0 to 1 to an ease-in ease-out quadractic curve
        /// </summary>
        public static float InOut(float x)
        {
            float a = -2 * x + 2;
            return x < 0.5f ? 2 * x * x : 1 - (a * a) / 2;
        }

        /// <summary>
        /// Remaps a linear value from 0 to 1 to an ease-in quadractic curve
        /// </summary>
        public static float In(float x) => x * x;

        /// <summary>
        /// Remaps a linear value from 0 to 1 to an ease-out quadractic curve
        /// </summary>
        public static float Out(float x) => 1 - (1 - x) * (1 - x);
    }
}
