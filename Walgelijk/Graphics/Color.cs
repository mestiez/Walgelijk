using System;
using System.Data.Common;

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
        /// Red
        /// </summary>
        public static Color Red => new Color("#AE1C28");
        /// <summary>
        /// White
        /// </summary>
        public static Color White => new Color(1f, 1f, 1f);
        /// <summary>
        /// Blue
        /// </summary>
        public static Color Blue => new Color("#21468B");        
        /// <summary>
        /// Orange
        /// </summary>
        public static Color Orange => new Color("#FF4F00");

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
    }
}
