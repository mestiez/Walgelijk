using System;
using System.Numerics;

namespace Walgelijk.Imgui
{
    public struct ImguiUtils
    {
        public static int CountVisibleCharacters(ReadOnlySpan<char> text)
        {
            int c = 0;
            for (int i = 0; i < text.Length; i++)
                if (!char.IsControl(text[i]))
                    c++;
            return c;
        }

        /// <summary>
        /// Makes sure the given size vector is not negative, NaN, or infinite
        /// </summary>
        public static Vector2 NormaliseSize(Vector2 v)
        {
            v.X = Utilities.NanFallback(v.X);
            v.Y = Utilities.NanFallback(v.Y);

            if (float.IsInfinity(v.X))
                v.X = 0;
            if (float.IsInfinity(v.Y))
                v.Y = 0;

            if (v.X < 0)
                v.X = 0;
            if (v.Y < 0)
                v.Y = 0;

            return v;
        }

        /// <summary>
        /// Removes fractional component and gets rid of Infinity and NaN
        /// </summary>
        public static Vector2 NormalisePosition(Vector2 v, bool floor = true)
        {
            v.X = Utilities.NanFallback(v.X);
            v.Y = Utilities.NanFallback(v.Y);

            if (float.IsInfinity(v.X))
                v.X = 0;
            if (float.IsInfinity(v.Y))
                v.Y = 0;

            if (floor)
            {
                v.X = (int)v.X;
                v.Y = (int)v.Y;
            }

            return v;
        }

        public static bool CharPassesFilter(char c, TextInputMode mode)
        {
            if (char.IsControl(c))
                return true;

            switch (mode)
            {
                default:
                case TextInputMode.Password or TextInputMode.Password:
                    return true;
                case TextInputMode.Alphanumeric:
                    return (c == ' ' || c == '.' || c == '_' || c == ',' || c == '-' || char.IsLetterOrDigit(c)) && c <= 255;
                case TextInputMode.Decimals:
                    return c == '.' || c == ',' || c == '-' || char.IsDigit(c);
                case TextInputMode.Integers:
                    return char.IsDigit(c) || c == '-';
                case TextInputMode.HexadecimalColourCode:
                    c = char.ToUpper(c);
                    return char.IsDigit(c) || c == '#' || c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F';
            }
        }
    }
}
