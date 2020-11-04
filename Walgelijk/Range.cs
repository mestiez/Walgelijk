using System.Numerics;

namespace Walgelijk
{

    /// <summary>
    /// Object with a minimum and maximum value
    /// </summary>
    public abstract class Range<T>
    {
        /// <summary>
        /// Minimum bound
        /// </summary>
        public T Min { get; set; }
        /// <summary>
        /// Maximum bound
        /// </summary>
        public T Max { get; set; }

        /// <summary>
        /// Construct a <see cref="Range{T}"/> with the given range values
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public Range(T min, T max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Construct a <see cref="Range{T}"/> where both bounds are identical
        /// </summary>
        /// <param name="val"></param>
        public Range(T val)
        {
            Min = val;
            Max = val;
        }

        /// <summary>
        /// Get a random number between <see cref="Min"/> and <see cref="Max"/>
        /// </summary>
        public abstract T GetRandom();

        /// <summary>
        /// Returns true if a value is within the range, and false otherwise
        /// </summary>
        public abstract bool IsInRange(T v);

        /// <summary>
        /// Clamps the given value within the range
        /// </summary>
        public abstract T Clamp(T v);
    }

    /// <summary>
    /// Struct with a minimum and maximum float value
    /// </summary>
    public class FloatRange : Range<float>
    {
        /// <summary>
        /// Construct a float range
        /// </summary>
        public FloatRange(float min, float max) : base(min, max)
        {
        }

        /// <summary>
        /// Construct a float range
        /// </summary>
        public FloatRange(float val) : base(val)
        {
        }

        public override float GetRandom()
        {
            return Utilities.RandomFloat(Min, Max);
        }

        public override bool IsInRange(float v)
        {
            return v >= Min && v <= Max;
        }

        public override float Clamp(float v)
        {
            return Utilities.Clamp(v, Min, Max);
        }
    }
    

    /// <summary>
    /// Struct with a minimum and maximum integer value
    /// </summary>
    public class IntRange : Range<int>
    {
        /// <summary>
        /// Construct an integer range
        /// </summary>
        public IntRange(int min, int max) : base(min, max)
        {
        }

        /// <summary>
        /// Construct an integer range
        /// </summary>
        public IntRange(int val) : base(val)
        {
        }

        public override int GetRandom()
        {
            return Utilities.RandomInt(Min, Max);
        }

        public override bool IsInRange(int v)
        {
            return v >= Min && v <= Max;
        }

        public override int Clamp(int v)
        {
            return Utilities.Clamp(v, Min, Max);
        }
    }

    /// <summary>
    /// Struct with a minimum and maximum <see cref="Vector2"/> value
    /// </summary>
    public class Vec2Range : Range<Vector2>
    {
        /// <summary>
        /// Construct a <see cref="Vector2"/> range
        /// </summary>
        public Vec2Range(Vector2 min, Vector2 max) : base(min, max)
        {
        }

        /// <summary>
        /// Construct a <see cref="Vector2"/>  range
        /// </summary>
        public Vec2Range(Vector2 val) : base(val)
        {
        }

        public override Vector2 GetRandom()
        {
            return new Vector2(
                Utilities.RandomFloat(Min.X, Max.X),
                Utilities.RandomFloat(Min.Y, Max.Y)
                );
        }

        public override bool IsInRange(Vector2 v)
        {
            return 
                v.X >= Min.X && 
                v.Y >= Min.Y && 
                v.X <= Max.X && 
                v.Y < Max.Y;
        }

        public override Vector2 Clamp(Vector2 v)
        {
            return new Vector2(
                Utilities.Clamp(v.X, Min.X, Max.X),
                Utilities.Clamp(v.Y, Min.Y, Max.Y)
                );
        }
    }

    /// <summary>
    /// Struct with a minimum and maximum <see cref="Color"/> value
    /// </summary>
    public class ColorRange : Range<Color>
    {
        /// <summary>
        /// Construct a <see cref="Vector2"/> range
        /// </summary>
        public ColorRange(Color min, Color max) : base(min, max)
        {
        }

        /// <summary>
        /// Construct a <see cref="Vector2"/>  range
        /// </summary>
        public ColorRange(Color val) : base(val)
        {
        }

        public override Color GetRandom()
        {
            return Utilities.RandomColour();
        }

        public override bool IsInRange(Color v)
        {
            return v >= Min && v <= Max;
        }

        public override Color Clamp(Color v)
        {
            return new Color(
                Utilities.Clamp(v.R, Min.R, Max.R),
                Utilities.Clamp(v.G, Min.G, Max.G),
                Utilities.Clamp(v.B, Min.B, Max.B),
                Utilities.Clamp(v.A, Min.A, Max.A)
                );
        }
    }
}
