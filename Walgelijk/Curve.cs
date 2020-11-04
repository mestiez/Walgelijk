using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Linear curve
    /// </summary>
    public abstract class Curve<T>
    {
        /// <summary>
        /// Value keys
        /// </summary>
        public Key[] Keys { get; set; }

        /// <summary>
        /// Construct a curve
        /// </summary>
        /// <param name="keys"></param>
        protected Curve(params Key[] keys)
        {
            Keys = keys;
        }

        /// <summary>
        /// Get value at a position in the curve using linear interpolation
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public T Evaluate(float t)
        {
            if (Keys.Length == 0) return default;
            if (Keys.Length == 1) return Keys[0].Value;

            Key a = null;
            Key b = null;

            float lowest = float.MaxValue;

            for (int i = 0; i < Keys.Length; i++)
            {
                Key key = Keys[i];

                if (key.Position < t)
                    a = key;
                else if (key.Position < lowest)
                {
                    lowest = key.Position;
                    b = key;
                }
            }

            if (a == null || b == null)
                return default;

            float mapped = Utilities.MapRange(a.Position, b.Position, 0, 1, t);

            return Lerp(a.Value, b.Value, mapped);
        }

        /// <summary>
        /// Linear interpolation implementation for type <typeparamref name="T"/>
        /// </summary>
        protected abstract T Lerp(T a, T b, float t);

        /// <summary>
        /// Key with a position and value of type <typeparamref name="T"/>
        /// </summary>
        public class Key
        {
            /// <summary>
            /// Value
            /// </summary>
            public T Value;
            /// <summary>
            /// Position in range 0, 1
            /// </summary>
            public float Position;

            /// <summary>
            /// Construct a Key
            /// </summary>
            public Key(T value, float position)
            {
                Value = value;
                Position = position;
            }
        }
    }

    /// <summary>
    /// <see cref="float"/> curve
    /// </summary>
    public class FloatCurve : Curve<float>
    {
        public FloatCurve(params Key[] keys) : base(keys)
        {
        }

        protected override float Lerp(float a, float b, float t)
        {
            return Utilities.Lerp(a, b, t);
        }
    }

    /// <summary>
    /// <see cref="Color"/> curve
    /// </summary>
    public class ColorCurve : Curve<Color>
    {
        public ColorCurve(params Key[] keys) : base(keys)
        {
        }

        protected override Color Lerp(Color a, Color b, float t)
        {
            return Utilities.Lerp(a, b, t);
        }
    }

    /// <summary>
    /// <see cref="Vector4"/> curve
    /// </summary>
    public class Vec4Curve : Curve<Vector4>
    {
        public Vec4Curve(params Key[] keys) : base(keys)
        {
        }

        protected override Vector4 Lerp(Vector4 a, Vector4 b, float t)
        {
            return Utilities.Lerp(a, b, t);
        }
    }

    /// <summary>
    /// <see cref="Vector2"/> curve
    /// </summary>
    public class Vec2Curve : Curve<Vector2>
    {
        public Vec2Curve(params Key[] keys) : base(keys)
        {
        }

        protected override Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return Utilities.Lerp(a, b, t);
        }
    }

    /// <summary>
    /// <see cref="Vector3"/> curve
    /// </summary>
    public class Vec3Curve : Curve<Vector3>
    {
        public Vec3Curve(params Key[] keys) : base(keys)
        {
        }

        protected override Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return Utilities.Lerp(a, b, t);
        }
    }
}
