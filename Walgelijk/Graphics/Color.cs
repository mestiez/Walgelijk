using System;
using System.Data.Common;
using System.Numerics;

namespace Walgelijk
{
    /// <summary>
    /// Colour with 4 floating point components ranging from 0-1
    /// </summary>
    public struct Color
    {
        /// <summary>
        /// Size of an instance of this struct in bytes
        /// </summary>
        public const int Stride = sizeof(float) * 4;

        /// <summary>
        /// Red component
        /// </summary>
        public float R;
        /// <summary>
        /// Green component
        /// </summary>
        public float G;
        /// <summary>
        /// Blue component
        /// </summary>
        public float B;
        /// <summary>
        /// Alpha component
        /// </summary>
        public float A;

        /// <summary>
        /// Create a colour using floating point values ranging from 0 to 1 in standard dynamic range
        /// </summary>
        public Color(float r, float g, float b, float a = 1)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Create a colour using bytes where 0 is 0.0 and 255 is 1.0
        /// </summary>
        public Color(byte r, byte g, byte b, byte a = 255)
        {
            R = r / 255f;
            G = g / 255f;
            B = b / 255f;
            A = a / 255f;
        }

        /// <summary>
        /// Create a colour based on a hexadecimal representation, such as "#d42c5e"
        /// </summary>
        public Color(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex.Substring(1);
            else if (hex.StartsWith("0x"))
                hex = hex.Substring(2);

            if (hex.Length < 6)
                throw new ArgumentException(hex + " is not a valid hexadecimal representation of a colour");

            R = byte.Parse(hex.Substring(0, 2), global::System.Globalization.NumberStyles.HexNumber) / 255f;
            G = byte.Parse(hex.Substring(2, 2), global::System.Globalization.NumberStyles.HexNumber) / 255f;
            B = byte.Parse(hex.Substring(4, 2), global::System.Globalization.NumberStyles.HexNumber) / 255f;

            if (hex.Length == 4 * 2)
                A = byte.Parse(hex.Substring(6, 2), global::System.Globalization.NumberStyles.HexNumber) / 255f;
            else
                A = 1;
        }

        /// <summary>
        /// Bright vermilion
        /// </summary>
        public static Color Red => new Color("#AE1C28");
        /// <summary>
        /// White
        /// </summary>
        public static Color White => new Color(1f, 1f, 1f);
        /// <summary>
        /// Cobalt blue
        /// </summary>
        public static Color Blue => new Color("#21468B");
        /// <summary>
        /// Orange
        /// </summary>
        public static Color Orange => new Color("#FF4F00");

        /// <summary>
        /// Return a copy of the colour with the given alpha
        /// </summary>
        public Color WithAlpha(float alpha) => new Color(R, G, B, alpha);

        /// <summary>
        /// Returns a tuple where each element corresponds with a component of the colour
        /// </summary>
        public (byte r, byte g, byte b, byte a) ToBytes()
        {
            byte r = (byte)(Utilities.Clamp(R) * 255);
            byte g = (byte)(Utilities.Clamp(G) * 255);
            byte b = (byte)(Utilities.Clamp(B) * 255);
            byte a = (byte)(Utilities.Clamp(A) * 255);

            return (r, g, b, a);
        }

        public override string ToString()
        {
            return $"({R},{G},{B},{A})";
        }

        public override bool Equals(object obj)
        {
            return obj is Color color &&
                   R == color.R &&
                   G == color.G &&
                   B == color.B &&
                   A == color.A;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(R, G, B, A);
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !(left == right);
        }

        public static Color operator *(Color left, Color right)
        {
            return new Color(
                left.R * right.R,
                left.G * right.G,
                left.B * right.B,
                left.A * right.A
                );
        }        
        
        public static Color operator *(float v, Color c)
        {
            return new Color(
                c.R * v,
                c.G * v,
                c.B * v,
                c.A * v
                );
        } 
        
        public static Color operator *(Color c, float v)
        {
            return new Color(
                c.R * v,
                c.G * v,
                c.B * v,
                c.A * v
                );
        }

        public static bool operator >(Color left, Color right)
        {
            return
                left.R > right.R &&
                left.G > right.G &&
                left.B > right.B &&
                left.A > right.A;
        }

        public static bool operator <(Color left, Color right)
        {
            return
                left.R < right.R &&
                left.G < right.G &&
                left.B < right.B &&
                left.A < right.A;
        }
        public static bool operator >=(Color left, Color right)
        {
            return
                left.R >= right.R &&
                left.G >= right.G &&
                left.B >= right.B &&
                left.A >= right.A;
        }

        public static bool operator <=(Color left, Color right)
        {
            return
                left.R <= right.R &&
                left.G <= right.G &&
                left.B <= right.B &&
                left.A <= right.A;
        }

        public static implicit operator Color(Vector4 value)
        {
            return new Color(value.X, value.Y, value.Z, value.W);
        }

        public static implicit operator Vector4(Color value)
        {
            return new Vector4(value.R, value.G, value.B, value.A);
        }

        public static implicit operator Color((float r,float g,float b,float a) value)
        {
            return new Color(value.r, value.g, value.b, value.a);
        }

        public static implicit operator (float r, float g, float b, float a)(Color value)
        {
            return (value.R, value.G, value.B, value.A);
        }
    }
}
