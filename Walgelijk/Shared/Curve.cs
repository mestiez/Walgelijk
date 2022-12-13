using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Linear curve
/// </summary>
public abstract class Curve<T> where T : notnull
{
    /// <summary>
    /// Value keys
    /// </summary>
    public Key[] Keys { get; set; }

    /// <summary>
    /// Mapping function of the input time value. Regarded as linear if null.
    /// </summary>
    public Func<float, float>? MappingFunction = null;

    /// <summary>
    /// Construct a curve and sort the given keys
    /// </summary>
    protected Curve(params Key[] keys)
    {
        Keys = keys;
        Array.Sort(Keys);
    }

    /// <summary>
    /// Get value at a position in the curve using linear interpolation
    /// </summary>
    public T Evaluate(float t)
    {
        if (Keys == null || Keys.Length == 0)
            return default; //null?? niet waar sukkel

        if (Keys.Length == 1)
            return Keys[0].Value;

        Key? previous = null;
        Key? next = null;
#if false //Ongesorteerd

        float highest = float.MinValue;
        float lowest = float.MaxValue;

        for (int i = 0; i < Keys.Length; i++)
        {
            Key key = Keys[i];

            //Als t zo goed als gelijk is aan de positie van de key, stuur gewoon de key value terug.
            if (MathF.Abs(t - key.Position) <= float.Epsilon)
                return key.Value;

            //Kijk wat de hoogste positie is onder t, en de laagste boven t
            if (key.Position <= t)
            {
                if (key.Position > highest)
                {
                    highest = key.Position;
                    previous = key;
                }
            }
            else if (key.Position < lowest)
            {
                lowest = key.Position;
                next = key;
            }
        }
#else //Al gesorteerd
        if (Keys[0].Position > t)
            return Keys[0].Value;

        if (Keys[^1].Position < t)
            return Keys[^1].Value;

        for (int i = 0; i < Keys.Length; i++)
        {
            var key = Keys[i];

            if (MathF.Abs(t - key.Position) <= float.Epsilon)
                return key.Value;

            if (key.Position <= t)
            {
                previous = key;
                next = key;
            }
            else if (key.Position >= t)
            {
                next = key;
                break;
            }
        }
#endif

        if (previous != null && next == null)
            return previous.Value;

        if (previous == null && next != null)
            return next.Value;

        if (previous == null || next == null)
            return default;

        float mapped = Utilities.MapRange(previous.Position, next.Position, 0, 1, t);

        if (MappingFunction != null)
            mapped = MappingFunction(mapped);

        return Lerp(previous.Value, next.Value, mapped);
    }

    /// <summary>
    /// Linear interpolation implementation for type <typeparamref name="T"/>
    /// </summary>
    protected abstract T Lerp(T a, T b, float t);

    /// <summary>
    /// Key with a position and value of type <typeparamref name="T"/>
    /// </summary>
    public class Key : IComparable<Key>
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

        public int CompareTo(Key? other) => (int)(Position * 1000) - (int)((other?.Position ?? 0) * 1000);
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
