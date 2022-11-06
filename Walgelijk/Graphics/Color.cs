﻿using System;
using System.Numerics;

namespace Walgelijk;

/// <summary>
/// Colour with 4 floating point components ranging from 0-1
/// </summary>
public struct Color : IEquatable<Color>
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
    public Color(ReadOnlySpan<char> hex)
    {
        if (hex.StartsWith("#"))
            hex = hex[1..];
        else if (hex.StartsWith("0x"))
            hex = hex[2..];

        if (hex.Length < 6)
            throw new ArgumentException(hex.ToString() + " is not a valid hexadecimal representation of a colour");

        R = byte.Parse(hex[0..2], global::System.Globalization.NumberStyles.HexNumber) / 255f;
        G = byte.Parse(hex[2..4], global::System.Globalization.NumberStyles.HexNumber) / 255f;
        B = byte.Parse(hex[4..6], global::System.Globalization.NumberStyles.HexNumber) / 255f;

        if (hex.Length == 8)
            A = byte.Parse(hex[6..8], global::System.Globalization.NumberStyles.HexNumber) / 255f;
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
    /// Returns a copy of this colour with each value clamped between 0 and 1 
    /// </summary>
    /// <returns></returns>
    public Color Clamped() => new Color(Utilities.Clamp(R), Utilities.Clamp(G), Utilities.Clamp(B), Utilities.Clamp(A));

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

    /// <summary>
    /// Get HSV all of which are in range 0 to 1
    /// </summary>
    public void GetHsv(out float h, out float s, out float v)
    {
        float rabs, gabs, babs, rr, gg, bb, diff;
        h = 0;
        s = 0;
        v = 0;

        rabs = R;
        gabs = G;
        babs = B;
        v = MathF.Max(rabs, MathF.Max(gabs, babs));
        diff = v - MathF.Min(rabs, MathF.Min(gabs, babs));

        static float diffc(float c, float v, float diff) => (v - c) / 6f / diff + 1 / 2f;

        if (diff == 0)
            h = s = 0;
        else
        {
            s = diff / v;
            rr = diffc(rabs, v, diff);
            gg = diffc(gabs, v, diff);
            bb = diffc(babs, v, diff);

            if (rabs == v)
                h = bb - gg;
            else if (gabs == v)
                h = (1 / 3) + rr - bb;
            else if (babs == v)
                h = (2 / 3) + gg - rr;
            if (h < 0)
                h += 1;
            else if (h > 1)
                h -= 1;
        }
    }

    /// <summary>
    /// Get the perceived luminance
    /// </summary>
    public float GetLuminance() => (R + R + B + G + G + G) / 6f;

    /// <summary>
    /// Create an RGBA colour from the given HSV values in the range 0 to 1
    /// </summary>
    /// <returns></returns>
    public static Color FromHsv(float h, float s, float v, float alpha = 1)
    {
        h *= 360;

        static float f(float n, float h, float s, float v)
        {
            float k = (n + h / 60) % 6;
            return v - v * s * MathF.Max(MathF.Min(k, MathF.Min(4 - k, 1)), 0);
        }

        return new Vector4(f(5, h, s, v), f(3, h, s, v), f(1, h, s, v), alpha);
    }

    public override bool Equals(object? obj)
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

    public bool Equals(Color other)
    {
        return R == other.R &&
               G == other.G &&
               B == other.B &&
               A == other.A;
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.R == right.R &&
               left.G == right.G &&
               left.B == right.B &&
               left.A == right.A;
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

    public static implicit operator Color((float r, float g, float b, float a) value)
    {
        return new Color(value.r, value.g, value.b, value.a);
    }

    public static implicit operator (float r, float g, float b, float a)(Color value)
    {
        return (value.R, value.G, value.B, value.A);
    }

    public readonly Vector3 RRR => new(R, R, R);
    public readonly Vector3 RRG => new(R, R, G);
    public readonly Vector3 RRB => new(R, R, B);
    public readonly Vector3 RRA => new(R, R, A);
    public readonly Vector3 RGR => new(R, G, R);
    public readonly Vector3 RGG => new(R, G, G);
    public readonly Vector3 RGB => new(R, G, B);
    public readonly Vector3 RGA => new(R, G, A);
    public readonly Vector3 RBR => new(R, B, R);
    public readonly Vector3 RBG => new(R, B, G);
    public readonly Vector3 RBB => new(R, B, B);
    public readonly Vector3 RBA => new(R, B, A);
    public readonly Vector3 RAR => new(R, A, R);
    public readonly Vector3 RAG => new(R, A, G);
    public readonly Vector3 RAB => new(R, A, B);
    public readonly Vector3 RAA => new(R, A, A);
    public readonly Vector3 GRR => new(G, R, R);
    public readonly Vector3 GRG => new(G, R, G);
    public readonly Vector3 GRB => new(G, R, B);
    public readonly Vector3 GRA => new(G, R, A);
    public readonly Vector3 GGR => new(G, G, R);
    public readonly Vector3 GGG => new(G, G, G);
    public readonly Vector3 GGB => new(G, G, B);
    public readonly Vector3 GGA => new(G, G, A);
    public readonly Vector3 GBR => new(G, B, R);
    public readonly Vector3 GBG => new(G, B, G);
    public readonly Vector3 GBB => new(G, B, B);
    public readonly Vector3 GBA => new(G, B, A);
    public readonly Vector3 GAR => new(G, A, R);
    public readonly Vector3 GAG => new(G, A, G);
    public readonly Vector3 GAB => new(G, A, B);
    public readonly Vector3 GAA => new(G, A, A);
    public readonly Vector3 BRR => new(B, R, R);
    public readonly Vector3 BRG => new(B, R, G);
    public readonly Vector3 BRB => new(B, R, B);
    public readonly Vector3 BRA => new(B, R, A);
    public readonly Vector3 BGR => new(B, G, R);
    public readonly Vector3 BGG => new(B, G, G);
    public readonly Vector3 BGB => new(B, G, B);
    public readonly Vector3 BGA => new(B, G, A);
    public readonly Vector3 BBR => new(B, B, R);
    public readonly Vector3 BBG => new(B, B, G);
    public readonly Vector3 BBB => new(B, B, B);
    public readonly Vector3 BBA => new(B, B, A);
    public readonly Vector3 BAR => new(B, A, R);
    public readonly Vector3 BAG => new(B, A, G);
    public readonly Vector3 BAB => new(B, A, B);
    public readonly Vector3 BAA => new(B, A, A);
    public readonly Vector3 ARR => new(A, R, R);
    public readonly Vector3 ARG => new(A, R, G);
    public readonly Vector3 ARB => new(A, R, B);
    public readonly Vector3 ARA => new(A, R, A);
    public readonly Vector3 AGR => new(A, G, R);
    public readonly Vector3 AGG => new(A, G, G);
    public readonly Vector3 AGB => new(A, G, B);
    public readonly Vector3 AGA => new(A, G, A);
    public readonly Vector3 ABR => new(A, B, R);
    public readonly Vector3 ABG => new(A, B, G);
    public readonly Vector3 ABB => new(A, B, B);
    public readonly Vector3 ABA => new(A, B, A);
    public readonly Vector3 AAR => new(A, A, R);
    public readonly Vector3 AAG => new(A, A, G);
    public readonly Vector3 AAB => new(A, A, B);
    public readonly Vector3 AAA => new(A, A, A);
    public readonly Vector2 RR => new(R, R);
    public readonly Vector2 RG => new(R, G);
    public readonly Vector2 RB => new(R, B);
    public readonly Vector2 RA => new(R, A);
    public readonly Vector2 GR => new(G, R);
    public readonly Vector2 GG => new(G, G);
    public readonly Vector2 GB => new(G, B);
    public readonly Vector2 GA => new(G, A);
    public readonly Vector2 BR => new(B, R);
    public readonly Vector2 BG => new(B, G);
    public readonly Vector2 BB => new(B, B);
    public readonly Vector2 BA => new(B, A);
    public readonly Vector2 AR => new(A, R);
    public readonly Vector2 AG => new(A, G);
    public readonly Vector2 AB => new(A, B);
    public readonly Vector2 AA => new(A, A);
    public readonly Vector4 RRRR => new(R, R, R, R);
    public readonly Vector4 RRRG => new(R, R, R, G);
    public readonly Vector4 RRRB => new(R, R, R, B);
    public readonly Vector4 RRRA => new(R, R, R, A);
    public readonly Vector4 RRGR => new(R, R, G, R);
    public readonly Vector4 RRGG => new(R, R, G, G);
    public readonly Vector4 RRGB => new(R, R, G, B);
    public readonly Vector4 RRGA => new(R, R, G, A);
    public readonly Vector4 RRBR => new(R, R, B, R);
    public readonly Vector4 RRBG => new(R, R, B, G);
    public readonly Vector4 RRBB => new(R, R, B, B);
    public readonly Vector4 RRBA => new(R, R, B, A);
    public readonly Vector4 RRAR => new(R, R, A, R);
    public readonly Vector4 RRAG => new(R, R, A, G);
    public readonly Vector4 RRAB => new(R, R, A, B);
    public readonly Vector4 RRAA => new(R, R, A, A);
    public readonly Vector4 RGRR => new(R, G, R, R);
    public readonly Vector4 RGRG => new(R, G, R, G);
    public readonly Vector4 RGRB => new(R, G, R, B);
    public readonly Vector4 RGRA => new(R, G, R, A);
    public readonly Vector4 RGGR => new(R, G, G, R);
    public readonly Vector4 RGGG => new(R, G, G, G);
    public readonly Vector4 RGGB => new(R, G, G, B);
    public readonly Vector4 RGGA => new(R, G, G, A);
    public readonly Vector4 RGBR => new(R, G, B, R);
    public readonly Vector4 RGBG => new(R, G, B, G);
    public readonly Vector4 RGBB => new(R, G, B, B);
    public readonly Vector4 RGBA => new(R, G, B, A);
    public readonly Vector4 RGAR => new(R, G, A, R);
    public readonly Vector4 RGAG => new(R, G, A, G);
    public readonly Vector4 RGAB => new(R, G, A, B);
    public readonly Vector4 RGAA => new(R, G, A, A);
    public readonly Vector4 RBRR => new(R, B, R, R);
    public readonly Vector4 RBRG => new(R, B, R, G);
    public readonly Vector4 RBRB => new(R, B, R, B);
    public readonly Vector4 RBRA => new(R, B, R, A);
    public readonly Vector4 RBGR => new(R, B, G, R);
    public readonly Vector4 RBGG => new(R, B, G, G);
    public readonly Vector4 RBGB => new(R, B, G, B);
    public readonly Vector4 RBGA => new(R, B, G, A);
    public readonly Vector4 RBBR => new(R, B, B, R);
    public readonly Vector4 RBBG => new(R, B, B, G);
    public readonly Vector4 RBBB => new(R, B, B, B);
    public readonly Vector4 RBBA => new(R, B, B, A);
    public readonly Vector4 RBAR => new(R, B, A, R);
    public readonly Vector4 RBAG => new(R, B, A, G);
    public readonly Vector4 RBAB => new(R, B, A, B);
    public readonly Vector4 RBAA => new(R, B, A, A);
    public readonly Vector4 RARR => new(R, A, R, R);
    public readonly Vector4 RARG => new(R, A, R, G);
    public readonly Vector4 RARB => new(R, A, R, B);
    public readonly Vector4 RARA => new(R, A, R, A);
    public readonly Vector4 RAGR => new(R, A, G, R);
    public readonly Vector4 RAGG => new(R, A, G, G);
    public readonly Vector4 RAGB => new(R, A, G, B);
    public readonly Vector4 RAGA => new(R, A, G, A);
    public readonly Vector4 RABR => new(R, A, B, R);
    public readonly Vector4 RABG => new(R, A, B, G);
    public readonly Vector4 RABB => new(R, A, B, B);
    public readonly Vector4 RABA => new(R, A, B, A);
    public readonly Vector4 RAAR => new(R, A, A, R);
    public readonly Vector4 RAAG => new(R, A, A, G);
    public readonly Vector4 RAAB => new(R, A, A, B);
    public readonly Vector4 RAAA => new(R, A, A, A);
    public readonly Vector4 GRRR => new(G, R, R, R);
    public readonly Vector4 GRRG => new(G, R, R, G);
    public readonly Vector4 GRRB => new(G, R, R, B);
    public readonly Vector4 GRRA => new(G, R, R, A);
    public readonly Vector4 GRGR => new(G, R, G, R);
    public readonly Vector4 GRGG => new(G, R, G, G);
    public readonly Vector4 GRGB => new(G, R, G, B);
    public readonly Vector4 GRGA => new(G, R, G, A);
    public readonly Vector4 GRBR => new(G, R, B, R);
    public readonly Vector4 GRBG => new(G, R, B, G);
    public readonly Vector4 GRBB => new(G, R, B, B);
    public readonly Vector4 GRBA => new(G, R, B, A);
    public readonly Vector4 GRAR => new(G, R, A, R);
    public readonly Vector4 GRAG => new(G, R, A, G);
    public readonly Vector4 GRAB => new(G, R, A, B);
    public readonly Vector4 GRAA => new(G, R, A, A);
    public readonly Vector4 GGRR => new(G, G, R, R);
    public readonly Vector4 GGRG => new(G, G, R, G);
    public readonly Vector4 GGRB => new(G, G, R, B);
    public readonly Vector4 GGRA => new(G, G, R, A);
    public readonly Vector4 GGGR => new(G, G, G, R);
    public readonly Vector4 GGGG => new(G, G, G, G);
    public readonly Vector4 GGGB => new(G, G, G, B);
    public readonly Vector4 GGGA => new(G, G, G, A);
    public readonly Vector4 GGBR => new(G, G, B, R);
    public readonly Vector4 GGBG => new(G, G, B, G);
    public readonly Vector4 GGBB => new(G, G, B, B);
    public readonly Vector4 GGBA => new(G, G, B, A);
    public readonly Vector4 GGAR => new(G, G, A, R);
    public readonly Vector4 GGAG => new(G, G, A, G);
    public readonly Vector4 GGAB => new(G, G, A, B);
    public readonly Vector4 GGAA => new(G, G, A, A);
    public readonly Vector4 GBRR => new(G, B, R, R);
    public readonly Vector4 GBRG => new(G, B, R, G);
    public readonly Vector4 GBRB => new(G, B, R, B);
    public readonly Vector4 GBRA => new(G, B, R, A);
    public readonly Vector4 GBGR => new(G, B, G, R);
    public readonly Vector4 GBGG => new(G, B, G, G);
    public readonly Vector4 GBGB => new(G, B, G, B);
    public readonly Vector4 GBGA => new(G, B, G, A);
    public readonly Vector4 GBBR => new(G, B, B, R);
    public readonly Vector4 GBBG => new(G, B, B, G);
    public readonly Vector4 GBBB => new(G, B, B, B);
    public readonly Vector4 GBBA => new(G, B, B, A);
    public readonly Vector4 GBAR => new(G, B, A, R);
    public readonly Vector4 GBAG => new(G, B, A, G);
    public readonly Vector4 GBAB => new(G, B, A, B);
    public readonly Vector4 GBAA => new(G, B, A, A);
    public readonly Vector4 GARR => new(G, A, R, R);
    public readonly Vector4 GARG => new(G, A, R, G);
    public readonly Vector4 GARB => new(G, A, R, B);
    public readonly Vector4 GARA => new(G, A, R, A);
    public readonly Vector4 GAGR => new(G, A, G, R);
    public readonly Vector4 GAGG => new(G, A, G, G);
    public readonly Vector4 GAGB => new(G, A, G, B);
    public readonly Vector4 GAGA => new(G, A, G, A);
    public readonly Vector4 GABR => new(G, A, B, R);
    public readonly Vector4 GABG => new(G, A, B, G);
    public readonly Vector4 GABB => new(G, A, B, B);
    public readonly Vector4 GABA => new(G, A, B, A);
    public readonly Vector4 GAAR => new(G, A, A, R);
    public readonly Vector4 GAAG => new(G, A, A, G);
    public readonly Vector4 GAAB => new(G, A, A, B);
    public readonly Vector4 GAAA => new(G, A, A, A);
    public readonly Vector4 BRRR => new(B, R, R, R);
    public readonly Vector4 BRRG => new(B, R, R, G);
    public readonly Vector4 BRRB => new(B, R, R, B);
    public readonly Vector4 BRRA => new(B, R, R, A);
    public readonly Vector4 BRGR => new(B, R, G, R);
    public readonly Vector4 BRGG => new(B, R, G, G);
    public readonly Vector4 BRGB => new(B, R, G, B);
    public readonly Vector4 BRGA => new(B, R, G, A);
    public readonly Vector4 BRBR => new(B, R, B, R);
    public readonly Vector4 BRBG => new(B, R, B, G);
    public readonly Vector4 BRBB => new(B, R, B, B);
    public readonly Vector4 BRBA => new(B, R, B, A);
    public readonly Vector4 BRAR => new(B, R, A, R);
    public readonly Vector4 BRAG => new(B, R, A, G);
    public readonly Vector4 BRAB => new(B, R, A, B);
    public readonly Vector4 BRAA => new(B, R, A, A);
    public readonly Vector4 BGRR => new(B, G, R, R);
    public readonly Vector4 BGRG => new(B, G, R, G);
    public readonly Vector4 BGRB => new(B, G, R, B);
    public readonly Vector4 BGRA => new(B, G, R, A);
    public readonly Vector4 BGGR => new(B, G, G, R);
    public readonly Vector4 BGGG => new(B, G, G, G);
    public readonly Vector4 BGGB => new(B, G, G, B);
    public readonly Vector4 BGGA => new(B, G, G, A);
    public readonly Vector4 BGBR => new(B, G, B, R);
    public readonly Vector4 BGBG => new(B, G, B, G);
    public readonly Vector4 BGBB => new(B, G, B, B);
    public readonly Vector4 BGBA => new(B, G, B, A);
    public readonly Vector4 BGAR => new(B, G, A, R);
    public readonly Vector4 BGAG => new(B, G, A, G);
    public readonly Vector4 BGAB => new(B, G, A, B);
    public readonly Vector4 BGAA => new(B, G, A, A);
    public readonly Vector4 BBRR => new(B, B, R, R);
    public readonly Vector4 BBRG => new(B, B, R, G);
    public readonly Vector4 BBRB => new(B, B, R, B);
    public readonly Vector4 BBRA => new(B, B, R, A);
    public readonly Vector4 BBGR => new(B, B, G, R);
    public readonly Vector4 BBGG => new(B, B, G, G);
    public readonly Vector4 BBGB => new(B, B, G, B);
    public readonly Vector4 BBGA => new(B, B, G, A);
    public readonly Vector4 BBBR => new(B, B, B, R);
    public readonly Vector4 BBBG => new(B, B, B, G);
    public readonly Vector4 BBBB => new(B, B, B, B);
    public readonly Vector4 BBBA => new(B, B, B, A);
    public readonly Vector4 BBAR => new(B, B, A, R);
    public readonly Vector4 BBAG => new(B, B, A, G);
    public readonly Vector4 BBAB => new(B, B, A, B);
    public readonly Vector4 BBAA => new(B, B, A, A);
    public readonly Vector4 BARR => new(B, A, R, R);
    public readonly Vector4 BARG => new(B, A, R, G);
    public readonly Vector4 BARB => new(B, A, R, B);
    public readonly Vector4 BARA => new(B, A, R, A);
    public readonly Vector4 BAGR => new(B, A, G, R);
    public readonly Vector4 BAGG => new(B, A, G, G);
    public readonly Vector4 BAGB => new(B, A, G, B);
    public readonly Vector4 BAGA => new(B, A, G, A);
    public readonly Vector4 BABR => new(B, A, B, R);
    public readonly Vector4 BABG => new(B, A, B, G);
    public readonly Vector4 BABB => new(B, A, B, B);
    public readonly Vector4 BABA => new(B, A, B, A);
    public readonly Vector4 BAAR => new(B, A, A, R);
    public readonly Vector4 BAAG => new(B, A, A, G);
    public readonly Vector4 BAAB => new(B, A, A, B);
    public readonly Vector4 BAAA => new(B, A, A, A);
    public readonly Vector4 ARRR => new(A, R, R, R);
    public readonly Vector4 ARRG => new(A, R, R, G);
    public readonly Vector4 ARRB => new(A, R, R, B);
    public readonly Vector4 ARRA => new(A, R, R, A);
    public readonly Vector4 ARGR => new(A, R, G, R);
    public readonly Vector4 ARGG => new(A, R, G, G);
    public readonly Vector4 ARGB => new(A, R, G, B);
    public readonly Vector4 ARGA => new(A, R, G, A);
    public readonly Vector4 ARBR => new(A, R, B, R);
    public readonly Vector4 ARBG => new(A, R, B, G);
    public readonly Vector4 ARBB => new(A, R, B, B);
    public readonly Vector4 ARBA => new(A, R, B, A);
    public readonly Vector4 ARAR => new(A, R, A, R);
    public readonly Vector4 ARAG => new(A, R, A, G);
    public readonly Vector4 ARAB => new(A, R, A, B);
    public readonly Vector4 ARAA => new(A, R, A, A);
    public readonly Vector4 AGRR => new(A, G, R, R);
    public readonly Vector4 AGRG => new(A, G, R, G);
    public readonly Vector4 AGRB => new(A, G, R, B);
    public readonly Vector4 AGRA => new(A, G, R, A);
    public readonly Vector4 AGGR => new(A, G, G, R);
    public readonly Vector4 AGGG => new(A, G, G, G);
    public readonly Vector4 AGGB => new(A, G, G, B);
    public readonly Vector4 AGGA => new(A, G, G, A);
    public readonly Vector4 AGBR => new(A, G, B, R);
    public readonly Vector4 AGBG => new(A, G, B, G);
    public readonly Vector4 AGBB => new(A, G, B, B);
    public readonly Vector4 AGBA => new(A, G, B, A);
    public readonly Vector4 AGAR => new(A, G, A, R);
    public readonly Vector4 AGAG => new(A, G, A, G);
    public readonly Vector4 AGAB => new(A, G, A, B);
    public readonly Vector4 AGAA => new(A, G, A, A);
    public readonly Vector4 ABRR => new(A, B, R, R);
    public readonly Vector4 ABRG => new(A, B, R, G);
    public readonly Vector4 ABRB => new(A, B, R, B);
    public readonly Vector4 ABRA => new(A, B, R, A);
    public readonly Vector4 ABGR => new(A, B, G, R);
    public readonly Vector4 ABGG => new(A, B, G, G);
    public readonly Vector4 ABGB => new(A, B, G, B);
    public readonly Vector4 ABGA => new(A, B, G, A);
    public readonly Vector4 ABBR => new(A, B, B, R);
    public readonly Vector4 ABBG => new(A, B, B, G);
    public readonly Vector4 ABBB => new(A, B, B, B);
    public readonly Vector4 ABBA => new(A, B, B, A);
    public readonly Vector4 ABAR => new(A, B, A, R);
    public readonly Vector4 ABAG => new(A, B, A, G);
    public readonly Vector4 ABAB => new(A, B, A, B);
    public readonly Vector4 ABAA => new(A, B, A, A);
    public readonly Vector4 AARR => new(A, A, R, R);
    public readonly Vector4 AARG => new(A, A, R, G);
    public readonly Vector4 AARB => new(A, A, R, B);
    public readonly Vector4 AARA => new(A, A, R, A);
    public readonly Vector4 AAGR => new(A, A, G, R);
    public readonly Vector4 AAGG => new(A, A, G, G);
    public readonly Vector4 AAGB => new(A, A, G, B);
    public readonly Vector4 AAGA => new(A, A, G, A);
    public readonly Vector4 AABR => new(A, A, B, R);
    public readonly Vector4 AABG => new(A, A, B, G);
    public readonly Vector4 AABB => new(A, A, B, B);
    public readonly Vector4 AABA => new(A, A, B, A);
    public readonly Vector4 AAAR => new(A, A, A, R);
    public readonly Vector4 AAAG => new(A, A, A, G);
    public readonly Vector4 AAAB => new(A, A, A, B);
    public readonly Vector4 AAAA => new(A, A, A, A);
}
